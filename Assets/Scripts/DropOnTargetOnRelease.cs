using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DropOnTargetOnRelease : MonoBehaviour
{
    [Header("本物品ID")]
    public string itemId = "ItemB";

    [Header("检测设置")]
    public LayerMask targetMask = ~0;   // 可设为只包含A的层
    public float overlapRadius = 0.15f; // 松手处容错半径

    private void OnMouseUp()
    {
        if (Camera.main == null) return;

        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 p2 = (Vector2)wp;

        // 1) 小圆重叠检测
        var hits = Physics2D.OverlapCircleAll(p2, overlapRadius, targetMask);
        foreach (var h in hits)
        {
            var target = h.GetComponentInParent<ItemATarget2D>();
            if (target != null && target.AcceptItem(itemId, gameObject))
                return;
        }

        // 2) 兜底：屏幕到世界的2D射线
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit2D = Physics2D.GetRayIntersection(ray, 1000f, targetMask);
        if (hit2D.collider != null)
        {
            var target = hit2D.collider.GetComponentInParent<ItemATarget2D>();
            if (target != null && target.AcceptItem(itemId, gameObject))
                return;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, overlapRadius);
    }
}
