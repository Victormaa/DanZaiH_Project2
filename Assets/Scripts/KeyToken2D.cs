using System.Collections;
using UnityEngine;

/// <summary>
/// 挂在“钥匙A”上的脚本：可被接收后渐隐并消耗
/// 需要：SpriteRenderer +（已有的）Draggable2D + Collider2D
/// 最好有 Rigidbody2D（Kinematic），以便触发器工作
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class KeyToken2D : MonoBehaviour
{
    [Header("基本")]
    public string keyId = "CabinetKey";      // 可留作标识
    public float fadeDuration = 0.35f;       // 渐隐时长
    public bool destroyOnConsumed = true;    // 渐隐结束后销毁自己

    [Header("可选音频")]
    public AudioClip vanishSfx;              // 渐隐结束的提示音（也可不用）
    private AudioSource _audio;

    [HideInInspector] public bool IsConsumed = false;

    SpriteRenderer _sr;
    Collider2D _col;
    MonoBehaviour _drag; // 例如 Draggable2D

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        _col = GetComponent<Collider2D>();
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();

        // 找一个拖拽脚本禁用用
        _drag = GetComponent<MonoBehaviour>(); // 这里会拿到第一个MonoBehaviour
        // 如果你项目里拖拽脚本名叫 Draggable2D，建议用：
        var d = GetComponent("Draggable2D") as MonoBehaviour;
        if (d != null) _drag = d;
    }

    /// <summary>被接收时调用：禁用拖拽与碰撞→渐隐→（可选销毁）</summary>
    public IEnumerator ConsumeAndFade()
    {
        if (IsConsumed) yield break;
        IsConsumed = true;

        if (_drag != null) _drag.enabled = false; // 禁止继续拖动
        if (_col != null) _col.enabled = false;   // 不再触发碰撞

        if (_sr != null)
        {
            var c = _sr.color;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float a = Mathf.Lerp(1f, 0f, t / fadeDuration);
                _sr.color = new Color(c.r, c.g, c.b, a);
                yield return null;
            }
            _sr.color = new Color(c.r, c.g, c.b, 0f);
        }

        // 渐隐结束音（可选）
        if (vanishSfx != null)
            _audio.PlayOneShot(vanishSfx);

        if (destroyOnConsumed)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);
    }
}
