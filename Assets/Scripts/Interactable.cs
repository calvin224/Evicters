using UnityEngine;

public class Interactable : MonoBehaviour
{
    public string interactionText = "Press E to interact";

    public virtual void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
    }
}