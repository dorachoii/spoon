using UnityEngine;
using System.IO;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;
    
    [Header("Default Spawn Position")]
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(0, 8, 0);
    
    private GameObject currentPlayer;
    private bool isDataLoaded = false;
    private Vector3 savedPlayerPosition;
    private string savePath;
    
    private void Start()
    {
        savePath = Path.Combine(Application.persistentDataPath, "savefile.json");
        
        // PersistenceManager의 OnDataLoaded 이벤트 구독
        PersistenceManager.OnDataLoaded += OnDataLoaded;
        
        // 기본 위치로 플레이어 생성 (새 게임용)
        SpawnPlayer(defaultSpawnPosition);
    }
    
    private void OnDestroy()
    {
        PersistenceManager.OnDataLoaded -= OnDataLoaded;
    }
    
    private void OnDataLoaded()
    {
        isDataLoaded = true;
        
        // 저장된 데이터가 있으면 플레이어 위치 업데이트
        if (PersistenceManager.Instance != null && PersistenceManager.Instance.HasSavedData())
        {
            // 저장된 플레이어 위치 읽기
            Vector3 savedPosition = ReadPlayerPositionFromSave();
            
            // 현재 플레이어 제거
            if (currentPlayer != null)
            {
                Destroy(currentPlayer);
            }
            
            // 저장된 위치에 플레이어 생성
            SpawnPlayer(savedPosition);
        }
    }
    
    private Vector3 ReadPlayerPositionFromSave()
    {
        if (!File.Exists(savePath))
        {
            return defaultSpawnPosition;
        }
        
        try
        {
            string json = File.ReadAllText(savePath);
            GameData data = JsonUtility.FromJson<GameData>(json);
            Debug.Log($"[PlayerSpawner] Read player position from save: {data.playerPosition}");
            return data.playerPosition;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[PlayerSpawner] Error reading save file: {e.Message}");
            return defaultSpawnPosition;
        }
    }
    
    private void SpawnPlayer(Vector3 position)
    {
        if (playerPrefab == null)
        {
            Debug.LogError("[PlayerSpawner] Player prefab is not assigned!");
            return;
        }
        
        currentPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
        Debug.Log($"[PlayerSpawner] Player spawned at position: {position}");
    }
    
    // 현재 플레이어 인스턴스 반환
    public GameObject GetCurrentPlayer()
    {
        return currentPlayer;
    }
}
