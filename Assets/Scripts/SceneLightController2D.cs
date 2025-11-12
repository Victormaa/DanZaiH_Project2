using UnityEngine;
using UnityEngine.Rendering.Universal; // Light2D

[DisallowMultipleComponent]
public class SceneLightController2D : MonoBehaviour
{
    [Header("全局2D灯光（Global Light 2D）")]
    public Light2D globalLight;

    [Header("进场时是否直接熄灯")]
    public bool turnDarkOnStart = true;

    [Header("进场时的强度（熄灯=0）")]
    [Range(0f, 1f)] public float startIntensity = 0f;

    public float CurrentIntensity => globalLight ? globalLight.intensity : 0f;

    void Awake()
    {
        if (globalLight == null)
            globalLight = FindObjectOfType<Light2D>(); // 场景里只有一个Global Light可直接找
    }

    void Start()
    {
        if (turnDarkOnStart && globalLight)
            globalLight.intensity = startIntensity; // 进场直接熄灯
    }

    /// <summary> 立即设置强度（不做插值，按需你也可扩展渐变） </summary>
    public void SetIntensityImmediate(float value)
    {
        if (globalLight) globalLight.intensity = Mathf.Clamp01(value);
    }
}

