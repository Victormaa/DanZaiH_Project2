using UnityEngine;

/// <summary>
/// 挂在 Door 上的总控脚本：
/// 1. 玩家进入触发区显示高光
/// 2. 点击门：
///    - 若上锁 → 播放锁住音效 + 调用 DialogueManager.StartDialogue()
///    - 若已解锁 → 调用 Interactable_Door.Interact() 切换场景
/// 3. 拖动钥匙进入触发区时：钥匙消失 + 播放解锁音效 + 门变为打开状态
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class DoorWithKey2D : MonoBehaviour
{
    [Header("基础设置")]
    public bool startLocked = true;       // 初始是否上锁
    public bool useKeyToUnlock = true;    // 是否需要钥匙解锁（不需要钥匙的门可以关掉）
    public string playerTag = "Player";   // 玩家 Tag
    public string keyTag = "Key";         // 钥匙 Tag

    [Header("高光")]
    public GameObject highlightObject;    // Door 高光图片（子物体，默认关掉）

    [Header("音频")]
    public AudioSource audioSource;       // 挂在 Door 上或其他地方的 AudioSource
    public AudioClip lockedClip;          // 音频 A：门锁着时点击的音效
    public AudioClip unlockedClip;        // 音频 B：钥匙解锁/门打开音效

    [Header("对话")]
    public DialogueManager dialogueManager;   // 场景里的 DialogueManager

    [Header("关卡切换")]
    public Interactable_Door doorSceneLoader; // 你现有的切关脚本（可挂在同一个 Door 上）

    // 内部状态
    bool _isLocked;
    bool _playerInRange;

    void Awake()
    {
        _isLocked = startLocked;

        // 确保高光一开始是关闭的
        if (highlightObject != null)
            highlightObject.SetActive(false);

        // Collider2D 记得勾选 IsTrigger
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    // 玩家或钥匙进入门的触发区域
    void OnTriggerEnter2D(Collider2D other)
    {
        // 玩家进入 → 显示高光
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;
            if (highlightObject != null)
                highlightObject.SetActive(true);
        }

        // 钥匙进入 → 解锁
        if (useKeyToUnlock && _isLocked && other.CompareTag(keyTag))
        {
            UnlockDoorWithKey(other.gameObject);
        }
    }

    // 玩家离开门的触发区域
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;
            if (highlightObject != null)
                highlightObject.SetActive(false);
        }
    }

    // 鼠标点击 Door（和你的 Draggable2D 一样用 OnMouseDown，便于测试）
    void OnMouseDown()
    {
        // 如果你有“必须靠近才能互动”的需求，可以限制只有玩家在范围内才响应点击
        if (!_playerInRange) return;

        if (_isLocked)
        {
            // 1）播放锁住音效
            if (audioSource != null && lockedClip != null)
                audioSource.PlayOneShot(lockedClip);

            // 2）弹出对话，让 DialogueManager 播放你在 Inspector 里配好的对话
            if (dialogueManager != null)
            {
                dialogueManager.StartDialogue();
            }
        }
        else
        {
            // 门已经是打开状态 → 切换到下一个关卡
            if (doorSceneLoader != null)
            {
                doorSceneLoader.Interact();   // 调用你现有的切关逻辑
            }
        }
    }

    // 用钥匙解锁门
    void UnlockDoorWithKey(GameObject keyGO)
    {
        _isLocked = false;

        // 播放解锁音效
        if (audioSource != null && unlockedClip != null)
            audioSource.PlayOneShot(unlockedClip);

        // 钥匙消失（你也可以改成播放完动画再 Destroy）
        keyGO.SetActive(false);
        // 或者：Destroy(keyGO);

        // 门已经解锁，可以一直亮着高光，也可以自己决定是否关掉
        if (highlightObject != null)
            highlightObject.SetActive(true);
    }
}
