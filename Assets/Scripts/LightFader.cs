using System.Collections.Generic;
using UnityEngine;
#if USING_URP || true
using UnityEngine.Rendering.Universal;
#endif

[DisallowMultipleComponent]
public class LightFader : MonoBehaviour
{
    [Header("Start In Darkness")]
    public bool startDark = true;
    public bool overrideAmbient = true;
    public Color ambientWhenDark = Color.black;

    private struct Rec
    {
        public Light l3; public float i3;
#if USING_URP || true
        public Light2D l2; public float i2;
#endif
    }
    private List<Rec> _recs = new();
    private Color _ambientBackup;

    void Awake()
    {
        _ambientBackup = RenderSettings.ambientLight;
        foreach (var l in GameObject.FindObjectsOfType<Light>(true))
            _recs.Add(new Rec { l3 = l, i3 = l.intensity });
#if USING_URP || true
        foreach (var l2 in GameObject.FindObjectsOfType<Light2D>(true))
            _recs.Add(new Rec { l2 = l2, i2 = l2.intensity });
#endif
    }

    void Start() { if (startDark) GoDark(); }

    public void GoDark()
    {
        foreach (var r in _recs)
        {
            if (r.l3) r.l3.intensity = 0f;
#if USING_URP || true
            if (r.l2) r.l2.intensity = 0f;
#endif
        }
        if (overrideAmbient) RenderSettings.ambientLight = ambientWhenDark;
    }

    public void EnableAll()
    {
        foreach (var r in _recs)
        {
            if (r.l3) r.l3.intensity = r.i3;
#if USING_URP || true
            if (r.l2) r.l2.intensity = r.i2;
#endif
        }
        if (overrideAmbient) RenderSettings.ambientLight = _ambientBackup;
    }
}
