using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject_Enlarge : IInteractable2D
{
    public GameObject thePanel;
    public Animator panelAnimator;

    private bool isShown = false; // 当前是否处于 Show 状态

    private void Awake()
    {
        panelAnimator = thePanel.GetComponent<Animator>();
    }

    public override void Interact()
    {
        AnimatorStateInfo stateInfo = panelAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.normalizedTime < 1f)
        {
            // 如果动画还没播放完，直接返回，不响应
            return;
        }

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