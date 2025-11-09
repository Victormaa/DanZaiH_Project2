using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ItemATarget2D : MonoBehaviour
{
    [Header("需要被投递的物品ID")]
    public string requiredItemId = "ItemB";

    [Header("整合：外部引用")]
    public LightFader lightFader;     // 拖入 LightFader
    public GameObject itemA;          // A 自身
    public GameObject itemCToShow;    // B 投递后显示的 C
    public Animator itemDAnimator;    // D 的 Animator（可为空）
    public string dAnimatorTrigger = "Play";

    [Header("无B时点击A触发的对话")]
    public DialogueManager dialogueManager;

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true; // 建议做触发体
    }

    private void OnMouseDown()
    {
        // 点击 A：若场景里没有激活的 B，则触发对话
        if (!HasActiveItem(requiredItemId) && dialogueManager != null)
        {
            dialogueManager.StartDialogue();
        }
    }

    /// <summary>
    /// 被 B 投递时由 B 调用
    /// </summary>
    public bool AcceptItem(string incomingItemId, GameObject incomingItemGO)
    {
        if (incomingItemId != requiredItemId) return false;

        // 1) 点亮灯
        if (lightFader != null) lightFader.EnableAll();

        // 2) A、B 隐藏；C 显示
        if (itemA != null) itemA.SetActive(false);
        if (incomingItemGO != null) incomingItemGO.SetActive(false);
        if (itemCToShow != null) itemCToShow.SetActive(true);

        // 3) 播动画 D
        if (itemDAnimator != null && !string.IsNullOrEmpty(dAnimatorTrigger))
        {
            itemDAnimator.SetTrigger(dAnimatorTrigger);
        }
        return true;
    }

    private bool HasActiveItem(string id)
    {
        var all = GameObject.FindObjectsOfType<ItemTag>(true);
        foreach (var t in all)
        {
            if (t.itemId == id && t.gameObject.activeInHierarchy)
                return true;
        }
        return false;
    }
}
