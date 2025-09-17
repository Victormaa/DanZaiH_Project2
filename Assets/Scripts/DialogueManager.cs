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
    public GameObject blackScreen;
    public GameObject dialoguePanel;      // 可能只是边框，所以代码里会同时控制文字/立绘显隐
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
    private bool hasFinishedAllLines = false; // 全部台词是否已结束
    private bool waitingForFirstEnter = true; // 等待第一次按下 Enter 才开始

    // ===== 场景切换所需 =====
    [Header("场景切换")]
    public string nextSceneName;          // 目标场景名
    public float fadeDuration = 0.8f;     // 渐黑时长
    public CanvasGroup blackScreenCG;     // 拖入黑屏的 CanvasGroup
    private bool isTransitioning = false; // 是否正在做渐黑/切场景

    // —— 启动即强制隐藏，杜绝“出生即显示”的闪烁/竞态
    private void Awake()
    {
        ForceHideUI();
        waitingForFirstEnter = true;
        isDialoguePlaying = false;
        hasFinishedAllLines = false;
        isTransitioning = false;
    }

    void Start()
    {
        if (blackScreenCG != null)
        {
            blackScreenCG.gameObject.SetActive(true);
            blackScreenCG.alpha = 0f;
        }
        else
        {
            Debug.LogWarning("建议在 blackScreen 上加 CanvasGroup 并拖到 blackScreenCG。");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            // 第一次按下 Enter → 显示对话框并播放第一句
            if (waitingForFirstEnter)
            {
                waitingForFirstEnter = false;
                StartCoroutine(PlayDialogue());
                return;
            }

            // 对话过程中，下一句的 Enter 在协程里用 WaitUntil 处理
            if (!isDialoguePlaying && !hasFinishedAllLines)
            {
                // 容错：若被意外打断，可重新开始播放
                StartCoroutine(PlayDialogue());
            }
            else if (hasFinishedAllLines && !isTransitioning)
            {
                // 全部对话结束后再按 Enter → 渐黑切场景
                StartCoroutine(FadeAndLoadNextScene());
            }
        }
    }

    // 一键强制隐藏 UI（在 Awake 调用，确保场景启动时看不到对话与立绘）
    private void ForceHideUI()
    {
        SetDialogueVisible(false);

        if (voiceAudioSource)
        {
            voiceAudioSource.Stop();
            voiceAudioSource.loop = false;
        }
    }

    // 统一控制对话 UI 的显隐（不改变层级也能一次性隐藏所有元素）
    private void SetDialogueVisible(bool visible)
    {
        // 主面板（若挂有 CanvasGroup，也一并处理交互/透明度）
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

        // 名字 & 内容文本
        if (speakerNameText) speakerNameText.gameObject.SetActive(visible);
        if (dialogueText) dialogueText.gameObject.SetActive(visible);

        // 立绘：只有在“可见且有图”的时候才显示
        if (portraitImage)
        {
            if (visible && portraitImage.sprite != null)
                portraitImage.gameObject.SetActive(true);
            else
                portraitImage.gameObject.SetActive(false);
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

        // 打开面板显示
        SetDialogueVisible(true);

        foreach (var line in dialogueLines)
        {
            // 显示名字
            if (speakerNameText != null)
                speakerNameText.text = line.speaker != null ? line.speaker.GetLocalizedString() : string.Empty;

            // 为避免语音串台：进入新一句前先停掉旧语音
            if (voiceAudioSource) voiceAudioSource.Stop();

            // 显示立绘（加载失败则隐藏，避免上一次残留）
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

            // 播放配音（循环），等打字机结束后再改为非循环，让本轮播完自然停
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

            // 打字机
            string content = line.content != null ? line.content.GetLocalizedString() : string.Empty;
            yield return StartCoroutine(TypeSentence(content));

            // 打字机结束 → 让配音完成当前循环后停
            if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            {
                voiceAudioSource.loop = false; // 取消循环，播放完当前这遍后会自动停止
            }

            // 等待玩家 Enter 进入下一句
            yield return new WaitUntil(() =>
                Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));

            // 双保险：避免语音串到下一句
            if (voiceAudioSource != null && voiceAudioSource.isPlaying)
            {
                voiceAudioSource.Stop();
            }
        }

        // 全部台词结束
        SetDialogueVisible(false);

        isDialoguePlaying = false;
        hasFinishedAllLines = true;

        OnDialogueFinished?.Invoke();
    }

    private IEnumerator TypeSentence(string sentence)
    {
        if (dialogueText == null)
            yield break;

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

    // ===== 渐黑并切场景 =====
    private IEnumerator FadeAndLoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("nextSceneName 为空，无法切换场景。");
            yield break;
        }

        if (blackScreenCG == null)
        {
            // 无渐黑组件，直接切场景
            SceneManager.LoadScene(nextSceneName);
            yield break;
        }

        isTransitioning = true;

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
        if (blackScreen != null) blackScreen.SetActive(false); // 提示一般不要黑屏

        if (speakerNameText) speakerNameText.text = name != null ? name.GetLocalizedString() : string.Empty;
        if (dialogueText) dialogueText.text = content != null ? content.GetLocalizedString() : string.Empty;

        yield return new WaitForSeconds(duration);

        SetDialogueVisible(false);
    }
}







