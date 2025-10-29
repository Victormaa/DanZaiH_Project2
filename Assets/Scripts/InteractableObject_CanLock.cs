using System.Collections;
using UnityEngine;

/// <summary>
/// 交互柜（拖钥匙自动解锁与发奖，Reward 渐显并定格）
/// - 钥匙(KeyToken.keyId)拖入触发区：解锁→音效→Reward 渐显→入背包
/// - 动画播放完后物品定格在最后一帧保持可见
/// - 若无 Animator，用 CanvasGroup 渐显并常驻
/// - 兼容 OnKeyDeliveredExternally() 外部调用
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class InteractableObject_CanLock : IInteractable2D
{
    [Header("Lock Settings")]
    public bool isLocked = true;
    public string requiredKeyId = "CabinetKey";
    private bool isOpenedOnce = false;
    private bool isBusy = false;
    private bool wasLockedAtStart = true;

    [Header("UI / Visual")]
    public GameObject highlightImage;
    public GameObject fKeyPrompt;
    public float highlightFlashDuration = 0.2f;
    private Coroutine highlightCo;

    [Header("Audio")]
    public AudioClip lockedSound;
    public AudioClip noKeySoundA;
    public AudioClip unlockSoundB;
    private AudioSource audioSource;

    [Header("Reward / Animation")]
    public GameObject rewardPrefab;
    public Transform rewardSpawnPoint;
    public GameObject getItemAnimObject;
    public string getItemAnimTrigger = "Play";
    public float fallbackAnimDuration = 1.2f;

    [Tooltip("当没有 Animator 时，使用 CanvasGroup 的淡入时长")]
    public float rewardFadeDuration = 0.6f;
    [Tooltip("淡入完成后的停留时间（备用）")]
    public float rewardDisplayHold = 0.5f;

    [Header("Inventory")]
    public InventoryLike playerInventory;
    public string rewardItemId = "SomeLoot";

    // 缓存
    private Animator getItemAnimator;
    private CanvasGroup rewardCG;
    private bool isPlayerNearby = false;

    private void Awake()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;

        wasLockedAtStart = isLocked;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        if (highlightImage) highlightImage.SetActive(false);
        if (fKeyPrompt) fKeyPrompt.SetActive(false);

        if (getItemAnimObject)
        {
            getItemAnimator = getItemAnimObject.GetComponent<Animator>();
            rewardCG = getItemAnimObject.GetComponent<CanvasGroup>();
            if (rewardCG == null) rewardCG = getItemAnimObject.AddComponent<CanvasGroup>();

            getItemAnimObject.SetActive(false);
            rewardCG.alpha = 0f;
        }
    }

    private void Update() { }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (fKeyPrompt) fKeyPrompt.SetActive(false);
            return;
        }

        var key = other.GetComponent<KeyToken>();
        if (key != null && isLocked && !isBusy)
        {
            if (!string.IsNullOrEmpty(requiredKeyId) && key.keyId == requiredKeyId)
                StartCoroutine(UnlockAndAutoRewardSequence());
            else
            {
                PlayOneShot(lockedSound);
                PlayOneShot(noKeySoundA);
                Debug.Log("[Cabinet] Wrong key token.");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (highlightImage) highlightImage.SetActive(false);
        }
    }

    public override void Interact()
    {
        if (!isPlayerNearby || isBusy) return;

        if (isLocked)
        {
            PlayOneShot(lockedSound);
            PlayOneShot(noKeySoundA);
        }
    }

    public void OnKeyDeliveredExternally()
    {
        if (isBusy) return;
        if (isLocked)
            StartCoroutine(UnlockAndAutoRewardSequence());
    }

    private IEnumerator UnlockAndAutoRewardSequence()
    {
        isBusy = true;
        bool shouldAutoReward = wasLockedAtStart && isLocked && !isOpenedOnce;
        isLocked = false;

        PlayOneShot(unlockSoundB);
        Debug.Log("[Cabinet] Unlocked by key. Auto reward flow if needed.");

        if (shouldAutoReward)
            yield return StartCoroutine(PlayRewardAppearAndGive());

        isBusy = false;
    }

    /// <summary>
    /// 奖励渐显并永久定格在最后一帧
    /// </summary>
    private IEnumerator PlayRewardAppearAndGive()
    {
        isOpenedOnce = true;

        if (rewardPrefab && rewardSpawnPoint)
            Instantiate(rewardPrefab, rewardSpawnPoint.position, Quaternion.identity);

        float waitTime = fallbackAnimDuration;

        // ---- Animator 路径：动画后定格 ----
        if (getItemAnimator && getItemAnimObject)
        {
            getItemAnimObject.SetActive(true);
            getItemAnimator.speed = 1f;
            getItemAnimator.ResetTrigger(getItemAnimTrigger);
            getItemAnimator.SetTrigger(getItemAnimTrigger);

            yield return null;
            var st = getItemAnimator.GetCurrentAnimatorStateInfo(0);
            if (st.length > 0.05f) waitTime = st.length;

            yield return new WaitForSeconds(waitTime);

            // ⭐ 定格在最后一帧
            getItemAnimator.speed = 0f;

            // 彻底静止物理运动
            var rb = getItemAnimObject.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        // ---- CanvasGroup 路径：淡入常驻 ----
        else if (getItemAnimObject && rewardCG)
        {
            getItemAnimObject.SetActive(true);
            rewardCG.alpha = 0f;

            float t = 0f;
            while (t < rewardFadeDuration)
            {
                t += Time.deltaTime;
                rewardCG.alpha = Mathf.Clamp01(t / rewardFadeDuration);
                yield return null;
            }

            // 常驻：保持 alpha = 1
            rewardCG.alpha = 1f;

            var rb = getItemAnimObject.GetComponent<Rigidbody2D>();
            if (rb)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Kinematic;
                rb.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
        else
        {
            yield return new WaitForSeconds(waitTime);
        }

        // 入背包
        if (playerInventory != null && !string.IsNullOrEmpty(rewardItemId))
        {
            playerInventory.AddItem(rewardItemId, 1);
            Debug.Log($"[Cabinet] Reward added to inventory: {rewardItemId}");
        }
        else
        {
            Debug.LogWarning("[Cabinet] Inventory or rewardItemId not set.");
        }

        if (highlightImage) highlightImage.SetActive(false);
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
        if (highlightCo != null) StopCoroutine(highlightCo);
        highlightCo = StartCoroutine(CoFlashHighlight());
    }

    private IEnumerator CoFlashHighlight()
    {
        highlightImage.SetActive(true);
        yield return new WaitForSeconds(highlightFlashDuration);
        highlightImage.SetActive(false);
        highlightCo = null;
    }

    public void UnlockExternallyAndAutoReward()
    {
        if (!isLocked || isBusy) return;
        StartCoroutine(UnlockAndAutoRewardSequence());
    }
}

/// <summary> 钥匙标记脚本 </summary>
public class KeyToken : MonoBehaviour
{
    public string keyId = "CabinetKey";
}

/// <summary> 简易背包示例 </summary>
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
            AddItem(testKeyId, 1);
    }
}

