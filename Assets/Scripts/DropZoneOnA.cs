using UnityEngine;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class DropZoneOnA : MonoBehaviour
{
    [Header("识别物品B的Tag（给物品B打上这个Tag）")]
    public string itemBTag = "ItemB";

    [Header("对象引用")]
    public ItemAController itemA;         // 物品A控制器（可与本物体同节点）
    public GameObject itemB;              // 物品B对象
    public GameObject itemCToShow;        // 成功后立即出现的物品C
    public SceneLightController2D lightCtrl;

    [Header("成功后灯光强度")]
    [Range(0f, 1f)] public float targetIntensity = 0.2f;

    [Header("掉落成功音效（可选）")]
    public AudioSource dropSfx;

    [Header("物品D动画（非循环），仅在灯光==targetIntensity时触发")]
    public Animator itemDAnimator;
    public string itemDTrigger = "Play";  // Animator 里的Trigger名

    bool _bInside = false;    // 当前是否有B处于触发器
    Collider2D _bCol = null;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    void Awake()
    {
        if (itemA == null) itemA = GetComponent<ItemAController>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(itemBTag))
        {
            _bInside = true;
            _bCol = other;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other == _bCol)
        {
            _bInside = false;
            _bCol = null;
        }
    }

    void Update()
    {
        // 松开鼠标的这一帧且B在区域内，判定成功
        if (_bInside && Input.GetMouseButtonUp(0))
        {
            ResolveDropSuccess();
        }
    }

    void ResolveDropSuccess()
    {
        // 高光与音效（高光改为显示一张高光图片）
        if (itemA)
        {
            // 使用你新版 ItemAController 提供的高光图片开关
            itemA.SetHighlightVisible(true);
        }
        if (dropSfx) dropSfx.Play();

        // A/B 消失，C 出现
        if (itemA) itemA.gameObject.SetActive(false);
        if (itemB) itemB.SetActive(false);
        if (itemCToShow) itemCToShow.SetActive(true);

        // 灯光直接设为 targetIntensity
        if (lightCtrl) lightCtrl.SetIntensityImmediate(targetIntensity);

        // 强度到 targetIntensity 时播放 D 动画（一次性）
        if (itemDAnimator && lightCtrl && Mathf.Approximately(lightCtrl.CurrentIntensity, targetIntensity))
        {
            itemDAnimator.ResetTrigger(itemDTrigger);
            itemDAnimator.SetTrigger(itemDTrigger);
        }

        // 本触发器一次性需求：可选自毁，避免重复触发
        // Destroy(this); 
        // 或者整个 A 区域已隐藏，无需销毁
    }
}
