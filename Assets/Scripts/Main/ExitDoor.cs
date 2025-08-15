using UnityEngine;
using System.Collections;

public class ExitDoor : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDuration = 2f;
    
    private bool isMovingToDoor = false;
    private Transform player;
    
    private void OnMouseDown()
    {
        if (isMovingToDoor) return;
        
        // 플레이어 찾기
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        if (player != null)
        {
            StartCoroutine(MovePlayerToDoor());
        }
    }
    
    private IEnumerator MovePlayerToDoor()
    {
        isMovingToDoor = true;
        
        Vector3 startPos = player.position;
        Vector3 targetPos = transform.position;
        float elapsed = 0f;
        
        // 부드러운 이동
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            
            // 부드러운 이징 적용
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            player.position = Vector3.Lerp(startPos, targetPos, smoothT);
            
            yield return null;
        }
        
        // 정확한 위치로 설정
        player.position = targetPos;
        
        // 트리거 대신 직접 게임 클리어 처리
        GameManager.Instance.SetGameCleared();
        GameManager.Instance.BackToTitle();
        
        isMovingToDoor = false;
    }
    
 
}
