using UnityEngine;

public class PlayerSpawner : MonoBehaviour, ISaveable
{
    [Header("Player Prefab")]
    [SerializeField] private GameObject playerPrefab;

    public Vector3 defaultSpawnPosition = new Vector3(0, 8, 0);
    private bool hasSpawnedPlayer = false;
    private GameObject currentPlayer;

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
        if (PersistenceManager.Instance.HasSavedData())
        {
            return PersistenceManager.Instance.CurrentGameData.playerPosition;
        }

        return defaultSpawnPosition;
    }

    private void SpawnPlayer(Vector3 position)
    {
        if (playerPrefab == null) return;

        currentPlayer = Instantiate(playerPrefab, position, Quaternion.identity);

        var rb = currentPlayer.AddComponent<Rigidbody2D>();

        rb.mass = 0.0001f;
        rb.angularDrag = 0.05f;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

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
        }
        else
        {
            data.playerPosition = defaultSpawnPosition;
        }
    }

    public void ReadAndSetData(GameData data)
    {
    }
    #endregion
}
