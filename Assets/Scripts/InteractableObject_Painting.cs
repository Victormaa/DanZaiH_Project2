using System.Collections;
using UnityEngine;
using UnityEngine.UI; // 让我们能把 UI 的 raycastTarget 关掉，避免吃点击

[RequireComponent(typeof(BoxCollider2D))]
public class Interactable_HighlightAndPlay : MonoBehaviour
{
    [Header("高光设置")]
    public GameObject highlightImage;              // 高光图片（SpriteRenderer 或 UI Image）
    public string playerTag = "Player";
    [Tooltip("点击后高光显示的时间（秒），结束后播放动画")]
    public float highlightFlashDuration = 0.2f;    // 点击后高光闪现时长

    [Header("碰撞体（可选）")]
    [Tooltip("用于‘点击检测’的碰撞体（建议非Trigger）。如果留空，将使用本对象上的第一个 Collider2D。")]
    public Collider2D clickCollider;               // 点击用碰撞体
    [Tooltip("用于‘靠近判定’的触发器（Trigger）。如果留空，将使用本对象上的 BoxCollider2D（要求是Trigger）。")]
    public Collider2D proximityTrigger;            // 近身触发器（只用于判定，不再控制高光显示）

    [Header("动画与音效")]
    public Animator animator;
    public string animationStateName = "Play";
    public AudioSource audioSource;
    public AudioClip sfxClip;
    public bool freezeAfterPlay = true;

    [Header("点击检测")]
    public LayerMask clickableLayers = ~0;         // 允许点击的层（默认全部）
    public Camera cam;                             // 不填则用 Camera.main

    private bool isPlayerInside = false;
    private bool isPlaying = false;
    private bool hasPlayed = false;

    private void Awake()
    {
        // 摄像机兜底
        if (cam == null) cam = Camera.main;

        // 近身触发器：默认用本体 BoxCollider2D，并确保是 Trigger
        var defaultBox = GetComponent<BoxCollider2D>();
        defaultBox.isTrigger = true;
        if (proximityTrigger == null) proximityTrigger = defaultBox;

        // 点击碰撞体兜底：优先找非 Trigger 的 Collider2D
        if (clickCollider == null)
        {
            var allCols = GetComponents<Collider2D>();
            foreach (var c in allCols)
            {
                if (c.enabled && !c.isTrigger)
                {
                    clickCollider = c;
                    break;
                }
            }
            if (clickCollider == null) clickCollider = defaultBox; // 没有就用默认的 Trigger 也可
        }

        // 启动时隐藏高光（重要：不再在触发器事件里显示）
        if (highlightImage != null)
        {
            highlightImage.SetActive(false);
            var g = highlightImage.GetComponent<Graphic>();
            if (g != null) g.raycastTarget = false; // 如果是 UI，避免吃点击
        }

        // 音源兜底
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 防止 Animator 自动播放（方案A：启动先禁用）
        if (animator != null) animator.enabled = false;

        // 可选：确保闪现时长合法
        if (highlightFlashDuration < 0f) highlightFlashDuration = 0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = true;
            // 注意：不在这里显示高光
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerInside = false;
            // 玩家离开时，确保高光被关掉（防御性）
            if (highlightImage != null) highlightImage.SetActive(false);
        }
    }

    private void Update()
    {
        // 只在玩家在范围内，且未播放过，且未在播放中时响应点击
        if (!Input.GetMouseButtonDown(0)) return;
        if (!isPlayerInside || isPlaying || hasPlayed) return;
        if (cam == null) return;

        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = new Vector2(wp.x, wp.y);

        bool hitSelf = false;

        if (clickCollider != null)
        {
            hitSelf = clickCollider.OverlapPoint(p);

            // 层级过滤（可选）
            if (hitSelf)
            {
                int objLayerMask = 1 << gameObject.layer;
                if ((clickableLayers.value & objLayerMask) == 0)
                    hitSelf = false;
            }
        }
        else
        {
            // 兜底射线
            RaycastHit2D hit = Physics2D.Raycast(p, Vector2.zero, 0f, clickableLayers);
            if (hit.collider != null && (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)))
                hitSelf = true;
        }

        if (hitSelf)
        {
            StartCoroutine(FlashHighlightThenPlay());
        }
    }

    private IEnumerator FlashHighlightThenPlay()
    {
        isPlaying = true;

        // 1) 点击反馈：高光闪现一下
        if (highlightImage != null)
        {
            highlightImage.SetActive(true);
            if (highlightFlashDuration > 0f)
                yield return new WaitForSeconds(highlightFlashDuration);
            highlightImage.SetActive(false);
        }

        // 2) 音效（和动画可以同时开始，也可以先后）
        if (audioSource != null && sfxClip != null)
            audioSource.PlayOneShot(sfxClip);

        // 3) 播放动画（并在最后一帧定格）
        if (animator != null && !string.IsNullOrEmpty(animationStateName))
        {
            animator.enabled = true;               // 方案A启用 Animator
            animator.speed = 1f;
            animator.Play(animationStateName, 0, 0f);
            animator.Update(0f);                   // 立刻应用首帧

            // 等待动画结束（要求 Play 状态 Loop 关闭）
            while (true)
            {
                var info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName(animationStateName) && info.normalizedTime >= 1f) break;
                yield return null;
            }

            if (freezeAfterPlay)
            {
                animator.Play(animationStateName, 0, 1f); // 跳到最后一帧
                animator.Update(0f);
                animator.speed = 0f;                      // 冻结
            }
        }

        hasPlayed = true;
        isPlaying = false;
    }
}
