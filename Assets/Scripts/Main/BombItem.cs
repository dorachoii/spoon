using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic;

public class BombItem : ItemBase
{
    private int explosionRadius = 30;  

    protected override void ApplyEffect(GameObject player)
    {
        Explode();
    }

    void Explode()
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
}
