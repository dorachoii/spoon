using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;


[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public List<TileData> tilemapData;
    public List<Vector3IntSerializable> removedTilePositions = new List<Vector3IntSerializable>();
}

[System.Serializable]
public class TileData
{
    public int x, y;
    public string tileName;
}

[System.Serializable]
public struct Vector3IntSerializable
{
    public int x, y, z;
    public Vector3IntSerializable(Vector3Int v)
    {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3Int ToVector3Int() => new Vector3Int(x, y, z);
}

public class GameManager : MonoBehaviour
{
    public Tilemap tilemap;
    public PlayerContoller playerController;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadGame();
        }
    }
    public void SaveGame()
    {
        SaveData saveData = new SaveData();
        saveData.playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position + Vector3.up;

        BoundsInt bounds = tilemap.cellBounds;

        List<TileData> tileList = new List<TileData>();

        for (int y = bounds.yMin; y < bounds.yMax; y++)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                Vector3Int position = new Vector3Int(x, y, 0);
                TileBase tile = tilemap.GetTile(position);
                if (tile != null)
                {
                    TileData tileData = new TileData
                    {
                        x = x,
                        y = y,
                        tileName = tile.name
                    };
                    tileList.Add(tileData);
                }
            }
        }

        saveData.tilemapData = tileList;

        saveData.removedTilePositions = new List<Vector3IntSerializable>();
        foreach (var pos in playerController.GetRemovedTiles())
        {
            saveData.removedTilePositions.Add(new Vector3IntSerializable(pos));
        }


        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(Path.Combine(Application.persistentDataPath, "savefile.json"), json);
    }

    public void LoadGame()
    {
        string path = Path.Combine(Application.persistentDataPath, "savefile.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SaveData saveData = JsonUtility.FromJson<SaveData>(json);

            if (saveData.tilemapData == null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = saveData.playerPosition;
            }

            if (tilemap == null) return;

            tilemap.ClearAllTiles();

            foreach (TileData tileData in saveData.tilemapData)
            {
                if (tileData == null) continue;

                Vector3Int position = new Vector3Int(tileData.x, tileData.y, 0);

                Tile tile = Resources.Load<Tile>("Tilemap/" + tileData.tileName);
                if (tile != null)
                {
                    tilemap.SetTile(position, tile);
                }
            }

            playerController.LoadRemovedTiles(saveData.removedTilePositions);
        }
        else
        {
            Debug.LogError("[SaveLoad] Save file not found!");
        }
    }


}
