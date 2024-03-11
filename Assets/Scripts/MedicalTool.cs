using Leap;
using Leap.Unity.Interaction;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MedicalTool : MonoBehaviour
{
    public static GameObject[] selectedTools = new GameObject[2];

    protected InteractionBehaviour interactor;

    protected Rigidbody rb;

    protected bool selectedThisTool;

    protected virtual void Awake()
    {
        interactor = GetComponent<InteractionBehaviour>();

        rb = GetComponent<Rigidbody>();
    }

    public void SelectTool()
    {
        Debug.Log("Selected: " + this.gameObject.name);

        if(GameManager.LeftHand == null &&
           GameManager.RightHand != null)
        {
            selectedTools[1] = gameObject;
            return;
        }

        else if (GameManager.LeftHand != null &&
                 GameManager.RightHand == null)
        {
            selectedTools[0] = gameObject;
            return;
        }

        else if (GameManager.LeftHand == null ||
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

    public virtual void DeselectTool()
    {
        for(int i = 0; i < selectedTools.Length; i++)
        {
            if (selectedTools[i] == this.gameObject)
            {
                selectedTools[i] = null;
                return;
            }
        }

    }

    protected Leap.Hand ClosestHand()
    {
        float minDistance = 0.1f;
        Leap.Hand selectedHand = null;

        if (GameManager.LeftHand == null &&
           GameManager.RightHand != null)
        {
            if (Vector3.Distance(GameManager.RightHand.PalmPosition, this.transform.position) < minDistance)
            {
                selectedHand = GameManager.RightHand;
            }
        }

        else if (GameManager.LeftHand != null &&
                 GameManager.RightHand == null)
        {
            if (Vector3.Distance(GameManager.LeftHand.PalmPosition, this.transform.position) < minDistance)
            {
                selectedHand = GameManager.LeftHand;
            }
        }

        else if (GameManager.LeftHand == null ||
                 GameManager.RightHand == null)
        {
            return null;
        }

        else if (Vector3.Distance(GameManager.LeftHand.PalmPosition, this.transform.position) <
                 Vector3.Distance(GameManager.RightHand.PalmPosition, this.transform.position))
        {
            if (Vector3.Distance(GameManager.LeftHand.PalmPosition, this.transform.position) < minDistance)
            {
                selectedHand = GameManager.LeftHand;
            }
        }

        else
        {
            if (Vector3.Distance(GameManager.RightHand.PalmPosition, this.transform.position) < minDistance)
            {
                selectedHand = GameManager.RightHand;
            }
        }

        return selectedHand;
    }

    protected bool IsCurrentHandOccupied(Leap.Hand hand)
    {
        if (hand.IsLeft)
        {
            if (selectedTools[0] != this.gameObject && selectedTools[0] != null)
            {
                return true;
            }
        }

        else
        {
            if (selectedTools[1] != this.gameObject && selectedTools[1] != null)
            {
                return true;
            }
        }

        return false;
    }
}
