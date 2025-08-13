using UnityEngine;
using TMPro;
using System.Collections;

public class TextBlinkWithPause : MonoBehaviour
{
    public TextMeshProUGUI text;   // 깜빡일 텍스트
    public float fadeDuration = 1f; // 서서히 나타나고/사라지는 시간
    public float pauseDuration = 1f; // 깜빡임 사이 대기 시간

    private void Reset()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        StartCoroutine(BlinkLoop());
    }

    private IEnumerator BlinkLoop()
    {
        while (true)
        {
            // 페이드 인
            yield return StartCoroutine(FadeAlpha(0f, 1f, fadeDuration));

            // 페이드 아웃
            yield return StartCoroutine(FadeAlpha(1f, 0f, fadeDuration));

            // 대기
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private IEnumerator FadeAlpha(float start, float end, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(start, end, t / duration);

            Color c = text.faceColor;
            c.a = alpha;
            text.faceColor = c;

            yield return null;
        }
    }
}
