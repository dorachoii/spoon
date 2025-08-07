using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum TileType { Plain, Dotted, Gradient }

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
    private int lastGradientLineY = -1;
    private int lastLevel = -1;

    private int lastGradientLevel = -1;
    private int lastDottedLevel = 0;

    // Stamping 관련 변수들
    private int lastStampingY = 0;
    [SerializeField] private int stampingInterval = 10; // 더 자주 찍히도록 줄임
    [SerializeField] private int maxStampingCount = 1; // 한 번에 찍을 개수


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

        // 타일맵 값 관련 초기화
        lastBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        lastTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));
        tilemapWidth = lastTopRightCell.x - lastBottomLeftCell.x + 1;

        tile_dotted = LoadTileBlockFromResources(TileType.Dotted.ToString(), "0", 9);

        // Stamping 초기화
        lastStampingY = lastBottomLeftCell.y;
    }

    void OnEnable()
    {
        if (LayerManager.Instance != null) LayerManager.Instance.OnLayerChanged += HandleLevelChanged;
        if (GameManager.Instance != null) GameManager.OnGameReady += LoadTile;

    }

    void OnDisable()
    {
        if (LayerManager.Instance != null) LayerManager.Instance.OnLayerChanged -= HandleLevelChanged;
        if (GameManager.Instance != null) GameManager.OnGameReady -= LoadTile;
    }

    void Update()
    {
        Vector3Int currentBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        Vector3Int currentTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        // 현재 그려진 타일맵보다 내려가면면
        if (currentBottomLeftCell.y <= lastBottomLeftCell.y)
        {
            FillBottomRows(currentBottomLeftCell.y - tileOffset, lastBottomLeftCell.y - 1, currentLayer);

            // Stamping 체크 및 실행
            CheckAndExecuteStamping(currentBottomLeftCell.y);

            int clearStartY = Mathf.Min(lastTopRightCell.y + 1, currentTopRightCell.y + 1);
            int clearEndY = Mathf.Max(lastTopRightCell.y, currentTopRightCell.y);

            ClearTopRows(clearStartY + tileOffset, clearEndY + tileOffset);

            lastBottomLeftCell = currentBottomLeftCell;
            lastTopRightCell = currentTopRightCell;
            tilemap.CompressBounds();
        }

    }

    // TODO: 얘 역할 불분명
    void LoadTile()
    {
        lastTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));
        lastBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
    }

    void HandleLevelChanged(int newLevel)
    {
        currentLayer = Mathf.Clamp(newLevel, 0, tile_plain.Length - 1);

        Vector3Int currentBottomLeft = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));

        // 경계선을 그어준다.
        StampGradientLine(currentBottomLeft.y, currentLayer - 1);

        lastGradientLineY = currentBottomLeft.y;
        lastLevel = currentLayer;
    }


    void FillBottomRows(int startY, int endY, int layer)
    {
        int height = endY - startY + 1;
        if (height <= 0) return;

        BoundsInt bounds = new BoundsInt(lastBottomLeftCell.x, startY, 0, tilemapWidth, height, 1);
        TileBase[] tiles = new TileBase[tilemapWidth * height];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tile_plain[layer];
        }

        tilemap.SetTilesBlock(bounds, tiles);
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

        // 반올림
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
                    // 파일 저장이 반대로 되어있음.
                    int index = dy * GRADIENT_TILE_SIZE + dx;
                    int reverseIndex = GRADIENT_TILE_SIZE * GRADIENT_TILE_SIZE - 1 - index;
                    tiles[reverseIndex] = tile_gradient[dx, dy];
                }
            }

            tilemap.SetTilesBlock(bounds, tiles);
        }
    }

    void CheckAndExecuteStamping(int currentY)
    {
        // stampingInterval만큼 내려갔는지 체크
        if (currentY <= lastStampingY - stampingInterval)
        {
            // ItemSpawner처럼 화면 기준으로 계산
            Camera cam = Camera.main;
            float z = Mathf.Abs(cam.transform.position.z - tilemap.transform.position.z);
            float xPadding = 0.15f; // 좌우 여백 15%

            // 화면 밖 아래쪽 영역 (Viewport 좌표)
            Vector3 bottomLeftWorld = cam.ViewportToWorldPoint(new Vector3(0f + xPadding, -0.2f, z));
            Vector3 topRightWorld = cam.ViewportToWorldPoint(new Vector3(1f - xPadding, 0f, z));

            Vector3Int min = tilemap.WorldToCell(bottomLeftWorld);
            Vector3Int max = tilemap.WorldToCell(topRightWorld);

            // 유효한 타일 위치 찾기 (9x9 크기 고려)
            List<Vector3Int> validTiles = new List<Vector3Int>();
            for (int x = min.x; x <= max.x - DOTTED_TILE_SIZE + 1; x++)
            {
                for (int y = min.y; y <= max.y - DOTTED_TILE_SIZE + 1; y++)
                {
                    Vector3Int tilePos = new Vector3Int(x, y, 0);
                    if (tilemap.HasTile(tilePos)) // 실제 타일이 있는 위치만
                    {
                        validTiles.Add(tilePos);
                    }
                }
            }

            if (validTiles.Count > 0)
            {
                Vector3Int stampOrigin = validTiles[Random.Range(0, validTiles.Count)];
                Debug.Log($"[TileGenerator] Stamping at {stampOrigin} (valid tiles: {validTiles.Count})");
                StampSingleDottedTile(stampOrigin, currentLayer);
            }
            else
            {
                Debug.LogWarning("[TileGenerator] No valid tile found for stamping");
            }

            lastStampingY = currentY;
        }
    }

    void StampSingleDottedTile(Vector3Int origin, int level)
    {
        int dottedLevelToLoad = 0;

        switch (level)
        {
            case 0:
                dottedLevelToLoad = 0;
                break;
            case 2:
                dottedLevelToLoad = 1;
                break;
            case 3:
                dottedLevelToLoad = 2;
                break;
            default:
                return;
        }

        if (lastDottedLevel != dottedLevelToLoad)
        {
            tile_dotted = LoadTileBlockFromResources(TileType.Dotted.ToString() + "_V2", "0" + dottedLevelToLoad.ToString(), 9);
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

