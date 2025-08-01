using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class IrisEffectController : MonoBehaviour
{
    private Transform playerTransform;

    public RectTransform circle;
    private float startSize = 3000f;
    private float endSize = 400f;
    public float duration = 2f;

    private bool isAnimating = false;

    void Update()
    {
         playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void IrisIn()
    {
        StartCoroutine(ShrinkCircle());
    }

    private IEnumerator ShrinkCircle()
    {
        Vector2 targetLocalPos = playerTransform.position;
        float elapsed = 0f;
        Vector2 startPos = circle.localPosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // 위치 보간
            circle.localPosition = Vector2.Lerp(startPos, targetLocalPos, t);

            // 크기 보간
            float currentSize = Mathf.Lerp(startSize, endSize, t);
            circle.sizeDelta = new Vector2(currentSize, currentSize);

            yield return null;
        }

        // 최종 정리
        circle.localPosition = targetLocalPos;
        circle.sizeDelta = new Vector2(endSize, endSize);
    }

}
