using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class LayerSpawnData
{
    public GameObject[] itemPrefabs;
    public GameObject[] enemyPrefabs;
    public int savePointCount;
}

public class ItemSpawner : MonoBehaviour
{
    private Tilemap tilemap;
    private Transform player;
    private float lastDropY;
    private bool isPlayerReady = false;

    public float dropInterval = 8f;

    [Header("Spawn Data")]
    public LayerSpawnData[] layerSpawnDatas;

    private int currentLayer = 0;
    public GameObject savePointPrefab;

    public GameObject breakableTilemap;
    public GameObject grid;
    
    void Start()
    {
        // 플레이어 준비 이벤트 구독
        GameManager.OnPlayerReady += OnPlayerReady;
        
        // Tilemap은 바로 설정 가능
        tilemap = TileGenerator.Instance.tilemap;
        
        // LayerManager 이벤트 구독
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForTilemapGeneration += HandleLayerChanged;
        }
    }
    
    private void OnPlayerReady()
    {
        isPlayerReady = true;
        
        // 플레이어가 준비되면 참조 설정
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            lastDropY = Mathf.Floor(player.position.y / dropInterval) * dropInterval;
            Debug.Log("[ItemSpawner] Player ready - item spawning initialized");
        }
    }

    void OnDestroy()
    {
        // 플레이어 준비 이벤트 구독 해제
        GameManager.OnPlayerReady -= OnPlayerReady;
        
        // LayerManager 이벤트 구독 해제
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForTilemapGeneration -= HandleLayerChanged;
        }
    }

    void Update()
    {
        // 플레이어가 준비되지 않았으면 처리하지 않음
        if (!isPlayerReady || player == null || tilemap == null) return;

        float expectedDropY = lastDropY - dropInterval;

        if (player.position.y <= expectedDropY)
        {
            SpawnItemBelowScreen();
            lastDropY = expectedDropY;
        }
    }

    private void HandleLayerChanged(int newLayer)
    {
        currentLayer = LayerManager.Instance.GetCurrentLayerTileIndex();
        SpawnSavePoints();
    }

    void SpawnItemBelowScreen()
    {
        LayerSpawnData data = layerSpawnDatas[currentLayer];
        if (data == null) return;

        Camera cam = Camera.main;
        float z = Mathf.Abs(cam.transform.position.z - tilemap.transform.position.z);

        float xPadding = 0.15f;

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
            SpawnItemAtTile(spawnTile, data);
        }
    }

    void SpawnItemAtTile(Vector3Int tilePos, LayerSpawnData data)
    {
        Vector3 worldPos = tilemap.CellToWorld(tilePos) + tilemap.tileAnchor;

        int spawnRoll = Random.Range(0, 4); // 0~3 중 하나

        if (spawnRoll == 0 && data.enemyPrefabs.Length > 0)
        {
            // 적 25% 확률 (4번 중 1번)
            Instantiate(data.enemyPrefabs[Random.Range(0, data.enemyPrefabs.Length)], worldPos, Quaternion.identity);
        }
        else if (data.itemPrefabs.Length > 0)
        {
            // 나머지 75%는 아이템
            Instantiate(data.itemPrefabs[Random.Range(0, data.itemPrefabs.Length)], worldPos, Quaternion.identity);
        }
    }

    void SpawnSavePoints()
    {
        LayerSpawnData data = layerSpawnDatas[currentLayer];
        if (savePointPrefab == null || data == null) return;

        int count = Random.Range(1, data.savePointCount + 1);

        Camera cam = Camera.main;
        float z = Mathf.Abs(cam.transform.position.z - tilemap.transform.position.z);

        Vector3 bottomLeftWorld = cam.ViewportToWorldPoint(new Vector3(0f, 0f, z));
        Vector3 topRightWorld = cam.ViewportToWorldPoint(new Vector3(1f, 1f, z));

        Vector3Int min = tilemap.WorldToCell(bottomLeftWorld);
        Vector3Int max = tilemap.WorldToCell(topRightWorld);

        List<Vector3Int> validTiles = new List<Vector3Int>();
        for (int x = min.x; x <= max.x; x++)
        {
            for (int y = min.y; y <= max.y; y++)
            {
                Vector3Int tilePos = new Vector3Int(x, y, 0);
                if (tilemap.HasTile(tilePos))
                    validTiles.Add(tilePos);
            }
        }

        for (int i = 0; i < count; i++)
        {
            if (validTiles.Count == 0) break;

            int idx = Random.Range(0, validTiles.Count);
            Vector3Int spawnTile = validTiles[idx];
            validTiles.RemoveAt(idx);

            Vector3 spawnPos = tilemap.CellToWorld(spawnTile) + tilemap.tileAnchor;
            Instantiate(savePointPrefab, spawnPos, Quaternion.identity);
        }
    }
}
