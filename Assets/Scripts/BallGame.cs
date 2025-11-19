using UnityEngine;

public class BallGame : MonoBehaviour
{
    public static BallGame Instance { get; private set; }

    [Header("三个容器")]
    public ContainerZone container1;   // 对应 Container_1
    public ContainerZone container2;   // 对应 Container_2
    public ContainerZone container3;   // 对应 Container_3

    [Header("物品A动画")]
    public Animator itemAAnimator;     // 物品A的 Animator
    public string itemATrigger = "Play"; // 触发动画的 Trigger 名

    [Header("音频A")]
    public AudioSource audioA;         // 播放音频A的 AudioSource

    bool hasPlayed = false;           // 防止重复播放

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// 由每个 ContainerZone 在状态变化时调用
    /// </summary>
    public void CheckAllContainers()
    {
        if (hasPlayed) return;

        if (container1 == null || container2 == null || container3 == null)
        {
            Debug.LogWarning("BallGame: 有 Container 没有在 Inspector 里绑定。");
            return;
        }

        // 三个容器都“标签正确且数量等于2”才算完成
        if (container1.IsConditionMet() &&
            container2.IsConditionMet() &&
            container3.IsConditionMet())
        {
            // 播放物品A动画
            if (itemAAnimator != null && !string.IsNullOrEmpty(itemATrigger))
            {
                itemAAnimator.SetTrigger(itemATrigger);
            }

            // 播放音频A
            if (audioA != null)
            {
                audioA.Play();
            }

            hasPlayed = true;
            Debug.Log("BallGame: 三个Container都正确，播放物品A动画和音频A。");
        }
    }
}
