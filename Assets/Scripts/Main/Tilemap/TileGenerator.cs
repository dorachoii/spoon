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
    int currentLayer = 0;


    [Header("Stamping Tiles")]
    const int GRADIENT_TILE_SIZE = 12;
    const int DOTTED_TILE_SIZE = 9;
    private TileBase[,] tile_dotted = new TileBase[DOTTED_TILE_SIZE, DOTTED_TILE_SIZE];
    private TileBase[,] tile_gradient = new TileBase[GRADIENT_TILE_SIZE, GRADIENT_TILE_SIZE];
    //Gradient
    private int gradientTileIdx = -1; 
    private int lastGradientLevel = -1;

    //Dotted
    private int lastDottedLevel = -1;
    private int lastStampingY = 0;
    [SerializeField] private int stampingInterval = 20; 
    [SerializeField] private int maxStampingCount = 1; 
    

    [Header("Tile Generation Control")]
    private bool isPaused = false;
    private bool isLayerChanged = false; 
    

    [Header("Boss Tilemap")]
    [SerializeField] private GameObject[] bossTilemapPrefabs; 
    private GameObject currentBossTilemap; 


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
            LayerManager.Instance.OnLayerChanged += HandleLevelChanged;
            LayerManager.Instance.OnTransitionLayerEntered += HandleTransitionLayerEntered;
            LayerManager.Instance.OnTransitionLayerExited += HandleTransitionLayerExited;
            LayerManager.Instance.OnBossLayerEntered += HandleBossLayerEntered;
            LayerManager.Instance.OnBossLayerExited += HandleBossLayerExited;
        }
    }

    void OnDisable()
    {
        if (LayerManager.Instance != null) 
        {
            LayerManager.Instance.OnLayerChanged -= HandleLevelChanged;
            LayerManager.Instance.OnTransitionLayerEntered -= HandleTransitionLayerEntered;
            LayerManager.Instance.OnTransitionLayerExited -= HandleTransitionLayerExited;
            LayerManager.Instance.OnBossLayerEntered -= HandleBossLayerEntered;
            LayerManager.Instance.OnBossLayerExited -= HandleBossLayerExited;
        }
    }

    void Update()
    {
        if (isPaused) return;

        Vector3Int currentBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        Vector3Int currentTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        // 카메라가 내려감에 따라 아래 타일은 채우고, 위 타일은 지움
        // カメラが下がるにつれて下のタイルを埋め、上のタイルを削除
        if (currentBottomLeftCell.y <= lastBottomLeftCell.y)
        {
            FillBottomRows(currentBottomLeftCell.y - tileOffset, lastBottomLeftCell.y - 1, currentLayer);

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

    void HandleLevelChanged(int newLevel)
    {
        currentLayer = Mathf.Clamp(newLevel, 0, tile_plain.Length - 1);
        gradientTileIdx = currentLayer - 1; 
        isLayerChanged = true;
    }

    void FillBottomRows(int startY, int endY, int layer)
    {
        // 레이어 변경되면, 경계선 그라디언트 그려준다.
        if (isLayerChanged && gradientTileIdx >= 0)
        {
            StampGradientLine(startY, gradientTileIdx);
            isLayerChanged = false; 
            gradientTileIdx = -1; 
        }
        
        int height = endY - startY + 1;
        if (height <= 0) return;

        BoundsInt bounds = new BoundsInt(lastBottomLeftCell.x, startY, 0, tilemapWidth, height, 1);
        TileBase[] existingTiles = tilemap.GetTilesBlock(bounds);
        TileBase[] newTiles = new TileBase[tilemapWidth * height];

        for (int i = 0; i < existingTiles.Length; i++)
        {
            newTiles[i] = existingTiles[i] ?? tile_plain[layer];
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
            tile_gradient = LoadTileBlockFromResources(TileType.Gradient.ToString(), level.ToString(), GRADIENT_TILE_SIZE);
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
            for (int x = min.x; x <= max.x - DOTTED_TILE_SIZE + 1; x+=DOTTED_TILE_SIZE)
            {
                for (int y = min.y; y <= max.y - DOTTED_TILE_SIZE + 1; y+=DOTTED_TILE_SIZE)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    validStampPos.Add(tilePos);
                }
            }

            // 랜덤으로 스탬프 찍기 (スタンプ候補位置からランダムに1つ選択してスタンプ)
            if (validStampPos.Count > 0)
            {
                Vector3Int randomStampPos = validStampPos[Random.Range(0, validStampPos.Count)];
                StampSingleDottedTile(randomStampPos, currentLayer);
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

        TileBase[] selectedTiles = new TileBase[9 * 9];

        for (int dx = 0; dx < 9; dx++)
        {
            for (int dy = 0; dy < 9; dy++)
            {
                selectedTiles[dy * 9 + dx] = tile_dotted[dx, dy];
            }
        }

        BoundsInt dotBounds = new BoundsInt(origin.x, origin.y, 0, 9, 9, 1);
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

    // 타일 생성 일시정지
    public void PauseTileGeneration()
    {
        isPaused = true;
    }

    // 타일 생성 재개
    public void ResumeTileGeneration()
    {
        isPaused = false;
    }

    // 타일 생성 상태 확인
    public bool IsTileGenerationPaused()
    {
        return isPaused;
    }
    
    // 전환 층 진입 시 호출
    private void HandleTransitionLayerEntered(int bossIndex)
    {
        Debug.Log($"[TileGenerator] 전환 층 {bossIndex} 진입 - 타일 생성 중단");
        
        // 타일 생성 중단 (전환 층에서는 타일 생성하지 않음)
        PauseTileGeneration();
    }
    
    // 전환 층 퇴장 시 호출
    private void HandleTransitionLayerExited(int bossIndex)
    {
        Debug.Log($"[TileGenerator] 전환 층 {bossIndex} 퇴장");
        
        // 전환 층을 나가면 보스 타일맵 생성 준비
        // (보스 층 진입 시점에 실제로 생성됨)
    }
    
    // 보스 층 진입 시 호출
    private void HandleBossLayerEntered(int bossIndex)
    {
        Debug.Log($"[TileGenerator] 보스 층 {bossIndex} 진입 - 보스 타일맵 생성");
        
        // 보스 타일맵 생성
        SpawnBossTilemap(bossIndex);
    }
    
    // 보스 층 퇴장 시 호출
    private void HandleBossLayerExited(int bossIndex)
    {
        Debug.Log($"[TileGenerator] 보스 층 {bossIndex} 퇴장 - 보스 타일맵 제거");
        
        // 보스 타일맵 제거
        RemoveBossTilemap();
        
        // 일반 타일 생성 재개
        ResumeTileGeneration();
    }
    
    // 보스 타일맵 생성
    public void SpawnBossTilemap(int bossIndex)
    {
        // 기존 보스 타일맵이 있다면 제거
        RemoveBossTilemap();
        
        // 보스 인덱스가 유효한지 확인
        if (bossIndex >= 0 && bossIndex < bossTilemapPrefabs.Length && bossTilemapPrefabs[bossIndex] != null)
        {
            // 보스 타일맵 프리팹 인스턴스화
            currentBossTilemap = Instantiate(bossTilemapPrefabs[bossIndex], transform);
            Debug.Log($"[TileGenerator] 보스 타일맵 {bossIndex} 생성됨");
        }
        else
        {
            Debug.LogWarning($"[TileGenerator] 보스 타일맵 프리팹 {bossIndex}가 없습니다!");
        }
    }
    
    // 보스 타일맵 제거
    private void RemoveBossTilemap()
    {
        if (currentBossTilemap != null)
        {
            Destroy(currentBossTilemap);
            currentBossTilemap = null;
            Debug.Log("[TileGenerator] 보스 타일맵 제거됨");
        }
    }
    


    #region Save&Load
    public void WriteData(GameData data)
    {
        data.tilemapData = GetTileDataList();
    }
    public void ReadData(GameData data)
    {
        LoadTilemapData(data.tilemapData);
    }



    // 저장용: 현재 타일맵에서 타일 데이터 리스트 얻기
    public List<TileData> GetTileDataList()
    {
        List<TileData> tileList = new List<TileData>();
        BoundsInt bounds = tilemap.cellBounds;

        Debug.Log($"[Save&Load] 저장 시, Tilemap.cellBounds Y Range: {bounds.yMin} ~ {bounds.yMax - 1}");

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
                        tilebaseName = tile.name
                    });
                }
            }
        }
        return tileList;
    }

    // 불러오기용: 타일맵에 타일 데이터 리스트로 복원
    public void LoadTilemapData(List<TileData> tileDataList)
    {
        Debug.Log($"[Save&Load] 로드 시작: ClearAllTiles할 tilemap.cellBounds Y Range: {tilemap.cellBounds.yMin} ~ {tilemap.cellBounds.yMax - 1}");

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

