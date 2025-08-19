using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable_Door : IInteractable2D
{
    public string sceneName;
    

   
    public override void Interact()
    {
        // ÇÐ»»³¡¾°
        //StartCoroutine(SceneTransition());
        GameManager.Instance.ChangeScene(sceneName);
    }

    

}
