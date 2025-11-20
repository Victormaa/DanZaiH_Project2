using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家靠近物体A显示高光，点击后【延迟一段时间】依次播放物品B-J的动画和音效
/// </summary>
public class ItemA_Interact : MonoBehaviour
{
    [Header("玩家 Tag")]
    public string playerTag = "Player";

    [Header("高光图片（默认关闭）")]
    public GameObject highlightObject;

    [Header("动画触发器控制器（放 B~J 的 Animator）")]
    public Animator[] itemAnimators;   // 按顺序放置 9 个 Animator (B-J)

    [Header("每个动画对应的音效")]
    public AudioClip[] itemSounds;     // 按顺序放置 9 个 AudioClip

    [Header("点击后到动画播放的延迟时间（秒）")]
    public float delayBeforePlay = 1f; // 你可以在 Inspector 里调，比如 0.5 / 1 / 2

    private AudioSource audioSource;

    private bool playerInRange = false;
    private int currentIndex = 0;      // 当前点击次数（0~8）

    private bool canClick = true;      // 防止在延迟期间疯狂点击

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 开局隐藏高光
        if (highlightObject != null)
            highlightObject.SetActive(false);
    }

    void Update()
    {
        // 玩家在范围内 & 当前允许点击时才检测点击
        if (playerInRange && canClick && Input.GetMouseButtonDown(0))
        {
            // 将鼠标点击的物体转换成世界坐标检测
            Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(clickPos, Vector2.zero);

            // 只有点击到当前这个物体A才会触发
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                // 这里不直接播放动画，而是开启一个带延迟的协程
                StartCoroutine(PlayNextAnimationWithDelay());
            }
        }
    }

    IEnumerator PlayNextAnimationWithDelay()
    {
        // 先锁定点击，避免在等待期间多次点击
        canClick = false;

        if (currentIndex >= itemAnimators.Length)
        {
            Debug.Log("所有动画都已经播放完了！");
            canClick = true; // 虽然没动画了，但可以继续点击，只是不会有反应
            yield break;
        }

        // 等待一段时间再播放动画和音效
        yield return new WaitForSeconds(delayBeforePlay);

        // 播放动画
        if (itemAnimators[currentIndex] != null)
        {
            itemAnimators[currentIndex].gameObject.SetActive(true);
            itemAnimators[currentIndex].SetTrigger("Play");
        }

        // 播放对应音效
        if (audioSource != null && itemSounds.Length > currentIndex && itemSounds[currentIndex] != null)
        {
            audioSource.PlayOneShot(itemSounds[currentIndex]);
        }

        currentIndex++;

        // 动画触发完，恢复可点击
        canClick = true;
    }

    // 玩家进入触发范围 → 显示高光
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(playerTag))
        {
            playerInRange = true;

            if (highlightObject != null)
                highlightObject.SetActive(true);
        }
    }

    // 玩家离开 → 隐藏高光
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag(playerTag))
        {
            playerInRange = false;

            if (highlightObject != null)
                highlightObject.SetActive(false);
        }
    }
}
