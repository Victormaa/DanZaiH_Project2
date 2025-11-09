using System.Collections.Generic;
using UnityEngine;
#if USING_URP || true
using UnityEngine.Rendering.Universal; // Light2D
#endif

/// <summary>
/// 选择性隐藏（带豁免）：
/// - 只对“我指定的灯/物体（或按层收集的对象）”生效；
/// - A/B/玩家放进 Exempt 列表或豁免层后，绝不会被隐藏；
/// - 黑场：选中灯 intensity=0（可选禁用组件），选中物体 Renderer.enabled=false 或整物体 SetActive(false)；
/// - 还原：恢复进入场景时的原始状态。
/// </summary>
[DisallowMultipleComponent]
public class SelectiveLightHiderEx : MonoBehaviour
{
    [Header("启动时是否立即黑场")]
    public bool startDark = true;

    [Header("只影响这些灯（手动填）")]
    public List<Light> lights3D = new List<Light>();
#if USING_URP || true
    public List<Light2D> lights2D = new List<Light2D>();
#endif

    [Header("只影响这些物体的 Renderer（手动填）")]
    public List<Renderer> renderersToHide = new List<Renderer>();

    [Header("可选：直接整物体 SetActive 的对象（手动填）")]
    public List<GameObject> gameObjectsToToggle = new List<GameObject>();

    [Header("按 Layer 自动收集（会加入到上面列表）")]
    public bool autoCollectByLayer = false;
    public LayerMask lightsLayer = 0;      // 需要控制的灯所在层
    public LayerMask renderersLayer = 0;   // 需要控制的物体所在层

    [Header("豁免（不受影响）：把A、B、玩家拖这里或放到豁免层")]
    public List<GameObject> exemptObjects = new List<GameObject>();
    public LayerMask exemptLayers = 0;     // 放到这个层的东西永远不隐藏（建议建个 AlwaysVisible 层）

    [Header("黑场行为")]
    public bool alsoDisableLightComponent = false; // 黑场时把 Light/Light2D.enabled=false
    public bool disableWholeGameObject = false;    // 对 renderersToHide，整物体隐藏而非只关Renderer

    // ——缓存对象原始状态——
    private struct L3 { public Light l; public float intensity; public bool enabled; }
#if USING_URP || true
    private struct L2 { public Light2D l; public float intensity; public bool enabled; }
#endif
    private struct RendRec
    {
        public Renderer r; public bool enabled;
        public GameObject go; public bool goActive;
        public bool hasRenderer; public bool hasGoToggle;
    }

    private readonly List<L3> _cache3 = new List<L3>();
#if USING_URP || true
    private readonly List<L2> _cache2 = new List<L2>();
#endif
    private readonly List<RendRec> _cacheR = new List<RendRec>();
    private readonly List<(GameObject go, bool active)> _cacheGo = new List<(GameObject, bool)>();

    void Awake()
    {
        // 按层自动收集（并过滤掉豁免）
        if (autoCollectByLayer)
        {
            foreach (var l in GameObject.FindObjectsOfType<Light>(true))
                TryAddLight3D(l);

#if USING_URP || true
            foreach (var l2 in GameObject.FindObjectsOfType<Light2D>(true))
                TryAddLight2D(l2);
#endif
            foreach (var r in GameObject.FindObjectsOfType<Renderer>(true))
                TryAddRenderer(r);
        }

        // 手动列表也做豁免过滤 & 建缓存
        for (int i = lights3D.Count - 1; i >= 0; i--)
        {
            var l = lights3D[i];
            if (l == null || IsExempt(l.gameObject)) { lights3D.RemoveAt(i); continue; }
            _cache3.Add(new L3 { l = l, intensity = l.intensity, enabled = l.enabled });
        }
#if USING_URP || true
        for (int i = lights2D.Count - 1; i >= 0; i--)
        {
            var l2 = lights2D[i];
            if (l2 == null || IsExempt(l2.gameObject)) { lights2D.RemoveAt(i); continue; }
            _cache2.Add(new L2 { l = l2, intensity = l2.intensity, enabled = l2.enabled });
        }
#endif
        for (int i = renderersToHide.Count - 1; i >= 0; i--)
        {
            var r = renderersToHide[i];
            if (r == null || IsExempt(r.gameObject)) { renderersToHide.RemoveAt(i); continue; }
            _cacheR.Add(new RendRec
            {
                r = r,
                enabled = r.enabled,
                go = r.gameObject,
                goActive = r.gameObject.activeSelf,
                hasRenderer = true,
                hasGoToggle = disableWholeGameObject
            });
        }
        // 直接整物体切换的名单也过滤豁免
        for (int i = gameObjectsToToggle.Count - 1; i >= 0; i--)
        {
            var go = gameObjectsToToggle[i];
            if (go == null || IsExempt(go)) { gameObjectsToToggle.RemoveAt(i); continue; }
            _cacheGo.Add((go, go.activeSelf));
        }
    }

