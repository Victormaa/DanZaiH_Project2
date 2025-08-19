using UnityEngine;
using System.Collections;

public abstract class FaderBase : MonoBehaviour
{   
    [Header("Fade Settings")]
    const float delayBeforeShowing = 0f;
    const float fadeDuration = 1f;
    const float displayDuration = 2f;

    public bool isAlphaBegin = true;

    protected abstract Color GetColor();
    protected abstract void SetColor(Color color);

    void Start()
    {
        Initialize();
    }

    protected void Initialize()
    {
        if (isAlphaBegin)
        {
            Color color = GetColor();
            color.a = 0f;
            SetColor(color);
        }
    }

    public IEnumerator FadeInAndOut(float delayBeforeShowing = delayBeforeShowing, float displayDuration = displayDuration)
    {
        yield return new WaitForSeconds(delayBeforeShowing);
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(displayDuration);
        yield return StartCoroutine(FadeOut());
    }

    public IEnumerator FadeIn(float fadeDuration = fadeDuration)
    {
        float elapsedTime = 0f;
        Color color = GetColor();
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(elapsedTime / fadeDuration);
            SetColor(color);
            yield return null;
        }
        color.a = 1f;
        SetColor(color);
    }

    public IEnumerator FadeOut(float fadeDuration = fadeDuration)
    {
        float elapsedTime = 0f;
        Color color = GetColor();
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            color.a = Mathf.Clamp01(1f - (elapsedTime / fadeDuration));
            SetColor(color);
            yield return null;
        }
        color.a = 0f;
        SetColor(color);
    }

    public void Appear()
    {
        StopAllCoroutines();
        StartCoroutine(FadeIn());
    }
    public void Disappear()
    {
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }
}