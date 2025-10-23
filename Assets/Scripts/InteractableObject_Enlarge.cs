using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractableObject_Enlarge : IInteractable2D
{
    public GameObject thePanel;
    public Animator panelAnimator;
    public Image theImage;
    public TMP_Text thrDescription;

    private bool isShown = false; // 当前是否处于 Show 状态

    public Sprite curSprite;

    private void Awake()
    {
        thePanel = GameObject.Find("Canvas").transform.Find("ObjectPanel").gameObject;
        panelAnimator = thePanel.GetComponent<Animator>();
        curSprite = GetComponent<SpriteRenderer>().sprite;

        // the image the description
        theImage = thePanel.transform.Find("ObjectContainer").GetComponent<Image>();
        thrDescription = thePanel.transform.Find("Text (TMP)").GetComponent<TMP_Text>();
    }

    public override void Interact()
    {
        AnimatorStateInfo stateInfo = panelAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime < 1f)
        {
            // 如果动画还没播放完，直接返回，不响应
            return;
        }

        theImage.sprite = curSprite;
        if(thrDescription != null)
            thrDescription.text = description;

        if (isShown)
        {
            panelAnimator.Play("Hide");
            isShown = false;
        }
        else
        {
            panelAnimator.Play("Show");
            isShown = true;
        }
    }
}