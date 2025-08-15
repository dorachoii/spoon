using UnityEngine;
using System.Collections;

public class DoorSpawner : MonoBehaviour
{
    [Header("Door Settings")]
    public GameObject doorPrefab;
    private Vector3 doorOffset = new Vector3(0f, 7f, 0f); // 플레이어 위치 기준 오프셋
    
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
       
        
    }
void Update(){
    if(Input.GetKeyDown(KeyCode.Space)){
        SpawnDoor();
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
        // 플레이어 찾기
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
            if (player != null)
            {
                isPlayerFound = true;
            }
        }
        
        if (doorSpawned || doorPrefab == null || !isPlayerFound || player == null) {
            return;
        }
          
        // 플레이어 리지드바디를 kinematic으로 설정
        Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = false;
        }
        
        // 플레이어 위치에 문 생성
        Vector3 doorPosition = player.position + doorOffset;
        Instantiate(doorPrefab, doorPosition, Quaternion.identity);
        
        doorSpawned = true;
    }
}
