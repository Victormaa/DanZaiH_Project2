using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[System.Serializable]
public class DialogueLine
{
    public LocalizedString speaker;
    public LocalizedString content;
    public LocalizedSprite portrait;
    public LocalizedAudioClip voice;
}

public class DialogueManager : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject blackScreen;          // 可选：黑幕的 GameObject（仅用于 ShowTemporaryMessage 里关闭）
    public GameObject dialoguePanel;        // 对话根节点（或最外层容器）
    public TMP_Text speakerNameText;
    public TMP_Text dialogueText;

    [Header("打字机设置")]
    public float typingSpeed = 0.05f;

    [Header("对话内容")]
    public List<DialogueLine> dialogueLines;
    public UnityEvent OnDialogueFinished;

    [Header("人物立绘")]
    public Image portraitImage;

    [Header("音频")]
    public AudioSource voiceAudioSource;

    // 对话状态
    private bool isDialoguePlaying = false;
    private bool hasFinishedAllLines = false;
    private bool waitingForFirstEnter = true;

    [Header("场景切换")]
    public string nextSceneName;          // 目标场景名
    public float fadeDuration = 0.8f;     // 渐黑时长
    public CanvasGroup blackScreenCG;     // 黑幕上挂的 CanvasGroup（务必拖上去）
    private bool isTransitioning = false;

    // ———————————————————————

    private void Awake()
    {
        ForceHideUI();
        waitingForFirstEnter = true;
        isDialoguePlaying = false;
        hasFinishedAllLines = false;
        isTransitioning = false;
    }

    private void Start()
    {
        PrepareBlackScreen();  // 关键：把黑幕顶到最上层并置为透明
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (waitingForFirstEnter)
                return;

            if (!isDialoguePlaying && !hasFinishedAllLines)
            {
                StartCoroutine(PlayDialogue());
            }
            //else if (hasFinishedAllLines && !isTransitioning)
            //{
                
            //}
        }
    }

    public void OnDialogueEnd()
    {
        StartCoroutine(FadeAndLoadNextScene());
    }
    // 一键强制隐藏 UI
    private void ForceHideUI()
    {
        SetDialogueVisible(false);

        if (voiceAudioSource)
        {
            voiceAudioSource.Stop();
            voiceAudioSource.loop = false;
        }
    }

    // 统一控制对话 UI 的显隐
    private void SetDialogueVisible(bool visible)
    {
        if (dialoguePanel)
        {
            dialoguePanel.SetActive(visible);
            var cg = dialoguePanel.GetComponent<CanvasGroup>();
            if (cg != null)
            {
                cg.alpha = visible ? 1f : 0f;
                cg.interactable = visible;
                cg.blocksRaycasts = visible;
            }
        }

        if (speakerNameText) speakerNameText.gameObject.SetActive(visible);
        if (dialogueText) dialogueText.gameObject.SetActive(visible);

        if (portraitImage)
        {
            if (visible && portraitImage.sprite != null)
                portraitImage.gameObject.SetActive(true);
            else
                portraitImage.gameObject.SetActive(false);
        }
    }

    // ——————— 黑幕准备：确保位于最上层且可覆盖全屏 ———————
    private void PrepareBlackScreen()
    {
        if (blackScreenCG == null) return;

        // 确保激活 & 透明起步
        blackScreenCG.gameObject.SetActive(true);
        blackScreenCG.alpha = 0f;

        // 若挂在 Image 上，保证颜色不透明（黑色全不透明）
        var img = blackScreenCG.GetComponent<Image>();
        if (img != null)
        {
            var c = img.color;
            if (c.a < 1f) { c.a = 1f; img.color = c; }   // 颜色自身要不透明，交由 CanvasGroup 控制透明度
            img.raycastTarget = true;
        }

        // 充满全屏
        var rt = blackScreenCG.transform as RectTransform;
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.sizeDelta = Vector2.zero;
            rt.anchoredPosition = Vector2.zero;
        }

        // 顶到最上层
        blackScreenCG.transform.SetAsLastSibling();

        // 如黑幕自己带 Canvas，则强制置顶排序
        var ownCanvas = blackScreenCG.GetComponent<Canvas>();
        if (ownCanvas != null)
        {
            ownCanvas.overrideSorting = true;
            ownCanvas.sortingOrder = 32767; // 极大，确保覆盖
        }
        else
        {
            // 若没有独立 Canvas，尽量把它放到最外层 Canvas 的最后
            var rootCanvas = blackScreenCG.GetComponentInParent<Canvas>();
            if (rootCanvas != null && !rootCanvas.overrideSorting)
            {
                // 常规情况：同一个 Canvas 下最后一个子物体会绘制在最上层
                blackScreenCG.transform.SetAsLastSibling();
            }
        }
    }

    public IEnumerator PlayDialogue()
    {
        if (dialogueLines == null || dialogueLines.Count == 0)
        {
            Debug.LogWarning("dialogueLines 为空，无法播放对话。");
            yield break;
        }

        isDialoguePlaying = true;
        SetDialogueVisible(true);

        foreach (var line in dialogueLines)
        {
            if (speakerNameText != null)
                speakerNameText.text = line.speaker != null ? line.speaker.GetLocalizedString() : string.Empty;

            if (voiceAudioSource) voiceAudioSource.Stop();

            bool portraitShown = false;
            if (portraitImage != null && line.portrait != null)
            {
                var spriteHandle = line.portrait.LoadAssetAsync();
                yield return spriteHandle;

                if (spriteHandle.Status == AsyncOperationStatus.Succeeded && spriteHandle.Result != null)
                {
                    portraitImage.sprite = spriteHandle.Result;
                    portraitImage.gameObject.SetActive(true);
                    portraitShown = true;
                }
            }
            if (!portraitShown && portraitImage != null)
            {
                portraitImage.sprite = null;
                portraitImage.gameObject.SetActive(false);
            }

            if (voiceAudioSource != null && line.voice != null)
            {
                var voiceHandle = line.voice.LoadAssetAsync();
                yield return voiceHandle;

                if (voiceHandle.Status == AsyncOperationStatus.Succeeded && voiceHandle.Result != null)
                {
                    voiceAudioSource.clip = voiceHandle.Result;
                    voiceAudioSource.loop = true;   // 打字期间循环
                    voiceAudioSource.Play();
                }
                else
                {
                    voiceAudioSource.Stop();
                }
            }

            string content = line.content != null ? line.content.GetLocalizedString() : string.Empty;
            yield return StartCoroutine(TypeSentence(content));

            if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            {
                voiceAudioSource.loop = false; // 取消循环，播放完当前这遍后自动停止
            }

            yield return new WaitUntil(() =>
                Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));

            if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            {
                voiceAudioSource.Stop();
            }
        }

        SetDialogueVisible(false);

        isDialoguePlaying = false;
        hasFinishedAllLines = true;

        waitingForFirstEnter = true;
        
        OnDialogueFinished?.Invoke();
    }

    private IEnumerator TypeSentence(string sentence)
    {
        if (dialogueText == null) yield break;

        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        SetDialogueVisible(false);
        if (voiceAudioSource) voiceAudioSource.Stop();
        isDialoguePlaying = false;
        hasFinishedAllLines = true;
        Debug.Log("对话结束");
    }

    // ——————— 渐黑并切场景（含兜底确保黑幕在最上层） ———————
    private IEnumerator FadeAndLoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("nextSceneName 为空，无法切换场景。");
            yield break;
        }

        if (blackScreenCG == null)
        {
            SceneManager.LoadScene(nextSceneName);   // 没有黑幕组件就直接切
            yield break;
        }

        isTransitioning = true;

        // 每次切场景前都再次确保黑幕状态正确且位于最上层
        PrepareBlackScreen();

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            blackScreenCG.alpha = Mathf.Clamp01(t / fadeDuration);
            yield return null;
        }

        SceneManager.LoadScene(nextSceneName);
    }

    // ======= 临时消息（保持你原有的公共 API）=======
    public void ShowTemporaryMessage(LocalizedString name, LocalizedString content, float duration)
    {
        StartCoroutine(ShowTemporaryRoutine(name, content, duration));
    }

    private IEnumerator ShowTemporaryRoutine(LocalizedString name, LocalizedString content, float duration)
    {
        SetDialogueVisible(true);
        if (blackScreen != null) blackScreen.SetActive(false);

        if (speakerNameText) speakerNameText.text = name != null ? name.GetLocalizedString() : string.Empty;
        if (dialogueText) dialogueText.text = content != null ? content.GetLocalizedString() : string.Empty;

        yield return new WaitForSeconds(duration);

        SetDialogueVisible(false);
    }

    public void StartDialogue()
    {
        if (waitingForFirstEnter)
        {
            waitingForFirstEnter = false;
            StartCoroutine(PlayDialogue());
            return;
        }
    }
}








