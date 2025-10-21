using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization; // 仅为了在 Inspector 里能看到 Localized* 字段类型，如果你只把对白配置在 DialogueManager 上，可不需要

[RequireComponent(typeof(Collider2D))]
public class InteractableObject_Letter : MonoBehaviour
{
    [Header("玩家与高亮")]
    public string playerTag = "Player";
    public GameObject highlightObject;           // 进入触发区显示高光（Sprite 或 UI Image）
    public bool useFadeForHighlight = false;
    public float highlightFadeSpeed = 8f;

    [Header("面板（点击物体后弹出）")]
    public GameObject panelRoot;                 // 面板根节点
    public Image panelImage;                     // 面板图片（信件插画等）
    public TMP_Text panelText;                   // 面板文字（显示在面板范围内）
    [TextArea(2, 6)] public string panelContent;
    public Sprite panelSprite;
    public AudioSource uiAudioSource;            // UI 音源
    public AudioClip sfxOpenPanel;               // 打开面板音效
    public AudioClip sfxClosePanel;              // 关闭面板音效

    [Header("对话——使用你自己的 DialogueManager")]
    public DialogueManager dialogueManager;      // 拖你现有的 DialogueManager
    public bool overrideDialogueLines = false;   // 若勾选：用下面这份覆盖 DialogueManager 的对白
    public List<DialogueLine> dialogueLinesToUse = new(); // 这里使用“你项目里的 DialogueLine”类型（带 Localized*）

    private bool _playerInRange = false;
    private bool _panelOpen = false;
    private Collider2D _col;
    private CanvasGroup _hlCg;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        if (_col) _col.isTrigger = true;

        // 高亮初始化
        if (highlightObject != null && useFadeForHighlight)
        {
            _hlCg = highlightObject.GetComponent<CanvasGroup>();
            if (_hlCg == null) _hlCg = highlightObject.AddComponent<CanvasGroup>();
            highlightObject.SetActive(true);
            _hlCg.alpha = 0f;
        }
        else if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }

        // 面板初始化
        if (panelRoot) panelRoot.SetActive(false);
        if (panelImage && panelSprite) panelImage.sprite = panelSprite;
        if (panelText && !string.IsNullOrEmpty(panelContent)) panelText.text = panelContent;
    }

    private void Update()
    {
        // 可选：高亮渐隐
        if (highlightObject != null && useFadeForHighlight && _hlCg != null)
        {
            float target = _playerInRange ? 1f : 0f;
            _hlCg.alpha = Mathf.MoveTowards(_hlCg.alpha, target, highlightFadeSpeed * Time.deltaTime);
        }

        // 进入范围内，点物体 → 打开面板
        if (_playerInRange && !_panelOpen && Input.GetMouseButtonDown(0))
        {
            if (IsMouseClickHittingMe()) OpenPanel();
        }
        // 面板打开时，任意点击 → 关面板并启动对话流程（走你的 DialogueManager）
        else if (_panelOpen && Input.GetMouseButtonDown(0))
        {
            ClosePanelAndStartDialogue();
        }
    }

    private bool IsMouseClickHittingMe()
    {
        // 屏幕坐标 → 世界坐标 → 2D 射线点击自身 Collider
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos2d = new Vector2(mouseWorld.x, mouseWorld.y);
        RaycastHit2D hit = Physics2D.Raycast(pos2d, Vector2.zero, 0.01f);
        return hit.collider != null && hit.collider == _col;
    }

    private void OpenPanel()
    {
        _panelOpen = true;

        if (panelImage && panelSprite) panelImage.sprite = panelSprite;
        if (panelText) panelText.text = panelContent;

        if (panelRoot) panelRoot.SetActive(true);
        PlayUISfx(sfxOpenPanel);
    }

    private void ClosePanelAndStartDialogue()
    {
        _panelOpen = false;
        if (panelRoot) panelRoot.SetActive(false);
        PlayUISfx(sfxClosePanel);

        // —— 进入你的对话系统 ——
        if (dialogueManager != null)
        {
            if (overrideDialogueLines && dialogueLinesToUse != null && dialogueLinesToUse.Count > 0)
            {
                // 用这份对白覆盖 DialogueManager 的对白
                dialogueManager.dialogueLines = new List<DialogueLine>(dialogueLinesToUse);
            }

            // 直接调用你已有的启动方法（无需按回车）
            dialogueManager.StartDialogue();
        }
        else
        {
            Debug.LogWarning("[InteractableObject_Letter] 未设置 DialogueManager 引用，无法开始对话。");
        }
    }

    private void PlayUISfx(AudioClip clip)
    {
        if (uiAudioSource != null && clip != null) uiAudioSource.PlayOneShot(clip);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;
            if (highlightObject != null && !useFadeForHighlight) highlightObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;
            if (highlightObject != null && !useFadeForHighlight) highlightObject.SetActive(false);
        }
    }
}
