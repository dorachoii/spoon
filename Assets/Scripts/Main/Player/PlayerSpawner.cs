using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;
    
    [Header("Default Spawn Position")]
    [SerializeField] private Vector3 defaultSpawnPosition = new Vector3(0, 8, 0);
    
    private bool hasSpawnedPlayer = false;
    
    private void OnEnable()
    {
        PersistenceManager.OnDataLoaded += SpawnBasedOnData;
    }
    
    private void OnDisable()
    {
        PersistenceManager.OnDataLoaded -= SpawnBasedOnData;
    }
    
 
    
    private void SpawnBasedOnData()
    {
        Debug.Log("test: SpawnBasedOnData");
        if (hasSpawnedPlayer) return;
        
        // 저장된 플레이어 위치 읽기
        Vector3 spawnPosition = GetSpawnPosition();
        Debug.Log("1:[PlayerSpawner] spawnPosition: " + spawnPosition);
        
        // 저장된 위치에 플레이어 생성
        SpawnPlayer(spawnPosition);
        hasSpawnedPlayer = true;
        Debug.Log("1:[PlayerSpawner] hasSpawnedPlayer set to true");
    }
    
    
    private Vector3 GetSpawnPosition()
    {
        // PersistenceManager에서 로드된 데이터 사용
        if (PersistenceManager.Instance?.CurrentGameData != null)
        {
            Debug.Log($"1:[PlayerSpawner] Using player position from PersistenceManager: {PersistenceManager.Instance.CurrentGameData.playerPosition}");
            return PersistenceManager.Instance.CurrentGameData.playerPosition;
        }
        
        // 저장된 데이터가 없으면 기본 위치 반환
        Debug.Log("1:[PlayerSpawner] No saved data found, using default position");
        return defaultSpawnPosition;
    }
    
    private void SpawnPlayer(Vector3 position)
    {
        if (playerPrefab == null) {
            Debug.LogError("1:[PlayerSpawner] Player prefab is not assigned!");
            return;
        }
        
        GameObject spawnedPlayer = Instantiate(playerPrefab, position, Quaternion.identity);
        Debug.Log($"1:[PlayerSpawner] Player spawned at {position}, GameObject: {spawnedPlayer.name}");
        
        // 생성된 플레이어 객체가 활성화되어 있는지 확인
        if (!spawnedPlayer.activeInHierarchy)
        {
            Debug.LogWarning("1:[PlayerSpawner] Spawned player is not active in hierarchy!");
        }
        
        // PlayerStat 컴포넌트가 있는지 확인
        PlayerStat playerStat = spawnedPlayer.GetComponent<PlayerStat>();
        if (playerStat == null)
        {
            Debug.LogError("1:[PlayerSpawner] PlayerStat component not found on spawned player!");
        }
        else
        {
            Debug.Log("1:[PlayerSpawner] PlayerStat component found successfully");
        }
        
        GameManager.TriggerPlayerReady();
    }
    
}
