using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class StatusTextAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Animation Settings")]
    [SerializeField] private float duration = 1f;
    private float startY = 1f;
    private float endY = 0.5f;

    private Coroutine animationCoroutine;
    private RectTransform rect;

    private void Awake()
    {
        tmp = GetComponentInChildren<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
    }

    public void Initialize(string text, Color color)
    {
        tmp.text = text;
        tmp.color = color;
        canvasGroup.alpha = 1f;

        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(PlayAnimation());
    }

    // fade out & fall
    private IEnumerator PlayAnimation()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            float currentY = Mathf.Lerp(startY, endY, t);
            Vector3 local = rect.localPosition;
            local.y = currentY;
            rect.localPosition = local;
            canvasGroup.alpha = Mathf.SmoothStep(1f, 0f, t);

            yield return null;
        }

        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
