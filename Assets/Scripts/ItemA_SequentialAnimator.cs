using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家靠近物体A显示高光，点击后依次播放物品B-J的动画和音效
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

    private AudioSource audioSource;

    private bool playerInRange = false;
    private int currentIndex = 0;      // 当前点击次数（0~8）

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        // 开局隐藏高光
        if (highlightObject != null)
            highlightObject.SetActive(false);
    }

    void Update()
    {
        // 玩家在范围内才检测点击
        if (playerInRange && Input.GetMouseButtonDown(0))
        {
            // 将鼠标点击的物体转换成世界坐标检测
            Vector2 clickPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(clickPos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                PlayNextAnimation();
            }
        }
    }

    void PlayNextAnimation()
    {
        if (currentIndex >= itemAnimators.Length)
        {
            Debug.Log("所有动画都已经播放完了！");
            return;
        }

        // 播放动画
        itemAnimators[currentIndex].SetTrigger("Play");

        // 播放对应音效
        if (itemSounds.Length > currentIndex)
        {
            audioSource.PlayOneShot(itemSounds[currentIndex]);
        }

        currentIndex++;
    }

    // 玩家进入触发范围 → 显示高光
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag(playerTag))
        {
            playerInRange = true;
            highlightObject.SetActive(true);
        }
    }

    // 玩家离开 → 隐藏高光
    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag(playerTag))
        {
            playerInRange = false;
            highlightObject.SetActive(false);
        }
    }
}
