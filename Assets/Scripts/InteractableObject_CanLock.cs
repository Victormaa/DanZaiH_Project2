using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject_CanLock : IInteractable2D
{
    // 1. locked Interact() => make a noise
    // 2. unlocked Interact() => Get reward

    [Header("Lock Settings")]
    public bool isLocked = true;

    [Header("Audio Settings")]
    public AudioClip lockedSound;     // 上锁时播放的音效
    public AudioClip unlockSound;     // 解锁时（获得奖励）播放的音效
    private AudioSource audioSource;

    [Header("Reward Settings")]
    public GameObject rewardPrefab;   // 奖励物体的预制体
    public Transform rewardSpawnPoint; // 奖励生成的位置

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public override void Interact()
    {
        if (isLocked)
        {
            // 🔒 被锁状态：播放上锁音效
            if (lockedSound != null)
            {
                audioSource.PlayOneShot(lockedSound);
            }
            Debug.Log("The object is locked. It made a noise.");
        }
        else
        {
            // 🔓 已解锁状态：生成奖励、播放奖励音效
            if (rewardPrefab != null && rewardSpawnPoint != null)
            {
                Instantiate(rewardPrefab, rewardSpawnPoint.position, Quaternion.identity);
            }
            else
            {
                Debug.LogWarning("Reward prefab or spawn point not set.");
            }

            if (unlockSound != null)
            {
                audioSource.PlayOneShot(unlockSound);
            }

            Debug.Log("The object was unlocked and you got a reward!");
        }
    }

    // 你可以通过其他事件来解锁，比如钥匙或任务触发
    public void Unlock()
    {
        isLocked = false;
        Debug.Log("The object has been unlocked!");
    }
}

