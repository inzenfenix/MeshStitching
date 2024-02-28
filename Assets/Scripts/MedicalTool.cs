using Leap;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MedicalTool : MonoBehaviour
{
    public static GameObject[] selectedTools = new GameObject[2];

    protected InteractionBehaviour interactor;

    protected virtual void Awake()
    {
        interactor = GetComponent<InteractionBehaviour>();
    }

    public void SelectTool()
    {
        if(GameManager.LeftHand == null ||
           GameManager.RightHand == null)
        {
            return;
        }

        if(Vector3.Distance(GameManager.LeftHand.PalmPosition, this.transform.position) <
           Vector3.Distance(GameManager.RightHand.PalmPosition, this.transform.position))
        {
            selectedTools[0] = gameObject;
        }

        else
        {
            selectedTools[1] = gameObject;
        }

        Debug.Log("Selected tool: " + this.gameObject.name);
    }

    public virtual void DeselectTool(GameObject tool)
    {
        for(int i = 0; i < selectedTools.Length; i++)
        {
            if (selectedTools[i] == tool)
            {
                selectedTools[i] = null;
                return;
            }
        }

    }
}
