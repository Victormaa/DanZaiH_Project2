using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
// 可选：强制同物体上必须有高光脚本，避免忘记挂
[RequireComponent(typeof(ProximityHighlighter2D))]
public class ItemAClickToSpawnB : MonoBehaviour
{
    [Header("物品B控制器预制体（或场景引用）")]
    public ItemBController itemBPrefab;   // 推荐用 Prefab
    public ItemBController itemBInScene;  // 或者用场景里的对象

    [Header("生成/显示位置")]
    public Transform spawnAnchor;         // 指定位置（空物体锚点）

    [Header("点击时仅在玩家靠近才生效")]
    public bool requireInRange = true;

    Camera _cam;
    Collider2D _col;
    ProximityHighlighter2D _highlighter;

    void Reset()
    {
        // 点击检测用 OverlapPoint，对是否 Trigger 不敏感，这里保持默认即可
        _col = GetComponent<Collider2D>();
    }

    void Awake()
    {
        _cam = Camera.main;
        _col = GetComponent<Collider2D>();
        _highlighter = GetComponent<ProximityHighlighter2D>(); // 直接自动抓，无需手动拖
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsMouseOverSelf()) return;

            if (requireInRange && _highlighter != null && !_highlighter.playerInRange) return;

            TriggerItemB();
        }
    }

    // 用 OverlapPoint 比 0 长度 Raycast 更稳
    bool IsMouseOverSelf()
    {
        if (_cam == null) _cam = Camera.main;
        if (_cam == null || _col == null) return false;

        Vector3 wp = _cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p2 = new Vector2(wp.x, wp.y);
        var hit = Physics2D.OverlapPoint(p2);
        return hit != null && hit == _col;
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
            Debug.LogWarning("[ItemAClickToSpawnB] 未找到物品B控制器引用（Prefab/场景都为空）。");
            return;
        }

        Vector3 pos = spawnAnchor != null ? spawnAnchor.position : transform.position;
        controller.ShowAt(pos); // 渐显/动画并停在最后一帧
    }
}
