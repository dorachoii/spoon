using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class LayerSpawnData
{
    public GameObject[] itemPrefabs;
    public GameObject[] enemyPrefabs;
    public int savePointCount;
}

[System.Serializable]
public class BossLayerData
{
    public int layerIndex;
    public GameObject bossPrefab;
    public Vector3 offset;
}

public class ItemSpawner : MonoBehaviour
{
    private Tilemap tilemap;

    private Transform player;
    private float lastDropY;

    public float dropInterval = 8f;

    [Header("Spawn Data")]
    public LayerSpawnData[] layerSpawnDatas;

    [Header("Boss Spawn Data")]
    public List<BossLayerData> bossLayerDatas;

    private int currentLayer = 0;
    public GameObject savePointPrefab;

    public GameObject breakableTilemap;
    public GameObject grid;
    
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        tilemap = TileGenerator.Instance.tilemap;

        lastDropY = Mathf.Floor(player.position.y / dropInterval) * dropInterval;
        LayerManager.Instance.OnLayerChanged += HandleLayerChanged;
    }

    void OnDestroy()
    {
        LayerManager.Instance.OnLayerChanged -= HandleLayerChanged;
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

    private void HandleLayerChanged(int newLayer)
    {
        currentLayer = Mathf.Clamp(newLayer, 0, layerSpawnDatas.Length - 1);
        SpawnSavePoints();

        TrySpawnBossForLayer(newLayer);
    }

    private bool bossSpawned = false;

    void TrySpawnBossForLayer(int layerIndex)
{
    if (bossSpawned) return; // 이미 생성됐으면 무시

    foreach (var bossData in bossLayerDatas)
    {
        if (bossData.layerIndex == layerIndex && bossData.bossPrefab != null)
        {
            // 보스 위치 계산
            Vector3 spawnPos = GetBossSpawnPosition() + bossData.offset;

            // 1. 보스 생성
            Instantiate(bossData.bossPrefab, spawnPos, Quaternion.identity);
            bossSpawned = true;
            Debug.Log($"[ItemSpawner] Boss spawned on layer {layerIndex} at {spawnPos}");

            // 2. Breakable 타일맵 생성 (선택사항)
            if (breakableTilemap != null)
            {
                // 보스보다 살짝 위에 생성 (위치는 필요에 따라 조정)
                Vector3 tilemapPos = spawnPos + new Vector3(0, 2f, 0);

                GameObject tilemapObj = Instantiate(breakableTilemap, tilemapPos, Quaternion.identity);
                    tilemapObj.transform.SetParent(grid.transform);
                
            }

            break;
        }
    }
}


    Vector3 GetBossSpawnPosition()
    {
        Camera cam = Camera.main;
        float z = Mathf.Abs(cam.transform.position.z - tilemap.transform.position.z);

        Vector3 midWorld = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.2f, z)); // 화면 아래쪽 중앙

        Vector3Int tilePos = tilemap.WorldToCell(midWorld);
        Vector3 spawnPos = tilemap.CellToWorld(tilePos) + tilemap.tileAnchor;

        return spawnPos;
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

    private int lastIndex = 6;
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
