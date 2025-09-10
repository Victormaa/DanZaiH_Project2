using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;   // TextMeshPro 建议用来显示文字
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Events;


[System.Serializable]
public class DialogueLine
{
    public LocalizedString speaker;  // 说话人
    public LocalizedString content;  // 对话内容
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


    void Start()
    {

    }

    public IEnumerator PlayDialogue()
    {

        dialoguePanel.SetActive(true);
        //if (GameManager.Instance.CurrentState == GameState.EntryAnimation)
        blackScreen.SetActive(true);

        foreach (var line in dialogueLines)
        {
            speakerNameText.text = line.speaker.GetLocalizedString();
            string content = line.content.GetLocalizedString();
            yield return StartCoroutine(TypeSentence(content));

            // 停顿或等待输入
            yield return new WaitForSeconds(1f);
        }

        dialoguePanel.SetActive(false);
        blackScreen.SetActive(false);

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
