using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

[RequireComponent(typeof(Collider2D))]
public class InteractableObject_Letter : MonoBehaviour
{
    [Header("玩家与高亮")]
    public string playerTag = "Player";
    public GameObject highlightObject;
    public bool useFadeForHighlight = false;
    public float highlightFadeSpeed = 8f;

    [Header("面板（点击物体后弹出）")]
    public GameObject panelRoot;
    public Image panelImage;
    public TMP_Text panelText;
    [TextArea(2, 6)] public string panelContent;
    public Sprite panelSprite;
    public AudioSource uiAudioSource;
    public AudioClip sfxOpenPanel;
    public AudioClip sfxClosePanel;

    [Header("面板弹出动画设置")]
    public bool enablePanelAnimation = true;
    public float fadeSpeed = 6f;
    public float scaleSpeed = 8f;
    public Vector3 minScale = new Vector3(0.8f, 0.8f, 0.8f);

    private CanvasGroup _panelCg;
    private RectTransform _panelRt;
    private float _targetAlpha = 0f;
    private Vector3 _targetScale;
    private bool _panelVisible = false;

    [Header("对话——使用你自己的 DialogueManager")]
    public DialogueManager dialogueManager;
    public bool overrideDialogueLines = false;
    public List<DialogueLine> dialogueLinesToUse = new();

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
        if (panelRoot != null)
        {
            _panelRt = panelRoot.GetComponent<RectTransform>();
            _panelCg = panelRoot.GetComponent<CanvasGroup>();
            if (_panelCg == null) _panelCg = panelRoot.AddComponent<CanvasGroup>();
            if (_panelRt == null) _panelRt = panelRoot.AddComponent<RectTransform>();

            panelRoot.SetActive(true);
            _panelCg.alpha = 0f;
            _panelRt.localScale = minScale;
            _panelVisible = false;
        }

        if (panelImage && panelSprite) panelImage.sprite = panelSprite;
        if (panelText && !string.IsNullOrEmpty(panelContent)) panelText.text = panelContent;
    }

    private void Update()
    {
        // 高亮渐隐
        if (highlightObject != null && useFadeForHighlight && _hlCg != null)
        {
            float target = _playerInRange ? 1f : 0f;
            _hlCg.alpha = Mathf.MoveTowards(_hlCg.alpha, target, highlightFadeSpeed * Time.deltaTime);
        }

        // 打开/关闭检测
        if (_playerInRange && !_panelOpen && Input.GetMouseButtonDown(0))
        {
            if (IsMouseClickHittingMe()) OpenPanel();
        }
        else if (_panelOpen && Input.GetMouseButtonDown(0))
        {
            ClosePanelAndStartDialogue();
        }

        // 平滑动画逻辑
        if (enablePanelAnimation && _panelCg != null && _panelRt != null)
        {
            _panelCg.alpha = Mathf.MoveTowards(_panelCg.alpha, _targetAlpha, fadeSpeed * Time.deltaTime);
            _panelRt.localScale = Vector3.Lerp(_panelRt.localScale, _targetScale, Time.deltaTime * scaleSpeed);
        }
    }

    private bool IsMouseClickHittingMe()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pos2d = new Vector2(mouseWorld.x, mouseWorld.y);
        RaycastHit2D hit = Physics2D.Raycast(pos2d, Vector2.zero, 0.01f);
        return hit.collider != null && hit.collider == _col;
    }

    private void OpenPanel()
    {
        _panelOpen = true;
        _panelVisible = true;

        if (panelImage && panelSprite) panelImage.sprite = panelSprite;
        if (panelText) panelText.text = panelContent;

        if (panelRoot) panelRoot.SetActive(true);

        if (enablePanelAnimation && _panelCg != null)
        {
            _targetAlpha = 1f;
            _targetScale = Vector3.one;
        }

        PlayUISfx(sfxOpenPanel);
    }

    private void ClosePanelAndStartDialogue()
    {
        _panelOpen = false;
        _panelVisible = false;

        if (enablePanelAnimation && _panelCg != null)
        {
            _targetAlpha = 0f;
            _targetScale = minScale;
            // 延迟隐藏（避免瞬间消失）
            StartCoroutine(HidePanelAfterFade());
        }
        else if (panelRoot)
        {
            panelRoot.SetActive(false);
        }

        PlayUISfx(sfxClosePanel);

        if (dialogueManager != null)
        {
            if (overrideDialogueLines && dialogueLinesToUse.Count > 0)
                dialogueManager.dialogueLines = new List<DialogueLine>(dialogueLinesToUse);

            dialogueManager.StartDialogue();
        }
        else
        {
            Debug.LogWarning("[InteractableObject_Letter] 未设置 DialogueManager 引用，无法开始对话。");
        }
    }

    private System.Collections.IEnumerator HidePanelAfterFade()
    {
        yield return new WaitForSeconds(0.3f);
        if (!_panelVisible && panelRoot != null) panelRoot.SetActive(false);
    }

    private void PlayUISfx(AudioClip clip)
    {
        if (uiAudioSource != null && clip != null)
            uiAudioSource.PlayOneShot(clip);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = true;
            if (highlightObject != null && !useFadeForHighlight)
                highlightObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            _playerInRange = false;
            if (highlightObject != null && !useFadeForHighlight)
                highlightObject.SetActive(false);
        }
    }
}

