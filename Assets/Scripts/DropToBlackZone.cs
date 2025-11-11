using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DropZone2D_InteractFade : MonoBehaviour
{
    [Header("检测与过滤")]
    public string playerTag = "Player";        // 玩家Tag（用于高光显示）
    public bool requireExactType = false;      // 勾上=必须带Draggable2D组件
    public string draggableTag = "Draggable";  // 不勾时，用Tag过滤可拖拽物
    public bool triggerOnlyOnRelease = true;   // 只在松手时判定投放

    [Header("高光显示")]
    public GameObject highlightGO;             // 高光图片（B的子物体，默认隐藏）

    [Header("音频与黑屏")]
    public AudioSource audioSource;            // 可放在B上
    public AudioClip dropSfx;                  // 投放成功的音效
    public BlackoutFader fader;                // 拖BlackoutFader进来
    public float fadeDuration = 1.2f;

    [Header("A的处理")]
    public bool destroyA = true;               // ✔销毁；否则仅SetActive(false)
    public bool triggerOnce = true;            // ✔只触发一次

    // 内部状态
    readonly HashSet<Collider2D> _draggablesInside = new();
    int _playerInsideCount = 0;
    bool _fired = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;                  // 很关键：B是触发器
    }

    void OnEnable()
    {
        if (highlightGO) highlightGO.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 玩家进来 → 高光开启
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
        {
            _playerInsideCount++;
            if (highlightGO) highlightGO.SetActive(true);
        }

        // 可拖拽物进来 → 记入集合
        if (IsValidDraggable(other))
            _draggablesInside.Add(other);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(playerTag) && other.CompareTag(playerTag))
        {
            _playerInsideCount = Mathf.Max(0, _playerInsideCount - 1);
            if (_playerInsideCount == 0 && highlightGO) highlightGO.SetActive(false);
        }

        if (_draggablesInside.Contains(other))
            _draggablesInside.Remove(other);
    }

    void Update()
    {
        if (_fired && triggerOnce) return;

        bool released = false;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        released = Input.GetMouseButtonUp(0);
#else
        // 移动端：任一触点结束即视为松手
        for (int i = 0; i < Input.touchCount; i++)
            if (Input.GetTouch(i).phase == TouchPhase.Ended) { released = true; break; }
#endif

        if (triggerOnlyOnRelease)
        {
            if (released && _draggablesInside.Count > 0)
                HandleDrop();
        }
        else
        {
            if (_draggablesInside.Count > 0)
                HandleDrop();
        }
    }

    bool IsValidDraggable(Collider2D c)
    {
        if (requireExactType)
        {
            // 不修改你的Draggable2D脚本，只是检查是否存在该组件
            return c.GetComponent<Draggable2D>() != null;
        }
        else
        {
            // 用Tag过滤，给A标成"Draggable"
            return string.IsNullOrEmpty(draggableTag) || c.CompareTag(draggableTag);
        }
    }

    void HandleDrop()
    {
        // 取一个候选A（如果你只允许特定A，改成白名单或直接引用）
        Collider2D any = null;
        foreach (var c in _draggablesInside) { any = c; break; }
        if (any == null) return;

        var goA = any.attachedRigidbody ? any.attachedRigidbody.gameObject : any.gameObject;

        // 播音效
        if (audioSource && dropSfx) audioSource.PlayOneShot(dropSfx);

        // A消失
        if (destroyA) Destroy(goA);
        else goA.SetActive(false);

        // 黑屏
        if (fader) fader.FadeToBlack(fadeDuration);

        _fired = true;
    }
}
