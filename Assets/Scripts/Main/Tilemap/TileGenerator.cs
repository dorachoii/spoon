using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Plain, Dotted_V2, Gradient }

public class TileGenerator : MonoBehaviour, ISaveable
{
    public static TileGenerator Instance { get; private set; }
    private Camera mainCamera;

    [Header("Tilemap")]
    [SerializeField] public Tilemap tilemap;
    [SerializeField] private TileBase[] tile_plain; // 5 colors


    [Header("Tilemap Boundary")]
    private Vector3Int lastBottomLeftCell;
    private Vector3Int lastTopRightCell;
    int tileOffset = 30;
    int tilemapWidth = 0;


    [Header("Layer")]
    int currentTileLayer = 0;


    [Header("Stamping Tiles")]
    const int GRADIENT_TILE_SIZE = 12;
    const int DOTTED_TILE_SIZE = 9;
    private TileBase[,] tile_dotted = new TileBase[DOTTED_TILE_SIZE, DOTTED_TILE_SIZE];
    private TileBase[,] tile_gradient = new TileBase[GRADIENT_TILE_SIZE, GRADIENT_TILE_SIZE];
    //Gradient
   
    private int lastGradientLevel = -1;

    //Dotted
    private int lastDottedLevel = -1;
    private int lastStampingY = 0;
    [SerializeField] private int stampingInterval = 20;
    [SerializeField] private int maxStampingCount = 1;


    [Header("Tile Generation Control")]
    private bool isPaused = false;
    private bool isNormalLayerChanged = false;


    [Header("Boss Tilemap")]
    [SerializeField] private GameObject[] crumbleTilemapPrefabs;
    [SerializeField] private GameObject[] bossGroundTilemapPrefabs;
    [SerializeField] private GameObject[] bossPrefabs; // 보스 프리팹 배열 추가
    private GameObject currentCrumbleTilemap;
    private GameObject currentBossGroundTilemap;
    private GameObject currentBoss; // 현재 보스 객체
    private bool isSpawningBoss = false;



    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        mainCamera = Camera.main;

        if (tilemap == null) tilemap = GetComponentInChildren<Tilemap>();

