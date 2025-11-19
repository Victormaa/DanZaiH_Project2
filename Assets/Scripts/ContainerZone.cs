using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class ContainerZone : MonoBehaviour
{
    [Header("本容器需要识别的Tag")]
    public string expectedTag = "GreyBall";  // Container_1: GreyBall, Container_2: WhiteBall, Container_3: BlackBall

    [Header("需要的数量")]
    public int requiredCount = 2;            // 你这里是每个容器需要 2 个球

    int totalCount = 0;      // 当前在盒子里的 Circle 总数
    int correctCount = 0;    // 当前在盒子里的、Tag 正确的 Circle 数

    void Reset()
    {
        // 自动把 BoxCollider2D 设成 Trigger
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        col.isTrigger = true;
    }

    /// <summary>
    /// 给 BallGame 调用：是否“标签正确且数量等于2”
    /// </summary>
    public bool IsConditionMet()
    {
        return totalCount == requiredCount && correctCount == requiredCount;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("BlackBall") &&
            !other.CompareTag("WhiteBall") &&
            !other.CompareTag("GreyBall"))
        {
            return; // 不是 Circle 就忽略
        }

        totalCount++;

        if (other.CompareTag(expectedTag))
        {
            correctCount++;
        }

        // 通知总控检查一次
        if (BallGame.Instance != null)
        {
            BallGame.Instance.CheckAllContainers();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("BlackBall") &&
            !other.CompareTag("WhiteBall") &&
            !other.CompareTag("GreyBall"))
        {
            return;
        }

        totalCount = Mathf.Max(0, totalCount - 1);

        if (other.CompareTag(expectedTag))
        {
            correctCount = Mathf.Max(0, correctCount - 1);
        }

        if (BallGame.Instance != null)
        {
            BallGame.Instance.CheckAllContainers();
        }
    }
}
