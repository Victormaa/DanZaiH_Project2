using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
public class Interactable_DialogueObject2D : MonoBehaviour
{
    [Header("玩家识别")]
    public string playerTag = "Player";

    [Header("高光")]
    [Tooltip("放你的高光图片的对象：可用子物体的 SpriteRenderer 或 UI Image。留空则不显示高光。")]
    public GameObject highlightObject;
    [Tooltip("进入/离开时是否做淡入淡出")]
    public bool useFadeForHighlight = true;
    [Range(1f, 20f)] public float highlightFadeSpeed = 10f;
    private SpriteRenderer highlightSR;
    private Image highlightImg;
    private float targetHighlightAlpha = 0f;

    [Header("交互")]
    [Tooltip("是否只触发一次对话")]
    public bool oneShot = false;
    private bool hasTriggeredOnce = false;

    [Tooltip("是否允许按 F 键交互（与点击并存）——本版默认关闭")]
    public bool allowKeyInteract = false;          // 默认关闭键盘触发
    public KeyCode interactKey = KeyCode.F;

    [Header("对话管理器（引用你场景中的 DialogueManager）")]
    public DialogueManager dialogueManager;

    [Header("本物体的对话内容（会在触发时注入到 DialogueManager）")]
    public List<DialogueLine> objectDialogueLines = new List<DialogueLine>();

    // 状态
    private bool isPlayerInRange = false;
    private Collider2D playerColliderInRange;

    private void Awake()
    {
        // 高光组件缓存
        if (highlightObject != null)
        {
            highlightSR = highlightObject.GetComponent<SpriteRenderer>();
            highlightImg = highlightObject.GetComponent<Image>();

            // 初始隐藏
            SetHighlightAlphaImmediate(0f);

            // 关键：UI 高光不吃射线，避免挡住点击
            if (highlightImg != null)
            {
                highlightImg.raycastTarget = false;
            }

            highlightObject.SetActive(true); // 让它可控，但初始透明
        }

        // 确保 BoxCollider2D 为触发器
        var box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    private void Update()
    {
        // 高光淡入/淡出
        if (useFadeForHighlight && (highlightSR != null || highlightImg != null))
        {
            float curA = GetHighlightAlpha();
            float nextA = Mathf.MoveTowards(curA, targetHighlightAlpha, highlightFadeSpeed * Time.deltaTime);
            SetHighlightAlphaImmediate(nextA);
        }

        // （可选）键盘交互 —— 默认关闭
        if (allowKeyInteract && isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            TryStartDialogue();
        }
    }

    // 鼠标点击（需要物体上有 2D Collider；OnMouseDown 对 2D 一样生效）
    private void OnMouseDown()
    {
        // 只允许在触发区内点击
        if (!isPlayerInRange) return;

        // 加一道保险：确保点击到的就是自己（避免叠层误触）
        var cam = Camera.main;
        if (cam != null)
        {
            Vector2 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
            var hit = Physics2D.Raycast(mouseWorld, Vector2.zero, 0f);
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                TryStartDialogue();
            }
        }
        else
        {
            // 没有主摄像机会退到原逻辑
            TryStartDialogue();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        isPlayerInRange = true;
        playerColliderInRange = other;
        ShowHighlight(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == playerColliderInRange)
        {
            isPlayerInRange = false;
            playerColliderInRange = null;
            ShowHighlight(false);
        }
    }

    private void TryStartDialogue()
    {
        if (oneShot && hasTriggeredOnce) return;
        if (dialogueManager == null)
        {
            Debug.LogWarning($"[{name}] 未绑定 DialogueManager，无法开始对话。");
            return;
        }

        // 注入本物体的对话内容（若未配置，则使用 DialogueManager 自己已有的列表）
        if (objectDialogueLines != null && objectDialogueLines.Count > 0)
        {
            dialogueManager.dialogueLines = objectDialogueLines;
        }

        // 触发对话
        dialogueManager.StartDialogue();
        hasTriggeredOnce = true;

        // 交互后把高光淡出/隐藏
        ShowHighlight(false);
    }

    #region 高光显隐与透明度
    private void ShowHighlight(bool show)
    {
        if (highlightObject == null) return;

        if (useFadeForHighlight)
        {
            targetHighlightAlpha = show ? 1f : 0f;
        }
        else
        {
            SetHighlightAlphaImmediate(show ? 1f : 0f);
        }
    }

    private float GetHighlightAlpha()
    {
        if (highlightSR != null) return highlightSR.color.a;
        if (highlightImg != null) return highlightImg.color.a;
        return 0f;
    }

    private void SetHighlightAlphaImmediate(float a)
    {
        a = Mathf.Clamp01(a);

        if (highlightSR != null)
        {
            var c = highlightSR.color; c.a = a; highlightSR.color = c;
        }
        if (highlightImg != null)
        {
            var c = highlightImg.color; c.a = a; highlightImg.color = c;
            // 这里保持 raycastTarget = false，避免 UI 层挡住点击
            highlightImg.raycastTarget = false;
        }
    }
    #endregion

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        var box = GetComponent<BoxCollider2D>();
        if (box != null)
        {
            Gizmos.color = new Color(1f, 0.85f, 0.2f, 0.35f);
            var m = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.matrix = m;
            Gizmos.DrawCube(box.offset, box.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }
#endif
}