        // tilemap
        lastBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        lastTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));
        tilemapWidth = lastTopRightCell.x - lastBottomLeftCell.x + 1;

        // stamping
        lastStampingY = lastBottomLeftCell.y;
    }

    void OnEnable()
    {
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForTilemapGeneration += HandleLevelChanged;
            LayerManager.Instance.OnTransitionLayerEntered += HandleTransitionLayerEntered;
            LayerManager.Instance.OnBossLayerEntered += HandleBossLayerEntered;
      
        }
    }

    void OnDisable()
    {
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChangedForTilemapGeneration -= HandleLevelChanged;
            LayerManager.Instance.OnTransitionLayerEntered -= HandleTransitionLayerEntered;
            LayerManager.Instance.OnBossLayerEntered -= HandleBossLayerEntered;
   
        }
    }

    void HandleLevelChanged(int newLevel)
    {
        currentTileLayer = LayerManager.Instance.GetCurrentLayerTileIndex();
  

        if (LayerManager.Instance != null && LayerManager.Instance.CurrentLayerState == LayerState.Normal)
        {
            isNormalLayerChanged = true;
            
            // 보스 층 완료 후 Normal 레이어로 변경될 때 타일 생성 재개
            if (isPaused)
            {
                Debug.Log("[TileGenerator] 보스 층 완료 후 Normal 레이어 진입 - 타일 생성 재개");
                ResumeTileGeneration();
            }
        }
    }

    #region Basic Tilemap
    void Update()
    {
        if (isPaused) return;

        Vector3Int currentBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        Vector3Int currentTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        // 카메라가 내려감에 따라 아래 타일은 채우고, 위 타일은 지움
        // カメラが下がるにつれて下のタイルを埋め、上のタイルを削除
        if (currentBottomLeftCell.y <= lastBottomLeftCell.y)
        {
            int currentLayerTileIndex = LayerManager.Instance.GetCurrentLayerTileIndex();
            FillBottomRows(currentBottomLeftCell.y - tileOffset, lastBottomLeftCell.y - 1, currentLayerTileIndex);

            // 패턴 찍어주는 함수수
            StampDottedPattern(currentBottomLeftCell.y);

            int clearStartY = Mathf.Min(lastTopRightCell.y + 1, currentTopRightCell.y + 1);
            int clearEndY = Mathf.Max(lastTopRightCell.y, currentTopRightCell.y);

            ClearTopRows(clearStartY + tileOffset, clearEndY + tileOffset);

            lastBottomLeftCell = currentBottomLeftCell;
            lastTopRightCell = currentTopRightCell;
            tilemap.CompressBounds();
        }

    }

    void FillBottomRows(int startY, int endY, int layer)
    {
        // 레이어 변경되면, 경계선 그라디언트 그려준다.
        if (isNormalLayerChanged)
        {
            StampGradientLine(startY, currentTileLayer);
            isNormalLayerChanged = false;
        }

        int height = endY - startY + 1;
        if (height <= 0) return;

        BoundsInt bounds = new BoundsInt(lastBottomLeftCell.x, startY, 0, tilemapWidth, height, 1);
        TileBase[] existingTiles = tilemap.GetTilesBlock(bounds);
        TileBase[] newTiles = new TileBase[tilemapWidth * height];

        // LayerManager에서 현재 레이어의 tileIndex를 가져옴
        int currentTileIndex = LayerManager.Instance.GetCurrentLayerTileIndex();
        
        for (int i = 0; i < existingTiles.Length; i++)
        {
            newTiles[i] = existingTiles[i] ?? tile_plain[currentTileIndex];
        }

        tilemap.SetTilesBlock(bounds, newTiles);

    }

    void ClearTopRows(int startY, int endY)
    {
        int height = endY - startY + 1;

        if (height <= 0) return;

        BoundsInt bounds = new BoundsInt(lastBottomLeftCell.x, startY, 0, tilemapWidth, height, 1);
        TileBase[] tiles = new TileBase[tilemapWidth * height]; // 전부 null (全てnull)

        tilemap.SetTilesBlock(bounds, tiles);
    }

    void StampGradientLine(int y, int level)
    {
        if (lastGradientLevel != level && level >= 0)
        {
            tile_gradient = LoadTileBlockFromResources(TileType.Gradient.ToString(), level.ToString("D2"), GRADIENT_TILE_SIZE);
            lastGradientLevel = level;
        }

        Vector3Int currentBottomLeft = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));

        // 반올림 (四捨五入)
        int chunkCount = Mathf.RoundToInt((float)tilemapWidth / GRADIENT_TILE_SIZE);

        int startX = currentBottomLeft.x;

        for (int i = 0; i < chunkCount; i++)
        {
            int originX = startX + i * GRADIENT_TILE_SIZE;

            BoundsInt bounds = new BoundsInt(originX, y, 0, GRADIENT_TILE_SIZE, GRADIENT_TILE_SIZE, 1);

            TileBase[] tiles = new TileBase[GRADIENT_TILE_SIZE * GRADIENT_TILE_SIZE];
            for (int dx = 0; dx < GRADIENT_TILE_SIZE; dx++)
            {
                for (int dy = 0; dy < GRADIENT_TILE_SIZE; dy++)
                {
                    // 파일 저장이 반대로 되어있음. (ファイルindexが逆になっている)
                    int index = dy * GRADIENT_TILE_SIZE + dx;
                    int reverseIndex = GRADIENT_TILE_SIZE * GRADIENT_TILE_SIZE - 1 - index;
                    tiles[reverseIndex] = tile_gradient[dx, dy];
                }
            }

            tilemap.SetTilesBlock(bounds, tiles);
        }
    }

    void StampDottedPattern(int currentY)
    {
        var validStampPos = new List<Vector3Int>();

        // 높이 체크: stampingInterval 간격으로 스탬프 찍기 (stampingInterval間隔でスタンプ)
        if (currentY <= lastStampingY - stampingInterval)
        {
            Vector3 belowViewportBottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0f, -0.2f, 0));
            Vector3 viewportTopRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 0f, 0));

            Vector3Int min = tilemap.WorldToCell(belowViewportBottomLeft);
            Vector3Int max = tilemap.WorldToCell(viewportTopRight);

            // 가로: 화면 너비, 세로: 화면 아래쪽 영역 + 타일 높이 (横: 画面幅、縦: 画面下側エリア + タイル高さ)
            // 범위 내에서 스탬프 후보 위치 저장 (範囲内でスタンプ候補位置を保存) 
            for (int x = min.x; x <= max.x - DOTTED_TILE_SIZE + 1; x += DOTTED_TILE_SIZE)
            {
                for (int y = min.y; y <= max.y - DOTTED_TILE_SIZE + 1; y += DOTTED_TILE_SIZE)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    validStampPos.Add(tilePos);
                }
            }

            // 랜덤으로 스탬프 찍기 (スタンプ候補位置からランダムに1つ選択してスタンプ)
            if (validStampPos.Count > 0)
            {
                Vector3Int randomStampPos = validStampPos[Random.Range(0, validStampPos.Count)];
                int currentLayerTileIndex = LayerManager.Instance.GetCurrentLayerTileIndex();
                StampSingleDottedTile(randomStampPos, currentLayerTileIndex);
            }

            lastStampingY = currentY;
        }
    }

    void StampSingleDottedTile(Vector3Int origin, int level)
    {
        int dottedLevelToLoad;

        switch (level)
        {
            case 0:
                dottedLevelToLoad = 1;
                break;
            case 2:
                dottedLevelToLoad = 2;
                break;
            case 3:
                dottedLevelToLoad = 3;
                break;
            default:
                return;
        }

        if (lastDottedLevel != dottedLevelToLoad)
        {
            tile_dotted = LoadTileBlockFromResources(TileType.Dotted_V2.ToString(), dottedLevelToLoad.ToString("D2"), 9);
            lastDottedLevel = dottedLevelToLoad;
        }

        // 해당 범위에 이미 타일이 있는지 체크
        BoundsInt dotBounds = new BoundsInt(origin.x, origin.y, 0, 9, 9, 1);
        TileBase[] existingTiles = tilemap.GetTilesBlock(dotBounds);
        
        // 기존 타일이 있는 위치에만 dotted 패턴을 찍기
        TileBase[] selectedTiles = new TileBase[9 * 9];

        for (int dx = 0; dx < 9; dx++)
        {
            for (int dy = 0; dy < 9; dy++)
            {
                int index = dy * 9 + dx;
                // 기존 타일이 있는 위치에만 dotted 타일을 찍고, 없으면 null 유지
                if (existingTiles[index] != null)
                {
                    selectedTiles[index] = tile_dotted[dx, dy];
                }
                else
                {
                    selectedTiles[index] = null;
                }
            }
        }

        tilemap.SetTilesBlock(dotBounds, selectedTiles);
    }

    TileBase[,] LoadTileBlockFromResources(string type, string color, int size)
    {
        TileBase[,] tileBlock = new TileBase[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                string path = $"TileMap/BG_{type}_{color}_{index}";

                TileBase tile = Resources.Load<TileBase>(path);

                if (tile == null) continue;

                tileBlock[x, y] = tile;
            }
        }
        return tileBlock;
    }
    #endregion

    #region Tile Generation Control
    public void PauseTileGeneration()
    {
        isPaused = true;
    }

    public void ResumeTileGeneration()
    {
        tilemap.ClearAllTiles();
        tilemap.CompressBounds();
        lastBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        lastTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));
        
        // 스탬핑 관련 변수들 초기화
        lastStampingY = lastBottomLeftCell.y;
        
        // PlayerController의 removedTiles 캐시 초기화
        PlayerContoller playerController = FindObjectOfType<PlayerContoller>();
        if (playerController != null)
        {
            playerController.ClearDiggedTiles();
        }

        isPaused = false;
    }


    // transition
    private void HandleTransitionLayerEntered(int bossIndex)
    {
        PauseTileGeneration();
    }

    // boss 
    private void HandleBossLayerEntered(int bossIndex)
    {
        SpawnBossTilemap(bossIndex);
    }

 

    #endregion

    #region Boss Tilemap
    // 보스 타일맵 생성
    public void SpawnBossTilemap(int bossIndex)
    {
        if (isSpawningBoss) return; // 이미 생성 중이면 중복 실행 방지
        
        StartCoroutine(SpawnBossTilemapCoroutine(bossIndex));
    }

    private IEnumerator SpawnBossTilemapCoroutine(int bossIndex)
    {
        isSpawningBoss = true;

        yield return null;

        if (bossIndex >= 0 && bossIndex < crumbleTilemapPrefabs.Length && crumbleTilemapPrefabs[bossIndex] != null)
        {
            currentCrumbleTilemap = Instantiate(crumbleTilemapPrefabs[bossIndex]);
            currentCrumbleTilemap.transform.SetParent(transform);
            
            Vector3 bottomCenterWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, -0.2f, mainCamera.nearClipPlane));
            currentCrumbleTilemap.transform.position = new Vector3(bottomCenterWorldPos.x, bottomCenterWorldPos.y, 0f);
            
            yield return null;
            
            currentBossGroundTilemap = Instantiate(bossGroundTilemapPrefabs[bossIndex]);
            currentBossGroundTilemap.transform.SetParent(transform);

            Vector3 belowBossWorldPos = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, -0.5f, mainCamera.nearClipPlane));
            currentBossGroundTilemap.transform.position = new Vector3(belowBossWorldPos.x, belowBossWorldPos.y, 0f);
            yield return null;

            Vector3 bossSpawnPosition = mainCamera.ViewportToWorldPoint(new Vector3(0.8f, -0.3f, mainCamera.nearClipPlane));
            bossSpawnPosition.z = 0f;

            SpawnBoss(bossIndex, bossSpawnPosition);
        }
        
        isSpawningBoss = false;
    }

    // 보스 스폰
    private void SpawnBoss(int bossIndex, Vector3 position)
    {
        if (bossIndex >= 0 && bossIndex < bossPrefabs.Length && bossPrefabs[bossIndex] != null)
        {
            currentBoss = Instantiate(bossPrefabs[bossIndex], position, Quaternion.identity);
            
            if (currentBoss != null)
            {
                currentBoss.SetActive(true);
            }
        }
    }


    #endregion


    #region Save&Load
    public void WriteData(GameData data)
    {
        data.tilemapData = GetTileDataList();
    }
    public void ReadAndSetData(GameData data)
    {
        LoadTilemapData(data.tilemapData);
    }



    // 저장용: 현재 타일맵에서 타일 데이터 리스트 얻기
    public List<TileData> GetTileDataList()
    {
        List<TileData> tileList = new List<TileData>();
        BoundsInt bounds = tilemap.cellBounds;

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int pos = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(pos);
                if (tile != null)
                {
                    tileList.Add(new TileData
                    {
                        x = x,
                        y = y,
                        tilebaseName = tile.name,
                       
                    });
                     Debug.Log("WriteData- TileData: " + tile.name);
                }
            }
        }
        return tileList;
    }

    // 불러오기용: 타일맵에 타일 데이터 리스트로 복원
    public void LoadTilemapData(List<TileData> tileDataList)
    {
        tilemap.ClearAllTiles();

        if (tileDataList == null || tileDataList.Count == 0) return;

        // 1. 좌표의 최소, 최대값 구하기 (bounds 계산)
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (var tileData in tileDataList)
        {
            if (tileData == null) continue;
            if (tileData.x < minX) minX = tileData.x;
            if (tileData.y < minY) minY = tileData.y;
            if (tileData.x > maxX) maxX = tileData.x;
            if (tileData.y > maxY) maxY = tileData.y;
        }

        // 3. 타일 데이터 배열 준비 및 세팅
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        TileBase[] tiles = new TileBase[width * height];
        for (int i = 0; i < tiles.Length; i++)
            tiles[i] = null;

        foreach (var tileData in tileDataList)
        {
            if (tileData == null) continue;
            int x = tileData.x - minX;
            int y = tileData.y - minY;
            int index = y * width + x;

            Tile tile = Resources.Load<Tile>("Tilemap/" + tileData.tilebaseName);
            if (tile != null)
                tiles[index] = tile;
            else
                Debug.LogWarning($"[TileMaker] Failed to load tile: {tileData.tilebaseName}");
        }

        BoundsInt bounds = new BoundsInt(minX, minY, 0, width, height, 1);
        tilemap.SetTilesBlock(bounds, tiles);
    }
    #endregion
}

