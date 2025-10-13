using System.Collections;
using UnityEngine;

/// <summary>
/// 交互柜：
/// - 进入触发区显示F提示；
/// - 按F即刻闪烁高光；
/// - 锁住+无钥匙：播放上锁音效（可选叠加音频A），不发物；
/// - 锁住+有钥匙：解锁→音频B→获得物品动画→动画结束入背包；
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class InteractableObject_CanLock : IInteractable2D
{
    [Header("Lock Settings")]
    public bool isLocked = true;
    public string requiredKeyId = "CabinetKey";
    private bool isOpenedOnce = false;
    private bool isBusy = false; // 防止重复触发流程

    [Header("UI / Visual")]
    public GameObject highlightImage;   // 高光图片（子物体），默认关闭
    public GameObject fKeyPrompt;       // F提示图标（子物体），默认关闭
    public float highlightFlashDuration = 0.2f; // 按F时闪一下
    private Coroutine highlightCo;

    [Header("Audio")]
    public AudioClip lockedSound;   // 锁住时的反馈音（你要求必播）
    public AudioClip noKeySoundA;   // 可选：无钥匙提示音
    public AudioClip unlockSoundB;  // 解锁成功音
    private AudioSource audioSource;

    [Header("Reward / Animation")]
    public GameObject rewardPrefab;     // 可选：场景里弹出展示物
    public Transform rewardSpawnPoint;  // 可选：展示物生成点
    public GameObject getItemAnimObject; // 获得物品动画（带Animator，默认关闭）
    public string getItemAnimTrigger = "Play";
    public float fallbackAnimDuration = 1.2f;

    [Header("Inventory")]
    public InventoryLike playerInventory;  // 你的背包脚本（示例）
    public string rewardItemId = "SomeLoot";

    private Animator getItemAnimator;
    private bool isPlayerNearby = false;

    private void Awake()
    {
        // 确保触发器
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        // AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // 高光与F提示默认关闭
        if (highlightImage) highlightImage.SetActive(false);
        if (fKeyPrompt) fKeyPrompt.SetActive(false);

        // 动画器
        if (getItemAnimObject)
        {
            getItemAnimator = getItemAnimObject.GetComponent<Animator>();
            getItemAnimObject.SetActive(false);
        }
    }

    private void Update()
    {
        // 在范围内才处理键入
        if (!isPlayerNearby || isBusy) return;

        // 进入区域即显示F提示（你现在的需求如此）
        if (fKeyPrompt && !fKeyPrompt.activeSelf)
            fKeyPrompt.SetActive(true);

        // F 键触发
        if (Input.GetKeyDown(KeyCode.F))
        {
            // 按F就让高光闪一下（视觉反馈更直接）
            FlashHighlight();

            // 走交互流程
            TryInteractByKey();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = true;

        // 进入区域，只显示F提示，不立刻高亮（高光在按F时出现）
        if (fKeyPrompt) fKeyPrompt.SetActive(true);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        isPlayerNearby = false;

        if (fKeyPrompt) fKeyPrompt.SetActive(false);
        if (highlightImage) highlightImage.SetActive(false);
    }

    public override void Interact()
    {
        if (!isPlayerNearby || isBusy) return;
        // 与统一逻辑保持一致：按F效果
        FlashHighlight();
        TryInteractByKey();
    }

    private void TryInteractByKey()
    {
        if (isOpenedOnce || isBusy) return;

        if (isLocked)
        {
            if (!HasRequiredKey())
            {
                // 锁住 + 无钥匙：播放上锁音效（你明确要求），可选叠加A
                PlayOneShot(lockedSound);
                PlayOneShot(noKeySoundA);
                Debug.Log("[Cabinet] Locked & No Key: played lockedSound (and A if set).");
                return;
            }

            // 锁住 + 有钥匙：解锁→音频B→动画→入背包
            StartCoroutine(UnlockAndRewardSequence());
        }
        else
        {
            // 已解锁但未发奖（兜底）
            if (!isOpenedOnce)
            {
                StartCoroutine(PlayGetItemAndGive());
            }
        }
    }

    private IEnumerator UnlockAndRewardSequence()
    {
        isBusy = true;

        isLocked = false; // 先标记为已解锁，避免多次触发
        PlayOneShot(unlockSoundB);
        Debug.Log("[Cabinet] Unlocked with key. Playing B and reward flow.");

        yield return StartCoroutine(PlayGetItemAndGive());

        isBusy = false;
    }

    private IEnumerator PlayGetItemAndGive()
    {
        isOpenedOnce = true;

        // 可选：场景展示一个小物
        if (rewardPrefab && rewardSpawnPoint)
            Instantiate(rewardPrefab, rewardSpawnPoint.position, Quaternion.identity);

        float waitTime = fallbackAnimDuration;
        if (getItemAnimator && getItemAnimObject)
        {
            getItemAnimObject.SetActive(true);
            getItemAnimator.ResetTrigger(getItemAnimTrigger);
            getItemAnimator.SetTrigger(getItemAnimTrigger);

            // 等一帧让 Animator 切到状态，再取时长
            yield return null;
            var st = getItemAnimator.GetCurrentAnimatorStateInfo(0);
            if (st.length > 0.05f) waitTime = st.length;

            yield return new WaitForSeconds(waitTime);
            getItemAnimObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSeconds(waitTime);
        }

        // 动画结束 → 入背包
        if (playerInventory != null && !string.IsNullOrEmpty(rewardItemId))
        {
            playerInventory.AddItem(rewardItemId, 1);
            Debug.Log($"[Cabinet] Reward added to inventory: {rewardItemId}");
        }
        else
        {
            Debug.LogWarning("[Cabinet] Inventory or rewardItemId not set.");
        }

        // 清理 UI
        if (fKeyPrompt) fKeyPrompt.SetActive(false);
        if (highlightImage) highlightImage.SetActive(false);
    }

    private bool HasRequiredKey()
    {
        if (playerInventory == null || string.IsNullOrEmpty(requiredKeyId)) return false;
        return playerInventory.HasItem(requiredKeyId);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (clip == null) return;
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.PlayOneShot(clip);
    }

    private void FlashHighlight()
    {
        if (!highlightImage) return;

        // 取消上一个闪烁
        if (highlightCo != null) StopCoroutine(highlightCo);
        highlightCo = StartCoroutine(CoFlashHighlight());
    }

    private IEnumerator CoFlashHighlight()
    {
        highlightImage.SetActive(true);
        yield return new WaitForSeconds(highlightFlashDuration);
        // 若流程正忙（比如解锁播放动画期间），保持高光开着会更直观
        // 但你需求是“按F出现高光”，所以忙时也关掉，统一手感
        highlightImage.SetActive(false);
        highlightCo = null;
    }

    // 外部直接解锁（可选）
    public void Unlock()
    {
        if (!isLocked) return;
        isLocked = false;
        Debug.Log("[Cabinet] Unlocked externally.");
    }
}

/// <summary>
/// 非正式背包示例（保持与你之前一致）
/// </summary>
public class InventoryLike : MonoBehaviour
{
    private readonly System.Collections.Generic.Dictionary<string, int> items =
        new System.Collections.Generic.Dictionary<string, int>();

    public bool HasItem(string id)
    {
        return items.TryGetValue(id, out int count) && count > 0;
    }

    public void AddItem(string id, int count)
    {
        if (string.IsNullOrEmpty(id) || count <= 0) return;
        if (!items.ContainsKey(id)) items[id] = 0;
        items[id] += count;
        Debug.Log($"[Inventory] {id} +{count}, now: {items[id]}");
    }

    [Header("Debug")]
    public bool giveTestKeyOnStart = false;
    public string testKeyId = "CabinetKey";

    private void Start()
    {
        if (giveTestKeyOnStart && !string.IsNullOrEmpty(testKeyId))
        {
            AddItem(testKeyId, 1);
        }
    }
}

