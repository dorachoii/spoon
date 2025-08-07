using System.Collections;
using UnityEngine;

public enum PlayerColor
{
    Red,
    Green
}

public static class PlayerColorExtensions
{
    public static Color ToColor(this PlayerColor effectColor)
    {
        return effectColor switch
        {
            PlayerColor.Red => new Color(1f, 0.4f, 0.4f),
            PlayerColor.Green => new Color(31f/255f, 230f/255f, 178f/255f),
            _ => Color.white,
        };
    }
}

public class SpriteColorEffector : MonoBehaviour
{
    //flicker : 지뢰, 플레이어 데미지 등 (地雷、player damageなど)
    public IEnumerator IFlicker(SpriteRenderer spriteRenderer, PlayerColor effectColor = PlayerColor.Red, float duration = 1f, bool loop = false)
    {
        Color color = effectColor.ToColor();
        float interval = 0.2f;
        float elapsed = 0f;
        Color origin = spriteRenderer.color;

        if (loop)
        {
            while (true)
            {
                spriteRenderer.color = color;
                yield return new WaitForSeconds(interval / 2f);
                spriteRenderer.color = origin;
                yield return new WaitForSeconds(interval / 2f);
            }
        }
        else
        {
            while (elapsed < duration)
            {
                spriteRenderer.color = color;
                yield return new WaitForSeconds(interval / 2f);
                spriteRenderer.color = origin;
                yield return new WaitForSeconds(interval / 2f);

                elapsed += interval;
            }
        }
        spriteRenderer.color = origin;
    }

    // rainbow: 무적 상태 등 (無敵状態など)
    public IEnumerator IRainbow(SpriteRenderer spriteRenderer, float duration = 0f, float hueSpeed = 2f, Color? tint = null, bool loop = false)
    {
        Color origin = spriteRenderer.color;
        float currentHue = 0f;
        float elapsed = 0f;
        Color actualTint = tint ?? Color.white;

        if (loop)
        {
            while (true)
            {
                currentHue += hueSpeed * Time.deltaTime;
                if (currentHue > 1f) currentHue -= 1f;

                Color rainbowColor = Color.HSVToRGB(currentHue, 1f, 1f);
                spriteRenderer.color = MultiplyColors(rainbowColor, actualTint);

                yield return null;
            }
        }
        else
        {
            while (duration <= 0f || elapsed < duration)
            {
                currentHue += hueSpeed * Time.deltaTime;
                if (currentHue > 1f) currentHue -= 1f;

            Color rainbowColor = Color.HSVToRGB(currentHue, 1f, 1f);
            spriteRenderer.color = MultiplyColors(rainbowColor, actualTint);

            elapsed += Time.deltaTime;
            yield return null;
        }

            spriteRenderer.color = origin;
        }
    }

    private Color MultiplyColors(Color a, Color b) {
        return new Color(a.r * b.r, a.g * b.g, a.b * b.b, 1f);
    }
}
