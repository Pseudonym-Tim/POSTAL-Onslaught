using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Fading screen transition...
/// </summary>
public class FadeUI : UIComponent
{
    private const float DEFAULT_FADE_TIME = 1.0f;
    public Image fadeImage;

    public static event Action OnFadeInComplete;
    public static event Action OnFadeOutComplete;

    private void Awake()
    {
        SetImageAlpha(0);
    }

    public void FadeIn(float fadeDuration = DEFAULT_FADE_TIME)
    {
        StartCoroutine(Fade(fadeDuration, true));
    }

    public void FadeOut(float fadeDuration = DEFAULT_FADE_TIME)
    {
        StartCoroutine(Fade(fadeDuration, false));
    }

    private IEnumerator Fade(float duration, bool fadeIn)
    {
        IsFading = true;

        float startAlpha = fadeIn ? 1 : 0;
        float endAlpha = fadeIn ? 0 : 1;
        float elapsed = 0;

        // Set initial alpha value...
        SetImageAlpha(startAlpha);

        // Gradually change the alpha value...
        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            SetImageAlpha(newAlpha);
            yield return null;
        }

        // Ensure the final alpha value is set...
        SetImageAlpha(endAlpha); 
        
        if(fadeIn)
        {
            OnFadeInComplete?.Invoke();
        }
        else
        {
            OnFadeOutComplete?.Invoke();
        }

        IsFading = false;
    }

    private void SetImageAlpha(float alpha)
    {
        if(fadeImage != null)
        {
            Color color = fadeImage.color;
            color.a = alpha;
            fadeImage.color = color;
        }
    }

    public bool IsFading { get; set; } = false;
}
