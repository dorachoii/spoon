using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.IO;

public enum TileType { Plain, Dotted, Gradient }

public class TileMaker : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase[] tile_plain; // 5가지 색상
    private TileBase[,] tile_dotted = new TileBase[9, 9];
    private TileBase[,] tile_gradient = new TileBase[12, 12];

    private Vector3Int lastBottomLeftCell;
    private Vector3Int lastTopRightCell;

    private Camera mainCamera;
    int currentLevel = 0;

    int width = 0;
    private int lastGradientLineY = int.MinValue;
    private int lastLevel = -1;
    private int loadGradientLevel = -1;

    void OnEnable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelChanged += HandleLevelChanged;
        }
    }

    void OnDisable()
    {
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.OnLevelChanged -= HandleLevelChanged;
        }
    }

    void HandleLevelChanged(int newLevel)
    {
        currentLevel = Mathf.Clamp(newLevel, 0, tile_plain.Length - 1);

        Vector3Int currentBottomLeft = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        FillGradientLine(currentBottomLeft.y, currentLevel-1);

        lastGradientLineY = currentBottomLeft.y;
        lastLevel = currentLevel;
    }

    void Start()
    {
        mainCamera = Camera.main;
        FillTiles();

        lastBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        lastTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        tile_dotted = LoadTiles(TileType.Dotted.ToString(), "0", 9);
        StampDottedTiles();

        width = lastTopRightCell.x - lastBottomLeftCell.x + 1;
    }

    void Update()
    {
        Vector3Int currentBottomLeft = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        Vector3Int currentTopRight = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        // 내려갔을 때 (더 아래로 이동)
        if (currentBottomLeft.y < lastBottomLeftCell.y)
        {


            FillNewBottom(currentBottomLeft.y, lastBottomLeftCell.y - 1, currentLevel);

            int clearStartY = Mathf.Min(lastTopRightCell.y + 1, currentTopRight.y + 1);
            int clearEndY = Mathf.Max(lastTopRightCell.y, currentTopRight.y);

            ClearTopRows(clearStartY, clearEndY);

            lastBottomLeftCell = currentBottomLeft;
            lastTopRightCell = currentTopRight;
        }
    }

    void FillGradientLine(int y, int level)
    {
        if (loadGradientLevel != level)
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



    void StampDottedTiles()
    {
        Vector3Int bottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        Vector3Int topRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        int mapWidth = topRightCell.x - bottomLeftCell.x + 1;
        int mapHeight = topRightCell.y - bottomLeftCell.y + 1;

        List<Vector3Int> stampPos = new List<Vector3Int>();

        for (int x = bottomLeftCell.x; x <= topRightCell.x - 8; x++)
        {
            for (int y = bottomLeftCell.y; y <= topRightCell.y - 8; y++)
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

        int stampCount = Mathf.Min(13, stampPos.Count);

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
            BoundsInt bounds = new BoundsInt(origin.x, origin.y, 0, 9, 9, 1);
            tilemap.SetTilesBlock(bounds, selectedTiles);
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
}
