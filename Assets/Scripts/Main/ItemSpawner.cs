using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemSpawner : MonoBehaviour
{
    private Tilemap tilemap;
    public GameObject[] itemPrefab;
    public GameObject[] enemyPrefab;

    private Transform player;
    private float lastDropY;

    public float dropInterval = 8f;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        tilemap = FindObjectOfType<Tilemap>();

        lastDropY = Mathf.Floor(player.position.y / dropInterval) * dropInterval;
    }

    void Update()
    {
        if (player == null || tilemap == null) return;

        float expectedDropY = lastDropY - dropInterval;

        if (player.position.y <= expectedDropY)
        {
            SpawnItemBelowScreen();
            lastDropY = expectedDropY;
        }
    }


    void SpawnItemBelowScreen()
    {
        Camera cam = Camera.main;
        float z = Mathf.Abs(cam.transform.position.z - tilemap.transform.position.z);

        float xPadding = 0.15f; // 좌우 10% 패딩 (원하면 더 줄이거나 늘릴 수 있음)

        Vector3 bottomLeftWorld = cam.ViewportToWorldPoint(new Vector3(0f + xPadding, -0.2f, z));
        Vector3 topRightWorld = cam.ViewportToWorldPoint(new Vector3(1f - xPadding, 0f, z));

        Vector3Int min = tilemap.WorldToCell(bottomLeftWorld);
        Vector3Int max = tilemap.WorldToCell(topRightWorld);

        List<Vector3Int> validTiles = new List<Vector3Int>();
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(tilePos))
                {
                    validTiles.Add(tilePos);
                }
            }
        }

        if (validTiles.Count > 0)
        {
            Vector3Int spawnTile = validTiles[Random.Range(0, validTiles.Count)];
            SpawnItemAtTile(spawnTile);
            Debug.Log($"[ItemSpawner] Spawned item at {spawnTile}");
        }
        else
        {
            Debug.Log("[ItemSpawner] No valid tile found to spawn item.");
        }
    }

    private int lastIndex = 6;
    void SpawnItemAtTile(Vector3Int tilePos)
    {
        Vector3 worldPos = tilemap.CellToWorld(tilePos) + tilemap.tileAnchor;

        int spawnRoll = Random.Range(0, 4); // 0~3 중 하나

        if (spawnRoll == 0 && enemyPrefab.Length > 0)
        {
            // 적 25% 확률 (4번 중 1번)
            Instantiate(enemyPrefab[Random.Range(0, enemyPrefab.Length)], worldPos, Quaternion.identity);
        }
        else if (itemPrefab.Length > 0)
        {
            // 나머지 75%는 아이템
            Instantiate(itemPrefab[Random.Range(0, itemPrefab.Length)], worldPos, Quaternion.identity);
            //int nextIndex = (lastIndex == 5) ? 6 : 5;
            //lastIndex = nextIndex;

            //Instantiate(itemPrefab[nextIndex], worldPos, Quaternion.identity);
        }
    }


}
