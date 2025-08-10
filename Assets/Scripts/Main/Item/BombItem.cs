using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombItem : ItemBase
{
    [Header("Bomb Settings")]
    [SerializeField] private int radius = 30;
    [SerializeField] private float force = 0.001f;
    [SerializeField] private float dmgRadius = 15f;
    [SerializeField] private float dmg = 20f;
    
    private SpriteColorEffector effector;

    private float autoExplodeDelay = 5f;

    protected override void Awake()
    {
        base.Awake();
        effector = gameObject.GetComponent<SpriteColorEffector>();
        if (effector != null)
        {
            StartCoroutine(effector.IFlicker(gameObject.GetComponent<SpriteRenderer>(), PlayerColor.Red, loop: true));
        }

        StartCoroutine(AutoExplode());
    }

    protected override void ApplyEffect(GameObject player)
    {
        ExplodeTiles();
        bool damageApplied = Damage(player);
        if (damageApplied)
        {
            ShowStatusText("Damaged", Color.red);
        }
    }

    void ExplodeTiles()
    {
        Vector3Int centerCell = tilemap.WorldToCell(transform.position);
        List<Vector3Int> tilesToClear = new List<Vector3Int>();
        
        // 빗변의 제곱 (斜辺の二乗)
        float radiusSq = radius * radius;

        // 지름 내에 포함되면 제거 (直径内に入ったら削除対象として追加)
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector3Int pos = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);
                float distSq = x * x + y * y;
                if (distSq <= radiusSq && tilemap.HasTile(pos))
                {
                    tilesToClear.Add(pos);
                }
            }
        }

        // SetTiles로 한 번에 제거 (SetTilesで一括削除)
        if (tilesToClear.Count > 0)
        {
            Vector3Int[] positions = tilesToClear.ToArray();
            TileBase[] emptyTiles = new TileBase[positions.Length]; // 全てnull
            tilemap.SetTiles(positions, emptyTiles);
        }
    }

    bool Damage(GameObject player)
    {
        if (player == null) return false;

        Vector2 playerPos = player.transform.position;
        float dist = Vector2.Distance(playerPos, transform.position);
        
        // 데미지 범위 내라면 (範囲内に入ったら)
        if (dist <= dmgRadius)
        {
            // 플레이어 반동 (プレイヤー反動)
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (playerPos - (Vector2)transform.position).normalized;
                rb.AddForce(dir * force, ForceMode2D.Impulse);
            }
            
            // 플레이어 데미지 (プレイヤーダメージ) 
            PlayerStat playerStat = player.GetComponent<PlayerStat>();
            if (playerStat != null)
            {
                return playerStat.DamageHP(dmg);
            }
        }
        return false;
    }

    private IEnumerator AutoExplode()
    {
        yield return new WaitForSeconds(autoExplodeDelay);
        
        // 사운드와 이펙트 재생
        PlaySoundEffect();
        InstantiateFX();
        
        ExplodeTiles();
        
        // 자동 폭발 시에도 플레이어에게 데미지 주기
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            bool damageApplied = Damage(player);
            if (damageApplied)
            {
                ShowStatusText("Damaged", Color.red);
            }
        }
        
        Destroy(gameObject);
    }
}
