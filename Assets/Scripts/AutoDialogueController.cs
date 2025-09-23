using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoDialogueController : MonoBehaviour
{
    public DialogueManager dialogueManager;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(AutoStartDialogue());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator AutoStartDialogue()
    {
        // wait 1 s
        yield return new WaitForSeconds(1.0f);

        dialogueManager.StartDialogue();

    }
}
