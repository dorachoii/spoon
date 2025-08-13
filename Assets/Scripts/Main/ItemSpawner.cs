using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class LayerSpawnData
{
    [Header("Items")]
    public GameObject[] hpItemPrefabs;
    public GameObject[] powerItemPrefabs;
    
    [Header("Enemies")]
    public GameObject[] enemyPrefabs;

    private float dropInterval = 8f;
    
    private float enemyChance = 0.25f;
    private float hpItemChance = 0.35f;
    private float powerItemChance = 0.40f;

    // 확률 설정 메서드들
    public void SetChances(float enemyChance, float hpChance, float powerChance, float dropInterval)
    {
        this.enemyChance = Mathf.Clamp01(enemyChance);
        this.hpItemChance = Mathf.Clamp01(hpChance);
        this.powerItemChance = Mathf.Clamp01(powerChance);
        this.dropInterval = dropInterval;
    }
    
    public float GetEnemyChance() { return enemyChance; }
    public float GetHpItemChance() { return hpItemChance; }
    public float GetPowerItemChance() { return powerItemChance; }
    public float GetDropInterval() { return dropInterval; }
}

public class ItemSpawner : MonoBehaviour
{
    private Tilemap tilemap;
    private Transform player;
    private bool isPlayerFound = false;
    
    // 전역 변수들
    private Queue<SpawnItem> spawnQueue = new Queue<SpawnItem>();
    private float currentDropInterval;
    private float lastDropY;
    
    [Header("Spawn Data")]
    public LayerSpawnData[] layerSpawnDatas;

    private int currentLayer = 0;
    public GameObject savePointPrefab;

    public GameObject breakableTilemap;
    public GameObject grid;
    
    // savepoints
    private bool shouldSpawnSavePoint = false;
    private bool savePointSpawned = false;
    private float savePointSpawnDepth = 5f;
    
    #region Initialize
    void Start()
    {
        tilemap = TileGenerator.Instance.tilemap;
        
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForTilemapGeneration += HandleLayerChanged;
        }
        
        // 레이어별 데이터 초기화
        InitializeLayerData();
        
        // 플레이어를 찾을 때까지 코루틴으로 대기
        StartCoroutine(FindPlayerCoroutine());
    }
    
    void OnEnable()
    {
        // GameManager resume 이벤트 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameResumed += HandleGameResumed;
            Debug.Log("ItemSpawner: GameManager.OnGameResumed 이벤트 구독 완료");
        }
        else
        {
            Debug.LogWarning("ItemSpawner: GameManager.Instance가 null입니다!");
        }
    }
    
    void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameResumed -= HandleGameResumed;
        }
    }
    
    void OnDestroy()
    {  
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForTilemapGeneration -= HandleLayerChanged;
        }
    }
