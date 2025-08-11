using UnityEngine;
using System.Collections;

public class IrisEffector : MonoBehaviour
{
    private Transform playerTransform;
    private bool isPlayerReady = false;

    [Header("Iris Effect Settings")]
    public RectTransform circle;
    private float startSize = 3000f;
    private float endSize = 400f;
    public float duration = 2f;

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
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    //TODO:FindGameObjectWithTag 대신 캐싱 필요
    void Update()
    {
        // 플레이어가 준비되지 않았으면 처리하지 않음
        if (!isPlayerReady)
        {
            return;
        }
        
        // 플레이어가 null이거나 파괴되었으면 다시 찾기
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }
    }

    public void IrisIn()
    {
        StartCoroutine(AnimateIrisIn());
    }

    // iris in: 플레이어를 향해 작아지는 원 (プレイヤーに向かって縮小する円)
    private IEnumerator AnimateIrisIn()
    {
        // playerTransform이 null이면 기본 위치 사용
        Vector2 targetPosition = Vector2.zero;
        if (playerTransform != null)
        {
            targetPosition = playerTransform.position;
        }
        
        float elapsed = 0f;
        Vector2 startPos = circle.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);    // Normalize(0~1)

            circle.position = Vector2.Lerp(startPos, targetPosition, t);

            float currentSize = Mathf.Lerp(startSize, endSize, t);
            circle.sizeDelta = new Vector2(currentSize, currentSize);

            yield return null;
        }
        
        circle.position = targetPosition;
        circle.sizeDelta = new Vector2(endSize, endSize);
    }

}
