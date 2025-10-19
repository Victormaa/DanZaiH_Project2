using System.Collections;
using UnityEngine;
using UnityEngine.UI; // 为了识别 UI 的 Graphic 组件（raycastTarget）

[RequireComponent(typeof(BoxCollider2D))]
public class Interactable_HighlightAndPlay : MonoBehaviour
{
    [Header("高光设置")]
    public GameObject highlightImage;              // 高光图片（SpriteRenderer 或 UI Image）
    public string playerTag = "Player";

    [Header("碰撞体（可选）")]
    [Tooltip("用于‘点击检测’的碰撞体（建议非Trigger）。如果留空，将使用本对象上的第一个 Collider2D。")]
    public Collider2D clickCollider;               // 点击用的碰撞体（非 Trigger 更稳）
    [Tooltip("用于‘靠近高亮’的触发器（Trigger）。如果留空，将使用本对象上的 BoxCollider2D（要求是Trigger）。")]
    public Collider2D proximityTrigger;            // 触发区（Trigger）

    [Header("动画与音效")]
    public Animator animator;
    public string animationStateName = "Play";
    public AudioSource audioSource;
    public AudioClip sfxClip;
    public bool freezeAfterPlay = true;

    [Header("点击检测")]
    public LayerMask clickableLayers = ~0;         // 允许点击的层（默认全部）
    public Camera cam;                             // 用于换算屏幕坐标到世界坐标；留空默认 Camera.main

    private bool isPlayerInside = false;
    private bool isPlaying = false;
    private bool hasPlayed = false;

    private void Awake()
    {
        // 摄像机兜底
        if (cam == null) cam = Camera.main;

        // 触发器确保是 Trigger
        var defaultBox = GetComponent<BoxCollider2D>();
        defaultBox.isTrigger = true;
        if (proximityTrigger == null) proximityTrigger = defaultBox;

        // 点击碰撞体兜底
        if (clickCollider == null)
        {
            // 找到第一个非 Trigger 的 Collider2D 作为点击用
            var allCols = GetComponents<Collider2D>();
            foreach (var c in allCols)
            {
                if (c.enabled && !c.isTrigger)
                {
                    clickCollider = c;
                    break;
                }
            }
            // 如果还是没找到，就退而求其次用自身的 Trigger（也能用，但不如非Trigger稳）
            if (clickCollider == null) clickCollider = defaultBox;
        }

        // 隐藏高光
        if (highlightImage != null)
        {
            highlightImage.SetActive(false);

            // 如果高光是 UI，禁止拦截点击
            var g = highlightImage.GetComponent<Graphic>();
            if (g != null) g.raycastTarget = false;
        }

        // 音源兜底
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        // 防止 Animator 自动播放
        if (animator != null)
        {
            animator.enabled = false;                  // 方案A：启动先禁用
            // 或者用方案B（定格到第0帧）：请改成
            // animator.speed = 0f;
            // animator.Play(animationStateName, 0, 0f);
            // animator.Update(0f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && other is Collider2D)
        {
            isPlayerInside = true;
            if (highlightImage != null) highlightImage.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && other is Collider2D)
        {
            isPlayerInside = false;
            if (highlightImage != null) highlightImage.SetActive(false);
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;          // 监听左键点击
        if (!isPlayerInside || isPlaying || hasPlayed) return;

        // 将鼠标位置转换到世界坐标
        if (cam == null) return;
        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p = new Vector2(wp.x, wp.y);

        // 使用 OverlapPoint 检测是否点击到了“点击碰撞体”
        // 注意：如果 clickCollider 是 Trigger，也能命中；非Trigger更稳
        if (clickCollider != null)
        {
            // 只在相同对象的碰撞体内才触发
            bool hitSelf = clickCollider.OverlapPoint(p);

            // 进一步用层级过滤（可选）
            if (hitSelf)
            {
                int objLayerMask = 1 << gameObject.layer;
                if ((clickableLayers.value & objLayerMask) == 0)
                {
                    hitSelf = false;
                }
            }

            if (hitSelf)
            {
                StartCoroutine(PlaySequence());
            }
        }
        else
        {
            // 兜底：射线检测（命中最上层2D碰撞体），检查是不是自己或自己的子层级
            RaycastHit2D hit = Physics2D.Raycast(p, Vector2.zero, 0f, clickableLayers);
            if (hit.collider != null && (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform)))
            {
                StartCoroutine(PlaySequence());
            }
        }
    }

    private IEnumerator PlaySequence()
    {
        isPlaying = true;

        // 播放音效
        if (audioSource != null && sfxClip != null)
            audioSource.PlayOneShot(sfxClip);

        // 播放动画
        if (animator != null && !string.IsNullOrEmpty(animationStateName))
        {
            animator.enabled = true;               // 方案A：启用 Animator
            animator.speed = 1f;
            animator.Play(animationStateName, 0, 0f);
            animator.Update(0f);                   // 立刻应用首帧，避免闪烁

            // 等待动画结束（Play 状态 Loop 要关闭）
            while (true)
            {
                var info = animator.GetCurrentAnimatorStateInfo(0);
                if (info.IsName(animationStateName) && info.normalizedTime >= 1f) break;
                yield return null;
            }

            if (freezeAfterPlay)
            {
                animator.Play(animationStateName, 0, 1f); // 定到最后一帧
                animator.Update(0f);
                animator.speed = 0f;                      // 冻结
            }
        }

        hasPlayed = true;
        isPlaying = false;
    }
}
