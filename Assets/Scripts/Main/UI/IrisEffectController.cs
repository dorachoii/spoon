using UnityEngine;
using System.Collections;

public class IrisEffector : MonoBehaviour
{
    private Transform playerTransform;
    private bool isPlayerFound = false;

    [Header("Iris Effect Settings")]
    public RectTransform circle;
    private float startSize = 3000f;
    private float endSize = 400f;
    public float duration = 2f;

    void Start()
    {
        StartCoroutine(FindPlayerCoroutine());
    }
    
    private IEnumerator FindPlayerCoroutine()
    {
        // 플레이어가 생성될 때까지 대기
        while (playerTransform == null)
        {
            if (GameObject.FindGameObjectWithTag("Player")!= null)
            {
                playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
                isPlayerFound = true;
                break;
            }
            
            yield return null;
        }
    }

    void Update()
    {
        if (!isPlayerFound) return;
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
