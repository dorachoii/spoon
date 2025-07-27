using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemSpawner : MonoBehaviour
{
    public Tilemap tilemap;
    public GameObject itemPrefab;

    public Vector3Int areaBottomLeft = new Vector3Int(-10, -10, 0);
    public Vector3Int areaTopRight = new Vector3Int(10, 10, 0);

    void Start()
    {
        SpawnRandomItemInArea(areaBottomLeft, areaTopRight);
    }

    public void SpawnRandomItemInArea(Vector3Int bottomLeft, Vector3Int topRight)
    {
        List<Vector3Int> validTilePositions = new List<Vector3Int>();

        for (int x = bottomLeft.x; x <= topRight.x; x++)
        {
            for (int y = bottomLeft.y; y <= topRight.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(tilePos))
                {
                    validTilePositions.Add(tilePos);
                }
            }
        }

        if (validTilePositions.Count > 0)
        {
            Vector3Int randomTile = validTilePositions[Random.Range(0, validTilePositions.Count)];
            SpawnItemAtTile(randomTile);
        }
        else
        {
            Debug.LogWarning("No valid tile to spawn item!");
        }
    }


    public void SpawnItemAtTile(Vector3Int tilePos)
    {
        Vector3 worldPos = tilemap.CellToWorld(tilePos) + tilemap.tileAnchor;
        Instantiate(itemPrefab, worldPos, Quaternion.identity);
    }

    public void SpawnItemInArea(Vector3Int bottomLeft, Vector3Int topRight)
    {
        for (int x = bottomLeft.x; x <= topRight.x; x++)
        {
            for (int y = bottomLeft.y; y <= topRight.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(tilePos))
                {
                    SpawnItemAtTile(tilePos);
                }
            }
        }
    }
}
