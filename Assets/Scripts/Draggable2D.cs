using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Draggable2D : MonoBehaviour
{
    [Header("相机")]
    public Camera cam;                     // 不设则自动用 Camera.main

    [Header("拖拽设置")]
    public bool constrainToScreen = true;  // 限制在屏幕内
    public float zDepth = 0f;              // 拖拽时的Z平面（2D一般用0）
    public bool raiseSortingWhileDrag = true; // 拖拽时是否抬高排序
    public int dragSortingOrder = 999;

    [Header("可选：吸附到目标")]
    public Transform snapTarget;           // 可选：比如钥匙孔的空物体锚点
    public float snapDistance = 0.3f;      // 距离小于该值则吸附

    Vector3 _grabOffsetWorld;
    bool _isDragging = false;
    int _originalSortingOrder;
    SpriteRenderer _sr;

    void Awake()
    {
        if (cam == null) cam = Camera.main;
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _originalSortingOrder = _sr.sortingOrder;
    }

    void OnMouseDown()
    {
        if (cam == null) return;
        _isDragging = true;

        // 记录鼠标点与物体中心的偏移，保证抓住哪里移动哪里
        Vector3 mouseWorld = ScreenToWorldOnPlane(Input.mousePosition, zDepth);
        _grabOffsetWorld = transform.position - mouseWorld;

        // 抬高排序，避免被其他物体遮挡
        if (raiseSortingWhileDrag && _sr != null)
            _sr.sortingOrder = dragSortingOrder;
    }

    void OnMouseDrag()
    {
        if (!_isDragging || cam == null) return;

        Vector3 targetPos = ScreenToWorldOnPlane(Input.mousePosition, zDepth) + _grabOffsetWorld;

        if (constrainToScreen)
        {
            // 把世界坐标转屏幕，再Clamp，再转回世界
            Vector3 sp = cam.WorldToScreenPoint(targetPos);
            sp.x = Mathf.Clamp(sp.x, 0, Screen.width);
            sp.y = Mathf.Clamp(sp.y, 0, Screen.height);
            targetPos = ScreenToWorldOnPlane(sp, zDepth) + _grabOffsetWorld;
        }

        transform.position = new Vector3(targetPos.x, targetPos.y, zDepth);
    }

    void OnMouseUp()
    {
        if (!_isDragging) return;
        _isDragging = false;

        // 还原排序
        if (raiseSortingWhileDrag && _sr != null)
            _sr.sortingOrder = _originalSortingOrder;

        // 吸附
        if (snapTarget != null)
        {
            float d = Vector2.Distance(transform.position, snapTarget.position);
            if (d <= snapDistance)
                transform.position = new Vector3(snapTarget.position.x, snapTarget.position.y, zDepth);
        }
    }

    Vector3 ScreenToWorldOnPlane(Vector3 screenPos, float z)
    {
        Vector3 wp = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(cam.transform.position.z - z)));
        // 强制在指定Z平面
        return new Vector3(wp.x, wp.y, z);
    }
}
