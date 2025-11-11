using System.Collections;
using UnityEngine;

public class BlackoutFader : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public float defaultDuration = 1.2f;
    Coroutine _co;

    void Awake()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }

    public void FadeToBlack(float duration = -1f)
    {
        if (duration <= 0) duration = defaultDuration;
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Fade(1f, duration));
    }

    public void FadeToClear(float duration = -1f)
    {
        if (duration <= 0) duration = defaultDuration;
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Fade(0f, duration));
    }

    IEnumerator Fade(float target, float duration)
    {
        float start = canvasGroup.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        canvasGroup.alpha = target;
        canvasGroup.blocksRaycasts = target >= 0.99f;
    }
}

