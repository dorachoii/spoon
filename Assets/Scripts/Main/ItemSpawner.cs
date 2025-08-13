using System.Collections;
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
    private bool isPlayerFound = false;

    public float dropInterval = 8f;

    [Header("Spawn Data")]
    public LayerSpawnData[] layerSpawnDatas;

    private int currentLayer = 0;
    public GameObject savePointPrefab;

    public GameObject breakableTilemap;
    public GameObject grid;
    
    // 세이브 포인트 생성 관련 변수
    private bool shouldSpawnSavePoint = false;
    private bool savePointSpawned = false;
    private float savePointSpawnDepth = 5f;
    
    void Start()
    {
        tilemap = TileGenerator.Instance.tilemap;
        
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForTilemapGeneration += HandleLayerChanged;
        }
        
        // 플레이어를 찾을 때까지 코루틴으로 대기
        StartCoroutine(FindPlayerCoroutine());
    }
    
    void OnDestroy()
    {
    
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForTilemapGeneration -= HandleLayerChanged;
        }
    }

    void Update()
    {
        if (!isPlayerFound || player == null || tilemap == null) return;

        float expectedDropY = lastDropY - dropInterval;

        if (player.position.y <= expectedDropY)
        {
            SpawnItemBelowScreen();
            lastDropY = expectedDropY;
        }
        
        CheckAndSpawnSavePoint();
    }

    
    private IEnumerator FindPlayerCoroutine()
    {
        while (player == null)
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
                lastDropY = Mathf.Floor(player.position.y / dropInterval) * dropInterval;
                isPlayerFound = true;
                break;
            }
            
            yield return null;
        }
    }


    private void HandleLayerChanged(int newLayer)
    {
        currentLayer = LayerManager.Instance.GetCurrentLayerTileIndex();
        
        if (currentLayer >= 0)
        {
            shouldSpawnSavePoint = true;
            savePointSpawned = false;
        }
    }

    void SpawnItemBelowScreen()
    {
        if (currentLayer < 0 || currentLayer >= layerSpawnDatas.Length) return;
        
        LayerSpawnData data = layerSpawnDatas[currentLayer];
        if (data == null) return;

        List<Vector3Int> validTiles = GetValidTilesBelowViewport();
        
        if (validTiles.Count > 0)
        {
            Vector3Int spawnTile = validTiles[Random.Range(0, validTiles.Count)];
            SpawnItemAtTile(spawnTile, data);
        }
    }

    void SpawnItemAtTile(Vector3Int tilePos, LayerSpawnData data)
    {
        Vector3 worldPos = tilemap.CellToWorld(tilePos) + tilemap.tileAnchor;

        int spawnRoll = Random.Range(0, 4);

        if (spawnRoll == 0 && data.enemyPrefabs.Length > 0)
        {
            Instantiate(data.enemyPrefabs[Random.Range(0, data.enemyPrefabs.Length)], worldPos, Quaternion.identity);
        }
        else if (data.itemPrefabs.Length > 0)
        {
            Instantiate(data.itemPrefabs[Random.Range(0, data.itemPrefabs.Length)], worldPos, Quaternion.identity);
        }
    }

    void CheckAndSpawnSavePoint()
    {
        if (!shouldSpawnSavePoint || savePointSpawned) return;
        
        float layerStartY = LayerManager.Instance.GetCurrentLayerStartY();
        float targetSpawnY = layerStartY - savePointSpawnDepth;
        Debug.Log($"savepoint: targetSpawnY: {targetSpawnY}, 현재 플레이어 위치: {player.position.y}, 현재 레이어 시작점: {layerStartY}");
        if (player.position.y <= targetSpawnY)
        {
            Debug.Log($"savepoint: 세이브 포인트 생성");
            SpawnSavePoints();
            savePointSpawned = true;
            shouldSpawnSavePoint = false;
        }
    }
    
    void SpawnSavePoints()
    {
        if (currentLayer < 0 || currentLayer >= layerSpawnDatas.Length) return;
        
        LayerSpawnData data = layerSpawnDatas[currentLayer];
        int count = Random.Range(1, data.savePointCount + 1);

        SpawnPrefabsInValidTiles(savePointPrefab, count);
    }

    void SpawnPrefabsInValidTiles(GameObject prefab, int count)
    {
        List<Vector3Int> validTiles = GetValidTilesBelowViewport();
        
        for (int i = 0; i < count; i++)
        {
            if (validTiles.Count == 0) break;

            int idx = Random.Range(0, validTiles.Count);
            Vector3Int spawnTile = validTiles[idx];
            validTiles.RemoveAt(idx);

            Vector3 spawnPos = tilemap.CellToWorld(spawnTile) + tilemap.tileAnchor;
            Instantiate(prefab, spawnPos, Quaternion.identity);
            Debug.Log($"savepoint: 세이브 포인트 생성");
        }
    }

    List<Vector3Int> GetValidTilesBelowViewport()
    {
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

        return validTiles;
    }
}
