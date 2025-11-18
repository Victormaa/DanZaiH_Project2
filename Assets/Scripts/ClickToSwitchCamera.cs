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
    public AudioSource clickSfx;

    [Header("玩家检测")]
    public string playerTag = "Player";

    [Header("BallGame 游戏对象")]
    public GameObject ballGameObject;          // ← 新增：BallGame 对象引用

    bool _playerInRange = false;

    public PlayerController pc;

    void Start()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // 初始化高光
        if (highlightObject != null && hideHighlightOnStart)
            highlightObject.SetActive(false);

        // 开局：MainCam 显示，GameCamera 隐藏
        if (mainCam != null)
            mainCam.Priority = activePriority;
        if (gameCam != null)
            gameCam.Priority = inactivePriority;

        // BallGame 游戏开始时隐藏
        if (ballGameObject != null)
            ballGameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;
            if (highlightObject != null)
                highlightObject.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;
            if (highlightObject != null)
                highlightObject.SetActive(false);
        }
    }

    void OnMouseDown()
    {
        if (!_playerInRange) return;
        if (mainCam == null || gameCam == null) return;

        if (clickSfx != null)
            clickSfx.Play();

        // 切换到 GameCamera
        gameCam.Priority = activePriority;
        mainCam.Priority = inactivePriority;

        // BallGame 显示
        if (ballGameObject != null)
        {
            pc.FreezeControl();

            ballGameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (mainCam == null || gameCam == null) return;

            // 切回主相机
            gameCam.Priority = inactivePriority;
            mainCam.Priority = activePriority;

            // 隐藏 BallGame
            if (ballGameObject != null)
            {
                pc.UnfreezeControl();
                ballGameObject.SetActive(false);
            }
        }
    }
}
