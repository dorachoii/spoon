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
    
    // 플랫폼별 저장 방식 구분
    private bool isWebGL;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 플랫폼 확인
        isWebGL = Application.platform == RuntimePlatform.WebGLPlayer;
        
        if (isWebGL)
        {
            // WebGL에서는 PlayerPrefs 사용
            Debug.Log("[PersistenceManager] WebGL 플랫폼 감지 - PlayerPrefs 사용");
        }
        else
        {
            // 다른 플랫폼에서는 파일 시스템 사용
            savePath = Path.Combine(Application.persistentDataPath, "savefile.json");
            Debug.Log($"[PersistenceManager] 파일 시스템 사용 - 경로: {savePath}");
        }
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
        
        if (isWebGL)
        {
            // WebGL에서는 PlayerPrefs 사용
            PlayerPrefs.SetString("GameSaveData", json);
            PlayerPrefs.Save();
            Debug.Log("[PersistenceManager] WebGL - PlayerPrefs에 게임 데이터 저장");
        }
        else
        {
            // 다른 플랫폼에서는 파일 시스템 사용
            File.WriteAllText(savePath, json);
            Debug.Log($"[PersistenceManager] 파일 시스템에 게임 데이터 저장: {savePath}");
        }
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
        
        if (isWebGL)
        {
            // WebGL에서는 PlayerPrefs에서 로드
            json = PlayerPrefs.GetString("GameSaveData", "");
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[PersistenceManager] WebGL - 저장된 게임 데이터가 없습니다");
                OnDataLoaded?.Invoke();
                yield break;
            }
            Debug.Log("[PersistenceManager] WebGL - PlayerPrefs에서 게임 데이터 로드");
        }
        else
        {
            // 다른 플랫폼에서는 파일 시스템에서 로드
            try
            {
                json = File.ReadAllText(savePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PersistenceManager] 파일 읽기 오류: {e.Message}");
                OnDataLoaded?.Invoke();
                yield break;
            }
            Debug.Log($"[PersistenceManager] 파일 시스템에서 게임 데이터 로드: {savePath}");
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
        if (isWebGL)
        {
            // WebGL에서는 PlayerPrefs에서 삭제
            PlayerPrefs.DeleteKey("GameSaveData");
            PlayerPrefs.Save();
            Debug.Log("[PersistenceManager] WebGL - PlayerPrefs에서 게임 데이터 삭제");
        }
        else
        {
            // 다른 플랫폼에서는 파일 삭제
            if (File.Exists(savePath)) 
            {
                File.Delete(savePath);
                Debug.Log($"[PersistenceManager] 파일 시스템에서 게임 데이터 삭제: {savePath}");
            }
        }
    }

    public bool HasSavedData()
    {
        if (isWebGL)
        {
            // WebGL에서는 PlayerPrefs에서 확인
            return PlayerPrefs.HasKey("GameSaveData") && !string.IsNullOrEmpty(PlayerPrefs.GetString("GameSaveData", ""));
        }
        else
        {
            // 다른 플랫폼에서는 파일 존재 여부 확인
            return File.Exists(savePath);
        }
    }
}
