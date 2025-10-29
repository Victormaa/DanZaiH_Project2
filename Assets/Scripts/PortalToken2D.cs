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
    private void OnMouseUpAsButton()
    {
        if (!useOnMouseClick) return;
        if (parentPortal == null) return;
        parentPortal.RequestTravel(destination);
    }

    // 方式二（可选）：如果你的 Token 是 UI（Graphic+Raycast + EventSystem），
    // 可把上面 OnMouseUpAsButton 关掉，然后在 UI Button 的 OnClick() 里调用这个公共方法：
    public void OnUIButtonClicked()
    {
        if (parentPortal == null) return;
        parentPortal.RequestTravel(destination);
    }
}

