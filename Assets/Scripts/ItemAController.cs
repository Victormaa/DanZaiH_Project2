using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))] // 让 OnMouseDown 能接到点击
public class ItemAController : MonoBehaviour
{
    [Header("高光图片（SpriteRenderer，建议放在A的子物体）")]
    public SpriteRenderer highlightSR;     // 这里拖你那张“高光图”的SpriteRenderer
    public bool hideHighlightOnAwake = true;

    [Header("对话管理器")]
    public DialogueManager dialogueManager;

    [Header("点击音效（可选）")]
    public AudioSource clickSfx;

    [Header("排序（可选）：让高光盖在物品上")]
    public SpriteRenderer baseSR;          // 物品A本体的SpriteRenderer（可不填）
    public int highlightOrderOffset = 10;  // 相对baseSR排序的提升量

    void Awake()
    {
        // 自动找：没拖的话，默认在子物体里找一个名含“Highlight”的SpriteRenderer
        if (highlightSR == null)
        {
            foreach (var sr in GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (sr != null && sr.gameObject != this.gameObject &&
                    sr.name.ToLower().Contains("highlight"))
                {
                    highlightSR = sr; break;
                }
            }
        }

        // 可选：自动记住本体SpriteRenderer
        if (baseSR == null) baseSR = GetComponent<SpriteRenderer>();

        // 初始隐藏高光
        if (hideHighlightOnAwake && highlightSR != null)
            highlightSR.enabled = false;

        // 确保高光在上层
        if (highlightSR != null && baseSR != null)
            highlightSR.sortingOrder = baseSR.sortingOrder + highlightOrderOffset;
    }

    void OnMouseDown()
    {
        if (clickSfx) clickSfx.Play();

        // 显示高光图片
        if (highlightSR != null)
        {
            // 若你需要半透明，可在Inspector调它的Color alpha
            highlightSR.enabled = true;

            // 保险：把高光排序顶到本体之上
            if (baseSR != null)
                highlightSR.sortingOrder = baseSR.sortingOrder + highlightOrderOffset;
        }

        // 调用对话
        if (dialogueManager) dialogueManager.StartDialogue();
    }

    /// <summary>供其他脚本（比如DropZone）也能开关高光</summary>
    public void SetHighlightVisible(bool visible)
    {
        if (highlightSR != null) highlightSR.enabled = visible;
    }
}
