using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction2D : MonoBehaviour
{
    public IInteractable2D overlappingInteractable;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(overlappingInteractable != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                overlappingInteractable.Interact();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable2D interactable = collision.gameObject.GetComponent<IInteractable2D>();
        if (interactable != null)
        {
            interactable.tipsIcon.SetActive(true);
            overlappingInteractable = interactable;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable2D interactable = collision.GetComponent<IInteractable2D>();
        if (interactable != null && interactable == overlappingInteractable)
        {
            interactable.tipsIcon.SetActive(false);
            overlappingInteractable = null;
        }
    }
}
