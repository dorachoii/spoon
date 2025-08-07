using UnityEngine;
using System.Collections;

public class IrisEffector : MonoBehaviour
{
    private Transform playerTransform;

    [Header("Iris Effect Settings")]
    public RectTransform circle;
    private float startSize = 3000f;
    private float endSize = 400f;
    public float duration = 2f;

    //TODO:FindGameObjectWithTag 대신 캐싱 필요
    void Update()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void IrisIn()
    {
        StartCoroutine(AnimateIrisIn());
    }

    // iris in: 플레이어를 향해 작아지는 원 (プレイヤーに向かって縮小する円)
    private IEnumerator AnimateIrisIn()
    {
        float elapsed = 0f;
        Vector2 startPos = circle.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);    // Normalize(0~1)

            circle.position = Vector2.Lerp(startPos, playerTransform.position, t);

            float currentSize = Mathf.Lerp(startSize, endSize, t);
            circle.sizeDelta = new Vector2(currentSize, currentSize);

            yield return null;
        }
        
        circle.position = playerTransform.position;
        circle.sizeDelta = new Vector2(endSize, endSize);
    }

}
