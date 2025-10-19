using System.Collections;
using UnityEngine;
using UnityEngine.UI; // 处理 UI Graphic 的 raycastTarget / 透明度

/// <summary>
/// 点击后：高光闪现 → 同步播放 A 与 B 的动画 → A、B 都定格在各自最后一帧；
/// 其中 B 在 A 开始播放之前一直保持完全透明（alpha=0），A 开始播放时让 B 显示并开始播放。
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class Interactable_HighlightAndPlay_ClickOnly : MonoBehaviour
{
    [Header("玩家/范围判定")]
    public Collider2D proximityTrigger;   // 触发器（Trigger）
    public Collider2D clickCollider;      // 点击碰撞体（非Trigger）
    public string playerTag = "Player";

    [Header("高光设置")]
    public GameObject highlightImage;     // 可为 SpriteRenderer 或 UI Image 对象
    public float highlightFlashDuration = 0.2f;

    [Header("动画与音效（A：当前物体）")]
    public Animator animator;
    public string animationStateName = "Play";
    public bool freezeAfterPlay = true;
    public AudioSource audioSource;
    public AudioClip sfxClip;

    [Header("点击设置")]
    public LayerMask clickableLayers = ~0;
    public Camera cam;
    public bool requirePlayerInside = true;

    // ================= 新增：B 同播与可见性控制 =================
    [Header("同步播放（B：另一个物体的动画）")]
    [Tooltip("物品B的Animator（可与本物体不同）。为空则不播放B。")]
    public Animator nextAnimator;
    public string nextAnimationStateName = "Play";
    public bool nextFreezeAfterPlay = true;

    [Tooltip("是否由脚本托管B的Animator启停。若为true，Awake时禁用它，等A/B播放时再启用。")]
    public bool manageNextAnimatorLifecycle = true;

    [Tooltip("是否在A未播放前让B保持完全透明(alpha=0)。为true时，Awake里会设置为透明，播放开始时置为alpha=1。")]
    public bool makeBInvisibleUntilPlay = true;

    [Tooltip("B 透明度控制的根物体（可选）。不填则默认使用 nextAnimator.gameObject。")]
    public GameObject bVisualRoot;

    [Tooltip("若为UI渲染，优先尝试CanvasGroup；否则自动遍历子节点的 Graphic 或 SpriteRenderer 调整alpha。")]
    public bool preferCanvasGroupForUI = true;

    // （可选）B 的音效
    public bool playNextSfx = false;
    public AudioSource nextAudioSource;
    public AudioClip nextSfxClip;

    // ===========================================================

    private bool isPlayerInside = false;
    private bool isPlaying = false;
    private bool hasPlayed = false;

    private BoxCollider2D _box;
    private CanvasGroup _bCanvasGroup;
    private Graphic[] _bGraphics;
    private SpriteRenderer[] _bSprites;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;

        _box = GetComponent<BoxCollider2D>();
        if (proximityTrigger == null)
        {
            proximityTrigger = _box;
            proximityTrigger.isTrigger = true;
        }
        else
        {
            proximityTrigger.isTrigger = true;
        }

        if (clickCollider == null)
        {
            var allCols = GetComponents<Collider2D>();
            foreach (var c in allCols)
            {
                if (c.enabled && !c.isTrigger) { clickCollider = c; break; }
            }
            if (clickCollider == null) clickCollider = _box; // 兜底
        }

        // 高光不挡鼠标
        if (highlightImage != null)
        {
            highlightImage.SetActive(false);
            var g = highlightImage.GetComponent<Graphic>();
            if (g != null) g.raycastTarget = false;
        }

        // A：音源兜底
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // A：Animator 防误播
        if (animator != null)
        {
            animator.enabled = false;
            animator.speed = 1f;
        }

        // B：Animator 生命周期托管（避免一进Play就自动播）
        if (manageNextAnimatorLifecycle && nextAnimator != null)
        {
            nextAnimator.enabled = false;
            nextAnimator.speed = 1f;
        }

        // 初始化 B 的可见性通道
        if (nextAnimator != null)
        {
            if (bVisualRoot == null) bVisualRoot = nextAnimator.gameObject;

            // 尝试拿 CanvasGroup
            if (preferCanvasGroupForUI)
            {
                _bCanvasGroup = bVisualRoot.GetComponent<CanvasGroup>();
                if (_bCanvasGroup == null) _bCanvasGroup = bVisualRoot.AddComponent<CanvasGroup>(); // 可按需添加
            }

            // 采集可能的渲染组件
            _bGraphics = bVisualRoot.GetComponentsInChildren<Graphic>(true);
            _bSprites = bVisualRoot.GetComponentsInChildren<SpriteRenderer>(true);

            // 在 A 未播放前，让 B 透明
            if (makeBInvisibleUntilPlay)
            {
                SetBAlphaImmediate(0f);
            }
        }

        if (highlightFlashDuration < 0f) highlightFlashDuration = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag)) isPlayerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
            if (highlightImage != null) highlightImage.SetActive(false);
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (requirePlayerInside && !isPlayerInside) return;
        if (isPlaying || hasPlayed) return;
        if (cam == null) return;

        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = new Vector2(wp.x, wp.y);

        bool hitSelf = false;

        if (clickCollider != null)
        {
            hitSelf = clickCollider.OverlapPoint(p);
            if (hitSelf)
            {
                int objLayerMask = 1 << gameObject.layer;
                if ((clickableLayers.value & objLayerMask) == 0) hitSelf = false;
            }
        }
        else
        {
            RaycastHit2D hit = Physics2D.Raycast(p, Vector2.zero, 0f, clickableLayers);
            if (hit.collider != null && (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)))
                hitSelf = true;
        }

        if (hitSelf) StartCoroutine(Co_FlashThenPlayABTogether());
    }

    private IEnumerator Co_FlashThenPlayABTogether()
    {
        isPlaying = true;

        // 1) 点击反馈：高光闪现
        if (highlightImage != null)
        {
            highlightImage.SetActive(true);
            if (highlightFlashDuration > 0f)
                yield return new WaitForSeconds(highlightFlashDuration);
            highlightImage.SetActive(false);
        }

        // 2) 播放 A 的音效（如有）
        if (audioSource != null && sfxClip != null) audioSource.PlayOneShot(sfxClip);

        // 3) 同步启动 A 和 B
        // A
        if (animator != null && !string.IsNullOrEmpty(animationStateName))
        {
            animator.enabled = true;
            animator.speed = 1f;
            animator.Play(animationStateName, 0, 0f);
            animator.Update(0f);
        }

        // B（先显示，再播放）
        if (nextAnimator != null && !string.IsNullOrEmpty(nextAnimationStateName))
        {
            // 可选：B 的音效
            if (playNextSfx)
            {
                if (nextAudioSource == null)
                {
                    nextAudioSource = nextAnimator.GetComponent<AudioSource>();
                    if (nextAudioSource == null)
                    {
                        nextAudioSource = nextAnimator.gameObject.AddComponent<AudioSource>();
                        nextAudioSource.playOnAwake = false;
                    }
                }
                if (nextSfxClip != null) nextAudioSource.PlayOneShot(nextSfxClip);
            }

            if (manageNextAnimatorLifecycle && !nextAnimator.enabled)
                nextAnimator.enabled = true;

            // A 开始的同一帧让 B 可见
            if (makeBInvisibleUntilPlay)
                SetBAlphaImmediate(1f);

            nextAnimator.speed = 1f;
            nextAnimator.Play(nextAnimationStateName, 0, 0f);
            nextAnimator.Update(0f);
        }

        // 4) 等待 A 与 B 都播放完
        // （若某一方不存在就只等待另一方）
        if (animator != null && !string.IsNullOrEmpty(animationStateName))
            yield return StartCoroutine(WaitForStateDone(animator, animationStateName));

        if (nextAnimator != null && !string.IsNullOrEmpty(nextAnimationStateName))
            yield return StartCoroutine(WaitForStateDone(nextAnimator, nextAnimationStateName));

        // 5) 双方均定格在最后一帧（按需）
        if (animator != null && freezeAfterPlay)
        {
            animator.Play(animationStateName, 0, 1f);
            animator.Update(0f);
            animator.speed = 0f;
        }
        if (nextAnimator != null && nextFreezeAfterPlay)
        {
            nextAnimator.Play(nextAnimationStateName, 0, 1f);
            nextAnimator.Update(0f);
            nextAnimator.speed = 0f;
        }

        hasPlayed = true;
        isPlaying = false;
    }

    /// <summary>等待进入并播放完指定状态（不在过渡中，normalizedTime ≥ 1）。</summary>
    private IEnumerator WaitForStateDone(Animator ani, string stateName, int layer = 0)
    {
        // 等待进入目标状态且不在过渡
        while (true)
        {
            var info = ani.GetCurrentAnimatorStateInfo(layer);
            if (info.IsName(stateName) && !ani.IsInTransition(layer)) break;
            yield return null;
        }
        // 等待播放完成
        while (true)
        {
            var info = ani.GetCurrentAnimatorStateInfo(layer);
            if (info.IsName(stateName) && !ani.IsInTransition(layer) && info.normalizedTime >= 1f) break;
            yield return null;
        }
    }

    #region —— B 透明度工具 —— 
    private void SetBAlphaImmediate(float a)
    {
        if (bVisualRoot == null) return;

        // 1) 优先 CanvasGroup（最稳，能同时控制UI/子层级）
        if (_bCanvasGroup != null)
        {
            _bCanvasGroup.alpha = Mathf.Clamp01(a);
        }
        else
        {
            // 2) UI Graphic
            if (_bGraphics != null && _bGraphics.Length > 0)
            {
                for (int i = 0; i < _bGraphics.Length; i++)
                {
                    if (_bGraphics[i] == null) continue;
                    var c = _bGraphics[i].color;
                    c.a = Mathf.Clamp01(a);
                    _bGraphics[i].color = c;
                }
            }
            // 3) SpriteRenderer
            if (_bSprites != null && _bSprites.Length > 0)
            {
                for (int i = 0; i < _bSprites.Length; i++)
                {
                    if (_bSprites[i] == null) continue;
                    var c = _bSprites[i].color;
                    c.a = Mathf.Clamp01(a);
                    _bSprites[i].color = c;
                }
            }
        }
    }
    #endregion
}
