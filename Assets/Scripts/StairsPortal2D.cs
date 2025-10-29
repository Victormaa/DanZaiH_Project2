using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class StairsPortal2D : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("进入触发区的玩家 Tag")]
    public string playerTag = "Player";

    [Header("Visuals")]
    [Tooltip("门框高光（可为空）")]
    public GameObject highlightObject;
    [Tooltip("上楼 Token 物体（带 2D Collider 便于点击）")]
    public GameObject tokenUpObject;
    [Tooltip("下楼 Token 物体（带 2D Collider 便于点击）")]
    public GameObject tokenDownObject;

    [Header("Destinations (Scene Names)")]
    [Tooltip("上楼要加载的场景名（关卡A）")]
    public string sceneNameUp = "Level_Up";
    [Tooltip("下楼要加载的场景名（关卡B）")]
    public string sceneNameDown = "Level_Down";

    [Header("Options")]
    [Tooltip("进入触发区时是否自动激活高光+Token")]
    public bool autoShowOnEnter = true;

    private bool playerInRange = false;

    private void Reset()
    {
        var col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    private void Start()
    {
        SetPortalUIActive(false);
        // 给 Token 绑定回调（如果挂了 PortalToken2D 就会自动绑定）
        TryBindToken(tokenUpObject, Destination.Up);
        TryBindToken(tokenDownObject, Destination.Down);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = true;
        if (autoShowOnEnter) SetPortalUIActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        playerInRange = false;
        SetPortalUIActive(false);
    }

    /// <summary>
    /// 被 Token 点击时调用。
    /// </summary>
    public void RequestTravel(Destination dest)
    {
        if (!playerInRange) return; // 保险：必须在范围内
        string target = (dest == Destination.Up) ? sceneNameUp : sceneNameDown;
        if (!string.IsNullOrEmpty(target))
        {
            SceneManager.LoadScene(target);
        }
        else
        {
            Debug.LogWarning($"[StairsPortal2D] 目标场景名为空：{dest}");
        }
    }

    /// <summary>
    /// 显隐 高光+两个 Token
    /// </summary>
    private void SetPortalUIActive(bool active)
    {
        if (highlightObject) highlightObject.SetActive(active);
        if (tokenUpObject) tokenUpObject.SetActive(active);
        if (tokenDownObject) tokenDownObject.SetActive(active);
    }

    private void TryBindToken(GameObject tokenObj, Destination dest)
    {
        if (!tokenObj) return;

        // 若 Token 上已有 PortalToken2D，就设置 parentPortal&dest
        var token = tokenObj.GetComponent<PortalToken2D>();
        if (token == null)
        {
            token = tokenObj.AddComponent<PortalToken2D>();
        }
        token.parentPortal = this;
        token.destination = dest;

        // 确保可点击：没有 Collider2D 就加一个（默认 Circle）
        var col = tokenObj.GetComponent<Collider2D>();
        if (col == null)
        {
            col = tokenObj.AddComponent<CircleCollider2D>();
            (col as CircleCollider2D).isTrigger = false; // OnMouse 系列需要非触发碰撞体也可
        }
    }

    // 供外部/Inspector 使用的简单枚举
    public enum Destination { Up, Down }
}

