using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseInteract : MonoBehaviour
{
    public bool isUpStair;
    public string UpStair;
    public string DownStair;
    void OnMouseUp()
    {
        Debug.Log("NONO");
        if (isUpStair)
        {
            GameManager.Instance.ChangeScene(UpStair);
        }
        else
        {
            GameManager.Instance.ChangeScene(DownStair);
        }
    }

    void OnMouseDown()
    {
        Debug.Log("YesYes");
    }
}
