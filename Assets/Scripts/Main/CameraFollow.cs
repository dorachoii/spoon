using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float smoothSpeed = 0.125f;
    
    private bool isPlayerFound = false;

    void Start()
    {
        StartCoroutine(FindPlayerCoroutine());
    }

    private IEnumerator FindPlayerCoroutine()
    {
        // 플레이어가 생성될 때까지 대기
        while (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                isPlayerFound = true;
                break;
            }
            
            yield return null;
        }
    }

    void LateUpdate()
    {
        // 플레이어를 찾지 못했으면 처리하지 않음
        if (!isPlayerFound || player == null) return;
        
        Vector3 desiredPosition = new Vector3(transform.position.x, player.position.y, transform.position.z);
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
