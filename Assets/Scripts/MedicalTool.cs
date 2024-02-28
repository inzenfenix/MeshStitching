using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicalTool : MonoBehaviour
{
    public static GameObject selectedTool;

    protected InteractionBehaviour interactor;

    protected bool handIsLeft = false;

    protected virtual void Awake()
    {
        interactor = GetComponent<InteractionBehaviour>();
    }

    public void SelectTool()
    {
        if (interactor.closestHoveringHand == GameManager.LeftHand)
        {
            handIsLeft = true;
        }

        else
        {
            handIsLeft = false;
        }

        selectedTool = this.gameObject;

        Debug.Log("Selected tool: " + this.gameObject.name);
    }

    public virtual void DeselectTool(GameObject tool)
    {
        if (selectedTool != tool) return;

        selectedTool = null;
    }
}
