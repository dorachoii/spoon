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
        if (hasSpawnedPlayer) return;
        
        Vector3 spawnPosition = GetSpawnPosition();
        
        SpawnPlayer(spawnPosition);
        hasSpawnedPlayer = true;
    }
    
    
    private Vector3 GetSpawnPosition()
    {
        if (PersistenceManager.Instance?.CurrentGameData != null)
        {
            return PersistenceManager.Instance.CurrentGameData.playerPosition;
        }
        
        Debug.Log("defaultSpawnPosition: " + defaultSpawnPosition);
        return defaultSpawnPosition;
    }
    
    private void SpawnPlayer(Vector3 position)
    {
        if (playerPrefab == null) return;
        
        // 비활성화 상태로 생성
        GameObject player = Instantiate(playerPrefab, position, Quaternion.identity);
        player.SetActive(false);
        
        // 위치 확실히 설정
        player.transform.position = position;
        
        // 위치 설정 후 활성화
        player.SetActive(true);
    } 
}
