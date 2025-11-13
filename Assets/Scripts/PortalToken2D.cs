using UnityEngine;

[DisallowMultipleComponent]
public class PortalToken2D : MonoBehaviour
{
    [Header("Binding")]
    public StairsPortal2D parentPortal;
    public StairsPortal2D.Destination destination = StairsPortal2D.Destination.Up;

    [Header("Click Settings")]
    [Tooltip("是否允许使用 OnMouseUpAsButton（适用于2D物体点击）")]
    public bool useOnMouseClick = true;

    // 方式一：适用于带 2D Collider 的世界物体
    //private void OnMouseUpAsButton()
    //void OnMouseUp()
    //{
    //    if (!useOnMouseClick) return;
    //    if (parentPortal == null) return;
    //    parentPortal.RequestTravel(destination);
    //}
    private void Start()
    {
        CheckLayerIssues();
    }
    void CheckLayerIssues()
    {
        // 检查对象图层
        Debug.Log($"当前图层: {gameObject.layer} ({LayerMask.LayerToName(gameObject.layer)})");

        // 检查是否有遮挡物
        RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
        if (hit.collider != null)
        {
            Debug.Log($"点击检测到: {hit.collider.gameObject.name}", hit.collider.gameObject);
        }
    }

    bool enableDebug = true;
    void OnMouseUp()
    {
        if (!useOnMouseClick) return;
        if (parentPortal == null)
        {
            Debug.LogWarning("PortalToken2D: parentPortal 未设置！", this);
            return;
        }

        if (enableDebug) Debug.Log($"PortalToken2D: 点击检测到 - {gameObject.name}");
        parentPortal.RequestTravel(destination);
    }

    // 可选：添加鼠标悬停反馈
    void OnMouseEnter()
    {
        if (enableDebug) Debug.Log($"PortalToken2D: 鼠标进入 - {gameObject.name}");
    }

    void OnMouseExit()
    {
        if (enableDebug) Debug.Log($"PortalToken2D: 鼠标离开 - {gameObject.name}");
    }

    // 方式二（可选）：如果你的 Token 是 UI（Graphic+Raycast + EventSystem），
    // 可把上面 OnMouseUpAsButton 关掉，然后在 UI Button 的 OnClick() 里调用这个公共方法：
    public void OnUIButtonClicked()
    {
        if (parentPortal == null) return;
        parentPortal.RequestTravel(destination);
    }
}

