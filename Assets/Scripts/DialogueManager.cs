using System.Collections;
using System.Collections.Generic;
using TMPro;   // TextMeshPro 建议用来显示文字
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;


[System.Serializable]
public class DialogueLine
{
    public LocalizedString speaker;  // 说话人
    public LocalizedString content;  // 对话内容
    public LocalizedSprite portrait;           // 人物立绘
    public LocalizedAudioClip voice;    // 配音
}


public class DialogueManager : MonoBehaviour
{
    [Header("UI 组件")]
    public GameObject blackScreen;       // 黑屏背景
    public GameObject dialoguePanel;     // 对话框背景
    public TMP_Text speakerNameText;     // 说话人名字
    public TMP_Text dialogueText;        // 对话内容

    [Header("打字机设置")]
    public float typingSpeed = 0.05f;    // 打字机速度

    [Header("对话内容")]
    public List<DialogueLine> dialogueLines;  // 对话台词列表

    public UnityEvent OnDialogueFinished;

    [Header("人物立绘")]
    public Image portraitImage;  // 显示立绘

    [Header("音频")]
    public AudioSource voiceAudioSource;



    // 添加对话状态标志
    private bool isDialoguePlaying = false;

    void Start()
    {

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !isDialoguePlaying)
        {
            StartCoroutine(PlayDialogue());
        }
    }

    public IEnumerator PlayDialogue()
    {
        isDialoguePlaying = true;
        dialoguePanel.SetActive(true);
        //if (GameManager.Instance.CurrentState == GameState.EntryAnimation)
        //blackScreen.SetActive(true);

        foreach (var line in dialogueLines)
        {
            // 显示名字
            speakerNameText.text = line.speaker.GetLocalizedString();

            // 显示立绘
            var spriteHandle = line.portrait.LoadAssetAsync();
            yield return spriteHandle;
            if (spriteHandle.Status == AsyncOperationStatus.Succeeded)
            {
                portraitImage.sprite = spriteHandle.Result;
                portraitImage.gameObject.SetActive(true);
            }

            // 播放配音（循环）
            if (line.voice != null)
            {
                var voiceHandle = line.voice.LoadAssetAsync();
                yield return voiceHandle;
                if (voiceHandle.Status == UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationStatus.Succeeded)
                {
                    voiceAudioSource.clip = voiceHandle.Result;
                    voiceAudioSource.loop = true;  // ✅ 循环，直到文字结束
                    voiceAudioSource.Play();
                }
            }

            // 打字机效果
            string content = line.content.GetLocalizedString();
            yield return StartCoroutine(TypeSentence(content));

            // 🔥 打字机结束 → 让配音完成当前循环后停
            if (voiceAudioSource.isPlaying)
            {
                voiceAudioSource.loop = false;  // ✅ 关掉循环，让它自然播完
                                                // 不要 Stop()，否则会戛然而止
            }

            // 等待玩家按下 Enter 进入下一句
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter));

            // 双保险：如果语音还在播，手动停掉，避免串到下一句
            if (voiceAudioSource.isPlaying)
            {
                voiceAudioSource.Stop();
            }
        }






        dialoguePanel.SetActive(false);
        blackScreen.SetActive(false);

        isDialoguePlaying = false;

        // 播放结束事件
        OnDialogueFinished?.Invoke();
    }



    IEnumerator TypeSentence(string sentence)
    {
        //isTyping = true;
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }
        //isTyping = false;
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        dialoguePanel.SetActive(false);
        blackScreen.SetActive(false);

        Debug.Log("对话结束，进入教学阶段");
        //TutorialManager.Instance.StartTutorial(); // 教学开始
    }

    public void ShowTemporaryMessage(LocalizedString name, LocalizedString content, float duration)
    {
        StartCoroutine(ShowTemporaryRoutine(name, content, duration));
    }

    private IEnumerator ShowTemporaryRoutine(LocalizedString name,LocalizedString content, float duration)
    {
        dialoguePanel.SetActive(true);
        blackScreen.SetActive(false); // 提示一般不要黑屏

        //speakerNameText.text = ""; // 提示一般不显示说话人
        speakerNameText.text = name.GetLocalizedString();
        dialogueText.text = content.GetLocalizedString();

        yield return new WaitForSeconds(duration);

        dialoguePanel.SetActive(false);
    }

}
