using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class ItemAClickToSpawnB : MonoBehaviour
{
    [Header("依赖高光脚本")]
    public ProximityHighlighter2D highlighter;

    [Header("物品B控制器预制体（或场景引用）")]
    public ItemBController itemBPrefab;   // 推荐做成 Prefab
    public ItemBController itemBInScene;  // 或者直接拖场景中的对象

    [Header("生成/显示位置")]
    public Transform spawnAnchor;         // 指定位置（场景里放一个空物体当锚点）

    [Header("点击时仅在玩家靠近才生效")]
    public bool requireInRange = true;

    Camera _cam;
    Collider2D _col;

    void Awake()
    {
        _cam = Camera.main;
        _col = GetComponent<Collider2D>();
        if (highlighter == null) highlighter = GetComponent<ProximityHighlighter2D>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!RaycastHitSelf()) return;

            if (requireInRange && highlighter != null && !highlighter.playerInRange) return;

            TriggerItemB();
        }
    }

    bool RaycastHitSelf()
    {
        if (_cam == null) return false;
        Vector3 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p2 = new Vector2(wp.x, wp.y);
        var hit = Physics2D.Raycast(p2, Vector2.zero, 0f);
        return hit.collider != null && hit.collider == _col;
    }

    void TriggerItemB()
    {
        ItemBController controller = itemBInScene;
        if (controller == null && itemBPrefab != null)
        {
            controller = Instantiate(itemBPrefab);
        }

        if (controller == null)
        {
            Debug.LogWarning("[ItemAClickToSpawnB] 未找到物品B控制器引用。");
            return;
        }

        Vector3 pos = spawnAnchor != null ? spawnAnchor.position : transform.position;
        controller.ShowAt(pos); // 开始渐显/动画，并在最后一帧冻结
    }
}
