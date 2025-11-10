using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class ProximityHighlighter2D : MonoBehaviour
{
    [Header("Player 判定")]
    public string playerTag = "Player";

    [Header("高光图片 (启/停)")]
    public GameObject highlightVisual; // 放你的高光图片（SpriteRenderer 或子物体）

    [Header("是否一开始隐藏高光")]
    public bool startHidden = true;

    [HideInInspector] public bool playerInRange = false;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        col.isTrigger = true; // 需要触发器
    }

    void Awake()
    {
        if (highlightVisual != null && startHidden) highlightVisual.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            if (highlightVisual != null) highlightVisual.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
            if (highlightVisual != null) highlightVisual.SetActive(false);
        }
    }
}

