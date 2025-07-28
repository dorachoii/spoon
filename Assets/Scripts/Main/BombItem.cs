using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class BombItem : ItemBase
{
    private int explosionRadius = 30;
    private float explisionForce = 0.001f;
    private float damageRadius = 15f;
    private float damageAmount = 20f;

    protected override void ApplyEffect(GameObject player)
    {
        ExplodeTiles();
        Damage(player);
    }

    void ExplodeTiles()
    {
        Vector3Int centerCell = tilemap.WorldToCell(transform.position);

        List<Vector3Int> tilesToClear = new List<Vector3Int>();

        for (int x = -explosionRadius; x <= explosionRadius; x++)
        {
            for (int y = -explosionRadius; y <= explosionRadius; y++)
            {
                Vector3Int checkPos = new Vector3Int(centerCell.x + x, centerCell.y + y, centerCell.z);
                float dist = Mathf.Sqrt(x * x + y * y);
                if (dist <= explosionRadius && tilemap.HasTile(checkPos))
                {
                    tilesToClear.Add(checkPos);
                }
            }
        }

        Vector3Int[] positions = tilesToClear.ToArray();
        TileBase[] emptyTiles = new TileBase[positions.Length];

        tilemap.SetTiles(positions, emptyTiles);
    }

    void Damage(GameObject player)
    {
        if (player == null) return;

        Vector2 playerPos = player.transform.position;
        float dist = Vector2.Distance(playerPos, transform.position);

        if (dist <= damageRadius)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (playerPos - (Vector2)transform.position).normalized;
                rb.AddForce(direction * explisionForce, ForceMode2D.Impulse);
            }
        }
        
        PlayerStat playerStat = player.GetComponent<PlayerStat>();
        if (playerStat != null)
        {
            playerStat.TakeDamage(damageAmount);
        }
    }
}
