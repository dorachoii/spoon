using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public interface ISaveable
{
    void WriteData(GameData data);    // 현재 상태 -> data 저장
    void ReadData(GameData data); // data -> 현재 상태 복원
}

[Serializable]
public class GameData
{
    public Vector3 playerPosition;
    public List<TileData> tilemapData = new List<TileData>();
    public List<Vector3IntSerializable> removedTilePositions = new List<Vector3IntSerializable>();
}

[Serializable]
public class TileData
{
    public int x, y;
    public string tileName;
}

[Serializable]
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

public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager Instance { get; private set; }

    private string savePath;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        savePath = Path.Combine(Application.persistentDataPath, "savefile.json");
    }

    public void SaveGame()
    {
        var data = new GameData();
        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
        foreach (var saveable in saveables)
        {
            saveable.WriteData(data);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"[PersistenceManager] Game saved to {savePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("[PersistenceManager] No save file found.");
            return;
        }

        string json = File.ReadAllText(savePath);
        GameData data = JsonUtility.FromJson<GameData>(json);

        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
        foreach (var saveable in saveables)
        {
            saveable.ReadData(data);
        }

        Debug.Log("[PersistenceManager] Game loaded.");
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("[PersistenceManager] Save file deleted.");
        }
    }

    public bool HasSaveData()
    {
        return File.Exists(savePath);
    }
}
