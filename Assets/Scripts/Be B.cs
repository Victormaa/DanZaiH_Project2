using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
public class ItemBController : MonoBehaviour
{
    [Header("渐显设置")]
    [Range(0.05f, 5f)] public float fadeDuration = 0.6f;

    [Header("可选帧动画（留空则只做渐显）")]
    public Sprite[] frames;
    [Range(1, 60)] public int frameRate = 12;

    [Header("初始是否隐藏")]
    public bool startHidden = true;

    SpriteRenderer _sr;
    Coroutine _playCo;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (startHidden)
        {
            SetAlpha(0f);
            if (frames != null && frames.Length > 0) _sr.sprite = frames[0];
            gameObject.SetActive(false); // 初始可见性
        }
    }

    public void ShowAt(Vector3 worldPos)
    {
        transform.position = worldPos;

        // 确保激活并从头开始
        gameObject.SetActive(true);
        if (_playCo != null) StopCoroutine(_playCo);
        _playCo = StartCoroutine(Co_FadeAndMaybeAnimate());
    }

    IEnumerator Co_FadeAndMaybeAnimate()
    {
        // 1) 渐显（0→1）
        float t = 0f;
        SetAlpha(0f);

        // 若需要同步帧动画，准备帧控制
        bool doFrames = frames != null && frames.Length > 0;
        int frameCount = doFrames ? frames.Length : 0;
        float frameTimer = 0f;
        float frameInterval = doFrames ? 1f / Mathf.Max(1, frameRate) : 0f;
        int frameIndex = 0;
        if (doFrames) _sr.sprite = frames[0];

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / fadeDuration);
            SetAlpha(a);

            if (doFrames)
            {
                frameTimer += Time.deltaTime;
                while (frameTimer >= frameInterval && frameIndex < frameCount - 1)
                {
                    frameTimer -= frameInterval;
                    frameIndex++;
                    _sr.sprite = frames[frameIndex];
                }
            }

            yield return null;
        }

        // 2) 保证最终状态：Alpha=1，帧停在最后一帧
        SetAlpha(1f);
        if (doFrames) _sr.sprite = frames[frameCount - 1];

        // 3) 冻结：什么也不做即可（不再切换帧，不再改透明度）
        _playCo = null;
    }

    void SetAlpha(float a)
    {
        var c = _sr.color;
        c.a = a;
        _sr.color = c;
    }
}
