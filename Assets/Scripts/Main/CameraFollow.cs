using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 0.125f;
    
    private bool isPlayerReady = false;

    void Start()
    {
        // 플레이어 준비 이벤트 구독
        GameManager.OnPlayerReady += OnPlayerReady;
    }
    
    void OnDestroy()
    {
        GameManager.OnPlayerReady -= OnPlayerReady;
    }
    
    private void OnPlayerReady()
    {
        isPlayerReady = true;
        // 플레이어가 준비되면 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    void LateUpdate()
    {
        // 플레이어가 준비되지 않았으면 처리하지 않음
        if (!isPlayerReady)
        {
            return;
        }
        
        // player가 null이거나 파괴되었으면 처리하지 않음
        if (player == null)
        {
            // 다시 찾아보기
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                return; // 플레이어를 찾을 수 없으면 업데이트하지 않음
            }
        }
        
        Vector3 desiredPosition = new Vector3(transform.position.x, player.position.y, transform.position.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
