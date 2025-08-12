using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
  
// 저장 가능한 객체가 구현해야 하는 인터페이스 (保存可能なオブジェクトが実装すべきインターフェース)
public interface ISaveable
{
    void WriteData(GameData data);    
    void ReadAndSetData(GameData data); 
}

[Serializable]
public class TileData
{
    public int x, y;  // grid cell 좌표 (座標)
    public string tilebaseName;  // tilebase name
}

// Vector3Int Serialize 가능하게 (シリアライズ可能にする)
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

// 게임 저장 데이터 (ゲームデータ)
[Serializable]
public class GameData
{
    public Vector3 playerPosition;
    public Vector3 cameraPosition;  // 카메라 위치 저장
    public List<TileData> tilemapData = new List<TileData>();   // 저장 시점의 tilemap 데이터 (保存時のタイルマップデータ)
}


public class PersistenceManager : MonoBehaviour
{
    public static PersistenceManager Instance { get; private set; }

    [Header("Save File")]
    private string savePath;
    public static event Action OnDataLoaded;
    
    // 현재 로드된 게임 데이터
    public GameData CurrentGameData { get; private set; }
    
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
        // TODO: Isaveable cache
        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();

        foreach (var saveable in saveables)
        {
            saveable.WriteData(data);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public void LoadGame()
    {
        if (HasSavedData()) 
        {
            StartCoroutine(LoadGameCoroutine());
        }else{

            OnDataLoaded?.Invoke();
        }
    }

    private IEnumerator LoadGameCoroutine()
    {
        string json;
        try
        {
            json = File.ReadAllText(savePath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"1:[PersistenceManager] Error reading save file: {e.Message}");
            OnDataLoaded?.Invoke();
            yield break;
        }
        
        CurrentGameData = JsonUtility.FromJson<GameData>(json);

        var saveables = FindObjectsOfType<MonoBehaviour>().OfType<ISaveable>();
        foreach (var saveable in saveables)
        {        
            saveable.ReadAndSetData(CurrentGameData);   
            yield return null;
        }

        OnDataLoaded?.Invoke();
    }

    public void ClearSave()
    {
        if (File.Exists(savePath)) File.Delete(savePath);
    }

    public bool HasSavedData()
    {
        return File.Exists(savePath);
    }
}
