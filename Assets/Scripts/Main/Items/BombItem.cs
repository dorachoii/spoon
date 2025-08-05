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
    
    private SpriteColorEffect effector;

    protected override void Awake()
    {
        base.Awake();
        effector = gameObject.GetComponent<SpriteColorEffect>();
        if (effector != null)
        {
            StartCoroutine(effector.IFlicker(gameObject.GetComponent<SpriteRenderer>(), SpriteEffectColor.Red, -1));
        }
    }

    protected override void ApplyEffect(GameObject player)
    {
        ExplodeTiles();
        Damage(player);
        ShowStatusText("Damaged", Color.red);
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

    void Damage(GameObject player)
    {
        if (player == null) return;

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
                playerStat.DamageHP(dmg);
            }
        }
    }
}
