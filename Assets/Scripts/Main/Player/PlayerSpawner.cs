using UnityEngine;

public class PlayerSpawner : MonoBehaviour, ISaveable
{
    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;

    public Vector3 defaultSpawnPosition = new Vector3(0, 8, 0);
    private bool hasSpawnedPlayer = false;
    private GameObject currentPlayer; // 현재 생성된 플레이어 참조

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
        Debug.Log($"플레이어 위치 - PlayerSpawner: {spawnPosition}");

        SpawnPlayer(spawnPosition);
        hasSpawnedPlayer = true;
    }

    private Vector3 GetSpawnPosition()
    {
        if (PersistenceManager.Instance.HasSavedData())
        {
            return PersistenceManager.Instance.CurrentGameData.playerPosition;
        }
        Debug.Log($"플레이어 위치 - PlayerSpawner 현재 저장된 정보가 없어서: {defaultSpawnPosition}");
        return defaultSpawnPosition;
    }

    private void SpawnPlayer(Vector3 position)
    {
        if (playerPrefab == null) return;

        // 원하는 위치에 생성
        currentPlayer = Instantiate(playerPrefab, position, Quaternion.identity);

        // Rigidbody 없으면 추가
        var rb = currentPlayer.AddComponent<Rigidbody2D>();

        // 설정값 적용
        rb.mass = 0.0001f;
        rb.angularDrag = 0.05f;
        rb.freezeRotation = true;  // 전체 회전 고정
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;  // Z회전만 고정
    }

    // 현재 플레이어 위치 가져오기
    public Vector3 GetCurrentPlayerPosition()
    {
        if (currentPlayer != null)
        {
            return currentPlayer.transform.position;
        }
        return defaultSpawnPosition;
    }

    #region Save & Load
    public void WriteData(GameData data)
    {
        if (currentPlayer != null)
        {
            data.playerPosition = currentPlayer.transform.position;
            Debug.Log($"PlayerSpawner - 위치 저장: {data.playerPosition}");
        }
        else
        {
            data.playerPosition = defaultSpawnPosition;
            Debug.Log($"PlayerSpawner - 플레이어가 없어서 기본 위치 저장: {data.playerPosition}");
        }
    }

    public void ReadAndSetData(GameData data)
    {
        // PlayerSpawner는 OnDataLoaded 이벤트에서 처리하므로 여기서는 아무것도 하지 않음
        // 플레이어 생성과 위치 설정은 SpawnBasedOnData에서 처리됨
    }
    #endregion
}
