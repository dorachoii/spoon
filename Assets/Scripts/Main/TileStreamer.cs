using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
// using System.Numerics; // 이건 지우세요
using System.IO;

public class TileStreamer : MonoBehaviour
{
    public Tilemap tilemap;
    public TileBase[] tile_plain; // 5가지 색상
    public TileBase[,] tile_dotted = new TileBase[9, 9];

    private Vector3Int lastBottomLeftCell;
    private Vector3Int lastTopRightCell;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        FillTiles();

        lastBottomLeftCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        lastTopRightCell = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        LoadDottedTiles("01");
        StampDottedTiles();
    }

    void Update()
    {
        Vector3Int currentBottomLeft = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(0, 0, mainCamera.nearClipPlane)));
        Vector3Int currentTopRight = tilemap.WorldToCell(mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane)));

        // 내려갔을 때 (더 아래로 이동)
        if (currentBottomLeft.y < lastBottomLeftCell.y)
        {
            int newHeight = lastBottomLeftCell.y - currentBottomLeft.y;

            FillNewBottom(currentBottomLeft.y, lastBottomLeftCell.y - 1);

            int clearStartY = Mathf.Min(lastTopRightCell.y + 1, currentTopRight.y + 1);
            int clearEndY = Mathf.Max(lastTopRightCell.y, currentTopRight.y);

            ClearTopRows(clearStartY, clearEndY);


            lastBottomLeftCell = currentBottomLeft;
            lastTopRightCell = currentTopRight;
        }
    }

    void FillNewBottom(int startY, int endY)
    {
        int width = lastTopRightCell.x - lastBottomLeftCell.x + 1;
        int height = endY - startY + 1;

        if (height <= 0) return;

        BoundsInt bounds = new BoundsInt(lastBottomLeftCell.x, startY, 0, width, height, 1);
        TileBase[] tiles = new TileBase[width * height];

        for (int i = 0; i < tiles.Length; i++)
        {
            tiles[i] = tile_plain[0];
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

    void LoadDottedTiles(string color)
    {
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                int index = y * 9 + x;
                string path = $"TileMap/BG_Dotted_V1_{color}_{index}";
                TileBase tile = Resources.Load<TileBase>(path);

                if (tile == null)
                {
                    Debug.LogError($"Tile not found at path: {path}");
                    continue;
                }

                tile_dotted[x, y] = tile;
            }
        }
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
