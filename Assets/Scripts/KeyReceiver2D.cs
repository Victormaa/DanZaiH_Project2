using System.Collections;
using UnityEngine;

/// <summary>
/// 挂在 B 的子物体（KeyZone）上：检测钥匙进入范围，触发钥匙渐隐→播放解锁音→通知B已解锁
/// 需要：Collider2D 勾 isTrigger；场景里至少一方带 Rigidbody2D（推荐钥匙带Kinematic）
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class KeyReceiver2D : MonoBehaviour
{
    [Header("引用")]
    public InteractableObject_CanLock cabinet;  // 拖入桌子B（带 InteractableObject_CanLock 的对象）

    [Header("逻辑与反馈")]
    public bool oneTime = true;                 // 只接收一次
    public bool playCabinetUnlockSound = true;  // 渐隐后播放B的 unlockSoundB
    public bool showFPromptAfterUnlock = true;  // 解锁后让玩家按F领取

    private bool _done = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (_done && oneTime) return;
        var key = other.GetComponent<KeyToken2D>();
        if (key == null || key.IsConsumed) return;

        // 只要进范围就触发（也可以换成 OnTriggerStay2D + 限制速度/停留时间等）
        StartCoroutine(CoAcceptKey(key));
    }

    IEnumerator CoAcceptKey(KeyToken2D key)
    {
        _done = true;

        // 1) 让钥匙渐隐并消耗
        yield return StartCoroutine(key.ConsumeAndFade());

        // 2) 播放B的“解锁音”
        if (playCabinetUnlockSound && cabinet != null && cabinet.unlockSoundB != null)
        {
            var src = cabinet.GetComponent<AudioSource>();
            if (src == null) src = cabinet.gameObject.AddComponent<AudioSource>();
            src.PlayOneShot(cabinet.unlockSoundB);
        }

        // 3) 通知B：已解锁（物理钥匙方式）
        if (cabinet != null)
        {
            cabinet.OnKeyDeliveredExternally();  // 见③
            if (showFPromptAfterUnlock && cabinet.fKeyPrompt != null)
                cabinet.fKeyPrompt.SetActive(true);
        }
    }
}
