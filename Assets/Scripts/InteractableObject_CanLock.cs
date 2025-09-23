using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableObject_CanLock : IInteractable2D
{
    // two state \
    // 1. locked Interact() = > make a noise
    // 2. unlocked Interact() = > Get reward

    public bool isLocked = true;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact()
    {

    }
}