    void Start()
    {
        if (startDark) GoDark();
    }

    // ——公共接口——
    public void GoDark()
    {
        // 3D/2D 灯
        for (int i = 0; i < _cache3.Count; i++)
        {
            var c = _cache3[i];
            if (c.l == null) continue;
            c.l.intensity = 0f;
            if (alsoDisableLightComponent) c.l.enabled = false;
            _cache3[i] = c;
        }
#if USING_URP || true
        for (int i = 0; i < _cache2.Count; i++)
        {
            var c = _cache2[i];
            if (c.l == null) continue;
            c.l.intensity = 0f;
            if (alsoDisableLightComponent) c.l.enabled = false;
            _cache2[i] = c;
        }
#endif
        // 渲染器/整物体
        for (int i = 0; i < _cacheR.Count; i++)
        {
            var c = _cacheR[i];
            if (c.hasGoToggle && c.go != null) c.go.SetActive(false);
            else if (c.hasRenderer && c.r != null) c.r.enabled = false;
            _cacheR[i] = c;
        }
        for (int i = 0; i < _cacheGo.Count; i++)
        {
            var pair = _cacheGo[i];
            if (pair.go != null) pair.go.SetActive(false);
            _cacheGo[i] = pair;
        }
    }

    public void EnableAll()
    {
        for (int i = 0; i < _cache3.Count; i++)
        {
            var c = _cache3[i];
            if (c.l == null) continue;
            if (alsoDisableLightComponent) c.l.enabled = c.enabled;
            c.l.intensity = c.intensity;
            _cache3[i] = c;
        }
#if USING_URP || true
        for (int i = 0; i < _cache2.Count; i++)
        {
            var c = _cache2[i];
            if (c.l == null) continue;
            if (alsoDisableLightComponent) c.l.enabled = c.enabled;
            c.l.intensity = c.intensity;
            _cache2[i] = c;
        }
#endif
        for (int i = 0; i < _cacheR.Count; i++)
        {
            var c = _cacheR[i];
            if (c.hasGoToggle && c.go != null) c.go.SetActive(c.goActive);
            else if (c.hasRenderer && c.r != null) c.r.enabled = c.enabled;
            _cacheR[i] = c;
        }
        for (int i = 0; i < _cacheGo.Count; i++)
        {
            var pair = _cacheGo[i];
            if (pair.go != null) pair.go.SetActive(pair.active);
            _cacheGo[i] = pair;
        }
    }

    // ——工具：按层自动收集 & 豁免判断——
    private bool IsInMask(int layer, LayerMask mask) => ((1 << layer) & mask) != 0;

    private bool IsExempt(GameObject go)
    {
        if (go == null) return true;
        if (IsInMask(go.layer, exemptLayers)) return true;
        foreach (var ex in exemptObjects) if (ex == go) return true;
        return false;
    }

    private void TryAddLight3D(Light l)
    {
        if (l == null) return;
        if (!IsInMask(l.gameObject.layer, lightsLayer)) return;
        if (IsExempt(l.gameObject)) return;
        if (!lights3D.Contains(l)) lights3D.Add(l);
    }
#if USING_URP || true
    private void TryAddLight2D(Light2D l2)
    {
        if (l2 == null) return;
        if (!IsInMask(l2.gameObject.layer, lightsLayer)) return;
        if (IsExempt(l2.gameObject)) return;
        if (!lights2D.Contains(l2)) lights2D.Add(l2);
    }
#endif
    private void TryAddRenderer(Renderer r)
    {
        if (r == null) return;
        if (!IsInMask(r.gameObject.layer, renderersLayer)) return;
        if (IsExempt(r.gameObject)) return;
        if (!renderersToHide.Contains(r)) renderersToHide.Add(r);
    }
}
