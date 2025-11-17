using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class ItemACameraSwitcher : MonoBehaviour
{
    [Header("虚拟摄像机")]
    public CinemachineVirtualCamera mainCam;   // MainCam
    public CinemachineVirtualCamera gameCam;   // GameCamera

    [Header("优先级设置")]
    public int activePriority = 20;            // 切过去时 GameCamera 的优先级
    public int inactivePriority = 0;           // 非激活时的优先级

    [Header("高光对象（图片 / 精灵等）")]
    public GameObject highlightObject;         // 物品A的高光图（建议做成物品A的子物体）
    public bool hideHighlightOnStart = true;   // 开局是否隐藏高光

    [Header("点击音效")]
    public AudioSource clickSfx;              // 挂在物品A上的 AudioSource，拖进来

    [Header("玩家检测")]
    public string playerTag = "Player";       // 玩家物体的 Tag（在 Inspector 里给玩家设为 Player）

    bool _playerInRange = false;             // 玩家是否在触发范围内

    void Start()
    {
        // 确保有 Collider2D
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;  // 用触发器检测玩家进入

        // 初始化高光显示状态
        if (highlightObject != null && hideHighlightOnStart)
        {
            highlightObject.SetActive(false);
        }

        // 开局：MainCam 显示，GameCamera 不显示
        if (mainCam != null)
            mainCam.Priority = activePriority;

        if (gameCam != null)
            gameCam.Priority = inactivePriority;
    }

    // 玩家进入物品A的 BoxCollider2D 范围
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;

            // 显示高光
            if (highlightObject != null)
                highlightObject.SetActive(true);
        }
    }

    // 玩家离开范围
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;

            // 隐藏高光
            if (highlightObject != null)
                highlightObject.SetActive(false);
        }
    }

    // 鼠标点击物品A（要求鼠标点到这个物体的 Collider2D）
    void OnMouseDown()
    {
        // 只有玩家在范围内时才允许点击生效
        if (!_playerInRange) return;
        if (mainCam == null || gameCam == null) return;

        // 播放音效
        if (clickSfx != null)
            clickSfx.Play();

        // 切换到 GameCamera
        gameCam.Priority = activePriority;
        mainCam.Priority = inactivePriority;
    }

    void Update()
    {
        // 按 Enter / 回车键，切回 MainCam
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (mainCam == null || gameCam == null) return;

            gameCam.Priority = inactivePriority;
            mainCam.Priority = activePriority;
        }
    }
}
