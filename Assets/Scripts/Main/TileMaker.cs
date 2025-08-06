using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;
using Unity.VisualScripting.Dependencies.Sqlite;
using Unity.VisualScripting;

public enum TileType { Plain, Dotted, Gradient }

public class TileMaker : MonoBehaviour, ISaveable
{
    public static TileMaker Instance { get; private set; }
    public Tilemap tilemap;
    public TileBase[] tile_plain; // 5가지 색상
    private TileBase[,] tile_dotted = new TileBase[9, 9];
    private TileBase[,] tile_gradient = new TileBase[12, 12];
    private TileBase[,] tile_breakable = new TileBase[24, 24];

    private Vector3Int lastBottomLeftCell;
    private Vector3Int lastTopRightCell;
    int tileBuffer = 30;

    private Camera mainCamera;
    int currentLevel = 0;

    int width = 0;
    private int lastGradientLineY = int.MinValue;
    private int lastLevel = -1;
    private int loadGradientLevel = -1;


    public void WriteData(GameData data)
    {
        data.tilemapData = GetTileDataList();
    }
    public void ReadData(GameData data)
    {
        LoadTilemapData(data.tilemapData);
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;



    }

    void OnEnable()
    {
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChanged += HandleLevelChanged;
        }

        GameManager.OnGameReady += LoadTile;
    }

    void OnDisable()
    {
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnLayerChanged -= HandleLevelChanged;
        }

        GameManager.OnGameReady -= LoadTile;
    }

    void LoadTile()
    {
        Debug.Log("[타일생성] LoadTile");
        // load되고, 플레이어 
        lastTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));
        lastBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
    }

    void HandleLevelChanged(int newLevel)
    {
        if (mainCamera == null) mainCamera = Camera.main;

        currentLevel = Mathf.Clamp(newLevel, 0, tile_plain.Length - 1);

        Vector3Int currentBottomLeft = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));

        FillGradientLine(currentBottomLeft.y, currentLevel - 1);


        lastGradientLineY = currentBottomLeft.y;
        lastLevel = currentLevel;
    }


    [SerializeField]
    private List<int> bossSpawnLevels;



    void Start()
    {
        mainCamera = Camera.main;
        //FillTiles();

        // viewport point -> world point -> cell point
        lastBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));

        lastTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        tile_dotted = LoadTiles(TileType.Dotted.ToString(), "0", 9);

        int width = lastTopRightCell.x - lastBottomLeftCell.x + 1;
        int height = lastTopRightCell.y - lastBottomLeftCell.y + 1;

        BoundsInt bounds = new BoundsInt(lastBottomLeftCell.x, lastBottomLeftCell.y, 0, width, height, 1);

        StampDottedTilesInBounds(bounds, 13, currentLevel);

        this.width = width;
    }

    void Update()
    {
        Vector3Int currentBottomLeft = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        Vector3Int currentTopRight = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        // 내려갔을 때 (더 아래로 이동)
        if (currentBottomLeft.y <= lastBottomLeftCell.y)
        {
            FillNewBottom(currentBottomLeft.y - tileBuffer, lastBottomLeftCell.y - 1, currentLevel);

            int clearStartY = Mathf.Min(lastTopRightCell.y + 1, currentTopRight.y + 1);
            int clearEndY = Mathf.Max(lastTopRightCell.y, currentTopRight.y);

            ClearTopRows(clearStartY + tileBuffer, clearEndY + tileBuffer);

            lastBottomLeftCell = currentBottomLeft;
            lastTopRightCell = currentTopRight;
            tilemap.CompressBounds();
        }
    }

    void FillGradientLine(int y, int level)
    {
        if (loadGradientLevel != level && level >= 0)
        {
            tile_gradient = LoadTiles(TileType.Gradient.ToString(), level.ToString(), 12);
            loadGradientLevel = level;
        }

        Vector3Int currentBottomLeft = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        Vector3Int currentTopRight = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        int cellWidth = currentTopRight.x - currentBottomLeft.x + 1;
        int chunkCount = Mathf.CeilToInt((float)cellWidth / 12);

        int startX = currentBottomLeft.x;

        for (int i = 0; i < chunkCount; i++)
        {
            int originX = startX + i * 12;

            BoundsInt bounds = new BoundsInt(originX, y, 0, 12, 12, 1);

            TileBase[] tiles = new TileBase[12 * 12];
            for (int dx = 0; dx < 12; dx++)
            {
                for (int dy = 0; dy < 12; dy++)
                {
                    int index = dy * 12 + dx;
                    int reverseIndex = (12 * 12 - 1) - index;
                    tiles[reverseIndex] = tile_gradient[dx, dy];
                }
            }

            tilemap.SetTilesBlock(bounds, tiles);
        }
    }





    void FillNewBottom(int startY, int endY, int level)
    {
        int height = endY - startY + 1;

        if (height <= 0) return;

        BoundsInt bounds = new BoundsInt(lastBottomLeftCell.x, startY, 0, width, height, 1);
        TileBase[] tiles = new TileBase[width * height];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tile_plain[level];
        }

        tilemap.SetTilesBlock(bounds, tiles);

    }

    void ClearTopRows(int startY, int endY)
    {
        int width = lastTopRightCell.x - lastBottomLeftCell.x + 1;
        int height = endY - startY + 1;

        if (height <= 0) return;

        BoundsInt bounds = new BoundsInt(lastBottomLeftCell.x, startY, 0, width, height, 1);
        TileBase[] tiles = new TileBase[width * height]; // 전부 null

        tilemap.SetTilesBlock(bounds, tiles);
    }


    private int loadedDottedLevel = -1;
    void StampDottedTilesInBounds(BoundsInt bounds, int maxStampCount, int level)
    {
        int dottedLevelToLoad = 0;

        Debug.Log($"level = {level}");

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

        Debug.Log($"level = {level}, dottedLevel = {dottedLevelToLoad}");

        if (loadedDottedLevel != dottedLevelToLoad)
        {
            tile_dotted = LoadTiles(TileType.Dotted.ToString(), dottedLevelToLoad.ToString(), 9);
            loadedDottedLevel = dottedLevelToLoad;
        }

        int mapWidth = bounds.size.x;
        int mapHeight = bounds.size.y;

        List<Vector3Int> stampPos = new List<Vector3Int>();

        for (int x = bounds.xMin; x <= bounds.xMax - 8; x++)
        {
            for (int y = bounds.yMin; y <= bounds.yMax - 8; y++)
            {
                stampPos.Add(new Vector3Int(x, y, 0));
            }
        }

        int n = stampPos.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int j = UnityEngine.Random.Range(i, n);
            var temp = stampPos[i];
            stampPos[i] = stampPos[j];
            stampPos[j] = temp;
        }

        int stampCount = Mathf.Min(maxStampCount, stampPos.Count);

        for (int i = 0; i < stampCount; i++)
        {
            Vector3Int origin = stampPos[i];
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
    }


    TileBase[,] LoadTiles(string type, string color, int size)
    {
        TileBase[,] tiles = new TileBase[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                int index = y * size + x;
                string path = $"TileMap/BG_{type}_{color}_{index}";
                TileBase tile = Resources.Load<TileBase>(path);

                if (tile == null)
                {
                    Debug.LogError($"Tile not found at path: {path}");
                    continue;
                }

                tiles[x, y] = tile;
            }
        }
        return tiles;
    }

    void FillTiles()
    {
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane));
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));

        Vector3Int bottomLeftCell = tilemap.WorldToCell(bottomLeft);
        Vector3Int topRightCell = tilemap.WorldToCell(topRight);

        int width = topRightCell.x - bottomLeftCell.x + 1;
        int height = topRightCell.y - bottomLeftCell.y + 1;

        BoundsInt bounds = new BoundsInt(bottomLeftCell.x, bottomLeftCell.y, 0, width, height, 1);
        TileBase[] tiles = new TileBase[width * height];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tile_plain[0];
        }

        tilemap.SetTilesBlock(bounds, tiles);
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

}
