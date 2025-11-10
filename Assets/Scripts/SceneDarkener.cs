using UnityEngine;

public class SceneDarkener : MonoBehaviour
{
    public LayerMask darkMask;  // 黑场时渲染的层（勾 AlwaysVisible + UI）
    public LayerMask litMask = ~0; // 亮场时渲染的层（Everything）
    public bool startDark = true;
    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>() ?? Camera.main;
    }

    void Start()
    {
        if (startDark) GoDark();
    }

    public void GoDark()
    {
        if (cam) cam.cullingMask = darkMask;
    }

    public void LetThereBeLight()
    {
        if (cam) cam.cullingMask = litMask;
    }
}