// 레이어별 데이터 초기화
    private void InitializeLayerData()
    {
        // 배열 범위 체크
        if (layerSpawnDatas == null || layerSpawnDatas.Length < 5) return;

        // 지뢰 (레이어 0)
        layerSpawnDatas[0].SetChances(enemyChance: 0.8f, hpChance: 0.2f, powerChance: 0f, dropInterval: 4f);

        // 해골 1 (레이어 1)
        layerSpawnDatas[1].SetChances(enemyChance: 0.25f, hpChance: 0.35f, powerChance: 0.4f, dropInterval: 5f);

        // 해골 2 (레이어 2)
        layerSpawnDatas[2].SetChances(enemyChance: 0.2f, hpChance: 0.40f, powerChance: 0.4f, dropInterval: 5f);

        // 뜨겁존 (레이어 3)
        layerSpawnDatas[3].SetChances(enemyChance: 0f, hpChance: 0.8f, powerChance: 0.2f, dropInterval: 2.5f);

        // 포이즌 (레이어 4)
        layerSpawnDatas[4].SetChances(enemyChance: 0.6f, hpChance: 0.4f, powerChance: 0.0f, dropInterval: 5f);
    }
    

    private IEnumerator FindPlayerCoroutine()
    {
        while (player == null)
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
                
                // LayerManager에서 현재 레이어 정보 가져오기
                if (LayerManager.Instance != null)
                {
                    currentLayer = LayerManager.Instance.GetCurrentLayerTileIndex();
                    Debug.Log("플레이어 찾음 - 현재 레이어 타일 인덱스: " + currentLayer);
                }
                
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
            
            Debug.Log("레이어 변경: " + currentLayer);
            
            FillSpawnQueue();
        }
    }

    private void HandleGameResumed()
    {
        Debug.Log("GameResumed");
        // 플레이어가 아직 찾아지지 않았다면 코루틴으로 대기
        if (player == null)
        {
            StartCoroutine(WaitForPlayerAndResume());
            return;
        }
        
        // LayerManager에서 현재 레이어 정보 가져오기
        if (LayerManager.Instance != null)
        {
            currentLayer = LayerManager.Instance.GetCurrentLayerTileIndex();
            Debug.Log("게임 Resume - 현재 레이어 타일 인덱스: " + currentLayer);
        }
        
        // 게임이 resume되었을 때 현재 위치에서 남은 길이를 계산해서 아이템을 다시 준비
        if (currentLayer >= 0)
        {
            shouldSpawnSavePoint = true;
            savePointSpawned = false;

            Debug.Log("게임 Resume: " + currentLayer);

            FillSpawnQueueForResume();
        }
    }
    
    private IEnumerator WaitForPlayerAndResume()
    {
        while (player == null)
        {
            yield return null;
        }
        
        // LayerManager에서 현재 레이어 정보 가져오기
        if (LayerManager.Instance != null)
        {
            currentLayer = LayerManager.Instance.GetCurrentLayerTileIndex();
            Debug.Log("게임 Resume (플레이어 찾음) - 현재 레이어 타일 인덱스: " + currentLayer);
        }
        
        // 플레이어를 찾은 후 다시 HandleGameResumed 로직 실행
        if (currentLayer >= 0)
        {
            shouldSpawnSavePoint = true;
            savePointSpawned = false;
            
            Debug.Log("게임 Resume (플레이어 찾음): " + currentLayer);
            
            FillSpawnQueueForResume();
        }
    }


    #endregion

    
    void Update()
    {
        if (!isPlayerFound || player == null || tilemap == null) return;

        // 높이 기반으로 dropInterval 간격마다 아이템 스폰
        float expectedDropY = lastDropY - currentDropInterval;
        
        if (player.position.y <= expectedDropY && spawnQueue.Count > 0)
        {
            SpawnNextItem();
            lastDropY = expectedDropY;
        }
        
        CheckAndSpawnSavePoint();
    }
    
    // 큐 관리 메서드들
    private void FillSpawnQueue()
    {
        if (currentLayer < 0 || currentLayer >= layerSpawnDatas.Length) return;
        
        LayerSpawnData data = layerSpawnDatas[currentLayer];
        currentDropInterval = data.GetDropInterval();

        float layerHeight = 40f;
        int totalItems = Mathf.RoundToInt(layerHeight / currentDropInterval);
        Debug.Log("현재 레이어는: " + currentLayer + "생성해야할 아이템 개수는: " + totalItems);
        
        if (totalItems <= 0) return;

        int enemyCount = Mathf.RoundToInt(totalItems * data.GetEnemyChance());
        int hpItemCount = Mathf.RoundToInt(totalItems * data.GetHpItemChance());
        int powerItemCount = Mathf.RoundToInt(totalItems * data.GetPowerItemChance());
        
        int actualTotal = enemyCount + hpItemCount + powerItemCount;
        if (actualTotal < totalItems)
        {
            if (data.GetEnemyChance() >= data.GetHpItemChance() && data.GetEnemyChance() >= data.GetPowerItemChance())
                enemyCount += (totalItems - actualTotal);
            else if (data.GetHpItemChance() >= data.GetPowerItemChance())
                hpItemCount += (totalItems - actualTotal);
            else
                powerItemCount += (totalItems - actualTotal);
        }

        // 아이템들을 리스트에 추가
        List<SpawnItem> tempList = new List<SpawnItem>();
        
        for (int i = 0; i < enemyCount; i++)
        {
            tempList.Add(new SpawnItem(SpawnType.Enemy, data.enemyPrefabs));
        }
        
        for (int i = 0; i < hpItemCount; i++)
        {
            tempList.Add(new SpawnItem(SpawnType.HpItem, data.hpItemPrefabs));
        }
        
        for (int i = 0; i < powerItemCount; i++)
        {
            tempList.Add(new SpawnItem(SpawnType.PowerItem, data.powerItemPrefabs));
        }
        
        // Fisher-Yates 셔플
        for (int i = tempList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            SpawnItem temp = tempList[i];
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }
        
        // 셔플된 아이템들을 큐에 추가
        ClearSpawnQueue();
        foreach (var item in tempList)
        {
            spawnQueue.Enqueue(item);
        }
        
        // 현재 플레이어 위치에서 lastDropY 초기화
        if (player != null)
        {
            lastDropY = Mathf.Floor(player.position.y / currentDropInterval) * currentDropInterval;
        }
    }
    
    private void FillSpawnQueueForResume()
    {
        if (currentLayer < 0 || currentLayer >= layerSpawnDatas.Length || player == null) return;
        
        LayerSpawnData data = layerSpawnDatas[currentLayer];
        currentDropInterval = data.GetDropInterval();

        // 현재 위치에서 레이어 끝까지의 남은 길이 계산
        float layerStartY = LayerManager.Instance.GetCurrentLayerStartY();
        float currentPlayerY = player.position.y;
        float remainingHeight = Mathf.Abs(currentPlayerY - layerStartY);
        
        int totalItems = Mathf.RoundToInt(remainingHeight / currentDropInterval);
        Debug.Log("게임 Resume - 현재 레이어: " + currentLayer + ", 남은 길이: " + remainingHeight + ", 생성할 아이템 개수: " + totalItems);
        
        if (totalItems <= 0) return;

        int enemyCount = Mathf.RoundToInt(totalItems * data.GetEnemyChance());
        int hpItemCount = Mathf.RoundToInt(totalItems * data.GetHpItemChance());
        int powerItemCount = Mathf.RoundToInt(totalItems * data.GetPowerItemChance());
        
        int actualTotal = enemyCount + hpItemCount + powerItemCount;
        if (actualTotal < totalItems)
        {
            if (data.GetEnemyChance() >= data.GetHpItemChance() && data.GetEnemyChance() >= data.GetPowerItemChance())
                enemyCount += (totalItems - actualTotal);
            else if (data.GetHpItemChance() >= data.GetPowerItemChance())
                hpItemCount += (totalItems - actualTotal);
            else
                powerItemCount += (totalItems - actualTotal);
        }

        // 아이템들을 리스트에 추가
        List<SpawnItem> tempList = new List<SpawnItem>();
        
        for (int i = 0; i < enemyCount; i++)
        {
            tempList.Add(new SpawnItem(SpawnType.Enemy, data.enemyPrefabs));
        }
        
        for (int i = 0; i < hpItemCount; i++)
        {
            tempList.Add(new SpawnItem(SpawnType.HpItem, data.hpItemPrefabs));
        }
        
        for (int i = 0; i < powerItemCount; i++)
        {
            tempList.Add(new SpawnItem(SpawnType.PowerItem, data.powerItemPrefabs));
        }
        
        // Fisher-Yates 셔플
        for (int i = tempList.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            SpawnItem temp = tempList[i];
            tempList[i] = tempList[randomIndex];
            tempList[randomIndex] = temp;
        }
        
        // 셔플된 아이템들을 큐에 추가
        ClearSpawnQueue();
        foreach (var item in tempList)
        {
            spawnQueue.Enqueue(item);
        }
        
        // 현재 플레이어 위치에서 lastDropY 초기화
        if (player != null)
        {
            lastDropY = Mathf.Floor(player.position.y / currentDropInterval) * currentDropInterval;
        }
    }
    
    private void ClearSpawnQueue()
    {
        spawnQueue.Clear();
    }



    void SpawnNextItem()
    {
        if (spawnQueue.Count == 0) return;
        
        List<Vector3Int> validTiles = GetValidTilesBelowViewport();
        if (validTiles.Count > 0)
        {
            Vector3Int spawnTile = validTiles[Random.Range(0, validTiles.Count)];
            Vector3 worldPos = tilemap.CellToWorld(spawnTile) + tilemap.tileAnchor;
            SpawnItem item = spawnQueue.Dequeue();
            item.Spawn(worldPos);
            
            Debug.Log("아이템 생성: 레이어 " + currentLayer + ", 큐 남은 개수: " + spawnQueue.Count);
        }
    }

    // 아이템 생성 타입
    public enum SpawnType
    {
        Enemy,
        HpItem,
        PowerItem
    }

    // 생성할 아이템 정보
    public class SpawnItem
    {
        public SpawnType type;
        public GameObject[] prefabs;
        
        public SpawnItem(SpawnType type, GameObject[] prefabs)
        {
            this.type = type;
            this.prefabs = prefabs;
        }
        
        public void Spawn(Vector3 position)
        {
            if (prefabs != null && prefabs.Length > 0)
            {
                GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
                if (prefab != null)
                {
                    Instantiate(prefab, position, Quaternion.identity);
                }
            }
        }
    }

    void CheckAndSpawnSavePoint()
    {
        if (!shouldSpawnSavePoint || savePointSpawned) return;
        
        float layerStartY = LayerManager.Instance.GetCurrentLayerStartY();
        float targetSpawnY = layerStartY - savePointSpawnDepth;
    
        if (player.position.y <= targetSpawnY)
        {
            SpawnSavePoint();
            savePointSpawned = true;
            shouldSpawnSavePoint = false;
        }
    }
    
    void SpawnSavePoint()
    {
        if (currentLayer < 0 || currentLayer >= layerSpawnDatas.Length) return;
        
        List<Vector3Int> validTiles = GetValidTilesBelowViewport();
        
        if (validTiles.Count > 0)
        {
            Vector3Int spawnTile = validTiles[Random.Range(0, validTiles.Count)];
            Vector3 spawnPos = tilemap.CellToWorld(spawnTile) + tilemap.tileAnchor;
            Instantiate(savePointPrefab, spawnPos, Quaternion.identity);
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
