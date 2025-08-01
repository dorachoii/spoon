using System.Collections;
using System.Collections.Generic;
using UnityEditor.ShaderGraph;
using UnityEngine;

public enum SpriteEffectColor
{
    Red,
    Green
}

public static class SpriteEffectColorExtensions
{
    public static Color ToColor(this SpriteEffectColor effectColor)
    {
        return effectColor switch
        {
            SpriteEffectColor.Red => new Color(1f, 0.4f, 0.4f),
            SpriteEffectColor.Green => new Color(0.4f, 1f, 0.4f),
            _ => Color.white,
        };
    }
}
public class SpriteColorEffect : MonoBehaviour
{
    Color red;

    void Awake()
    {
        red = new Color(1f, 0.4f, 0.4f);
    }
    public IEnumerator IFlicker(SpriteRenderer spriteRenderer, SpriteEffectColor effectColor = SpriteEffectColor.Red, float duration = 1f)
    {
        Color color = effectColor.ToColor();
        float interval = 0.2f;
        float elapsed = 0f;
        Color origin = spriteRenderer.color;

        if (duration <= 0f)
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
            spriteRenderer.color = origin;
        }
    }

    public IEnumerator IRainbowEffect(SpriteRenderer spriteRenderer, float duration = 0f, float hueSpeed = 2f)
    {
        Color origin = spriteRenderer.color;
        float currentHue = 0f;
        float elapsed = 0f;

        while (duration <= 0f || elapsed < duration)
        {
            currentHue += hueSpeed * Time.deltaTime;
            if (currentHue > 1f) currentHue -= 1f;

            Color rainbowColor = Color.HSVToRGB(currentHue, 1f, 1f);
            spriteRenderer.color = rainbowColor;

            elapsed += Time.deltaTime;
            yield return null; 
        }

        spriteRenderer.color = origin;
    }

}
