using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 挂在“物品A”上的脚本：
/// 1. 玩家进入触发区 → 显示高光
/// 2. 玩家离开触发区 → 关闭高光
/// 3. 当 B、C、D 被拖进触发区 → 启动渐隐
/// 4. 三个物品都渐隐完成后 → 自动切换场景
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class AreaFadeAndScene2D : MonoBehaviour
{
    [Header("玩家检测")]
    public string playerTag = "Player";        // 玩家Tag（你的玩家物体需要设置这个Tag）

    [Header("物品A的高光物体")]
    public GameObject highlightObject;         // 比如一个高光Sprite，默认勾掉Active

    [Header("需要放到这里的三个物体")]
    public GameObject itemB;                   // 在Inspector里拖进来
    public GameObject itemC;
    public GameObject itemD;

    [Header("渐隐设置")]
    public float fadeDuration = 1.0f;          // 渐隐时间
    public bool disableColliderOnFade = true;  // 渐隐时是否关掉物体自身Collider

    [Header("场景设置")]
    public string nextSceneName;               // 目标场景名（要在 Build Settings 里加好）

    // 内部状态
    private HashSet<GameObject> _alreadyFaded = new HashSet<GameObject>(); // 已经渐隐过的物体
    private int _fadeCount = 0;

    Collider2D _col;

    void Awake()
    {
        _col = GetComponent<Collider2D>();
        // 确保是触发器
        _col.isTrigger = true;

        if (highlightObject != null)
            highlightObject.SetActive(false);
    }

    // 玩家进来：开高光
    void OnTriggerEnter2D(Collider2D other)
    {
        // 1. 玩家进入 → 开高光
        if (other.CompareTag(playerTag))
        {
            if (highlightObject != null)
                highlightObject.SetActive(true);
        }

        // 2. 检测 B/C/D 是否被拖进来
        GameObject obj = other.gameObject;
        if (obj == itemB || obj == itemC || obj == itemD)
        {
            // 防止重复渐隐
            if (_alreadyFaded.Contains(obj)) return;

            StartCoroutine(FadeOutItem(obj));
        }
    }

    // 玩家离开：关高光
    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            if (highlightObject != null)
                highlightObject.SetActive(false);
        }
    }

    IEnumerator FadeOutItem(GameObject item)
    {
        _alreadyFaded.Add(item);

        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            // 没有SpriteRenderer就直接关掉
            item.SetActive(false);
            OnOneItemFadeFinished();
            yield break;
        }

        if (disableColliderOnFade)
        {
            Collider2D itemCol = item.GetComponent<Collider2D>();
            if (itemCol != null) itemCol.enabled = false;
        }

        float timer = 0f;
        Color startColor = sr.color;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            float alpha = Mathf.Lerp(1f, 0f, t);
            sr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        // 完全隐掉后关闭物体
        item.SetActive(false);

        OnOneItemFadeFinished();
    }

    void OnOneItemFadeFinished()
    {
        _fadeCount++;

        // B,C,D 三个全部渐隐完成
        if (_fadeCount >= 3)
        {
            // 这里根据需求，你可以先黑屏，再切换场景
            // 这里只做简单的直接切场
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
            }
            else
            {
                Debug.LogWarning("未设置 nextSceneName，无法切换场景。");
            }
        }
    }
}
