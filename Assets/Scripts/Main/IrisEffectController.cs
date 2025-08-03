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
        Vector2 startPos = circle.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            circle.position = Vector2.Lerp(startPos, targetLocalPos, t);

            float currentSize = Mathf.Lerp(startSize, endSize, t);
            circle.sizeDelta = new Vector2(currentSize, currentSize);

            yield return null;
        }

        circle.position = targetLocalPos;
        circle.sizeDelta = new Vector2(endSize, endSize);
    }

}
