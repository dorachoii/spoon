using UnityEngine;

public class DoorSpawner : MonoBehaviour
{
    [Header("Door Settings")]
    public GameObject doorPrefab;
    public Vector3 doorOffset = new Vector3(0f, 2f, 0f); // 플레이어 위치 기준 오프셋
    
    private Transform player;
    private bool isPlayerFound = false;
    private bool doorSpawned = false;
    
    void Start()
    {
        // LayerManager 이벤트 구독
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnAllLayersCompleted += SpawnDoor;
        }
        
        // 플레이어를 찾을 때까지 코루틴으로 대기
        StartCoroutine(FindPlayerCoroutine());
    }
    
    private System.Collections.IEnumerator FindPlayerCoroutine()
    {
        // 플레이어가 생성될 때까지 대기
        while (player == null)
        {
            if (GameObject.FindGameObjectWithTag("Player") != null)
            {
                player = GameObject.FindGameObjectWithTag("Player").transform;
                isPlayerFound = true;
                break;
            }
            
            yield return null;
        }
    }
    
    void OnDestroy()
    {
        // LayerManager 이벤트 구독 해제
        if (LayerManager.Instance != null)
        {
            LayerManager.Instance.OnAllLayersCompleted -= SpawnDoor;
        }
    }
    
    private void SpawnDoor()
    {
        if (doorSpawned || doorPrefab == null || !isPlayerFound || player == null) return;
        
        // 플레이어 위치에 문 생성
        Vector3 doorPosition = player.position + doorOffset;
        Instantiate(doorPrefab, doorPosition, Quaternion.identity);
        
        // 플레이어 리지드바디 중력 비활성화 (떨어지지 않도록)
        Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();
        if (playerRigidbody != null)
        {
            playerRigidbody.gravityScale = 0f;
            Debug.Log("[DoorSpawner] Player gravity disabled!");
        }
        
        doorSpawned = true;
        Debug.Log("[DoorSpawner] Door spawned at player position!");
    }
}
