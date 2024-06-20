using Leap;
using Leap.Unity;
using Leap.Unity.Interaction;
using Leap.Unity.PhysicalHands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
public class MedicalTool : MonoBehaviour
{

    //Array of 2 objects with a tool for each hand, 0 is left 1 is right
    public static GameObject[] selectedTools = new GameObject[2];

    protected InteractionBehaviour interactor;

    protected Rigidbody rb;

    protected bool selectedThisTool;

    [SerializeField] protected CapsuleHand leftHand;
    [SerializeField] protected CapsuleHand rightHand;

    [Header("First Part")]
    [SerializeField] protected Transform firstComponent;

    [Range(0f, 1f)]
    public float key1;

    [Header("\nSecond Part")]
    [SerializeField] protected Transform secondComponent;

    [Range(0f, 1f)]
    public float key2;

    protected Quaternion originalRotation1;
    protected Quaternion goalRotation1;

    protected Quaternion originalRotation2;
    protected Quaternion goalRotation2;

    protected bool isHandLeft = false;

    protected virtual void Awake()
    {
        interactor = GetComponent<InteractionBehaviour>();

        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        rb.isKinematic = true;

        if (GameManager.instance.isLeapMotion)
        {
            UsingLeapMotion();
        }

        else if(GameManager.instance.isNovaGlove)
        {
            UsingNovaGloves();
        }
    }

    public void UsingLeapMotion()
    {
        Leap.Hand currentHand = ClosestHandLeap();

        if (currentHand == null)
        {
            if (selectedThisTool)
            {
                if (leftHand == null || rightHand == null)
                    return;

                if (isHandLeft)
                {
                    leftHand.SetMaterialToNormal();
                    GameManager.grabbingToolLeftHand = false;
                }

                else
                {
                    rightHand.SetMaterialToNormal();
                    GameManager.grabbingToolRightHand = false;
                }

                selectedThisTool = false;

                DeselectTool();

            }

            return;
        }

        if (IsCurrentHandOccupied(currentHand.IsLeft))
        {
            if (selectedThisTool)
            {
                if (leftHand == null || rightHand == null)
                    return;

                if (isHandLeft)
                {
                    leftHand.SetMaterialToNormal();
                    GameManager.grabbingToolLeftHand = false;
                }

                else
                {
                    rightHand.SetMaterialToNormal();
                    GameManager.grabbingToolRightHand = false;
                }

                selectedThisTool = false;
                DeselectTool();

            }
            return;
        }

        if (currentHand.GetFingerStrength(4) <= .15d)
        {
            if (selectedThisTool)
            {
                if (currentHand.IsLeft) leftHand.SetMaterialToNormal();
                else rightHand.SetMaterialToNormal();
                selectedThisTool = false;
                DeselectTool();

                if (isHandLeft)
                {
                    GameManager.grabbingToolLeftHand = false;
                }

                else
                {
                    GameManager.grabbingToolRightHand = false;
                }

            }

            return;
        }

        if (!selectedThisTool)
        {
            if (currentHand.IsLeft)
            {
                if (GameManager.instance.grabbingWithLeft)
                {
                    return;
                }

                GameManager.grabbingToolLeftHand = true;
                isHandLeft = true;
                leftHand.SetTransparentHands();
            }

            else
            {
                if (GameManager.instance.grabbingWithRight)
                {
                    return;
                }

                GameManager.grabbingToolRightHand = true;
                isHandLeft = false;
                rightHand.SetTransparentHands();
            }

            SelectTool();
            selectedThisTool = true;
        }

        transform.position = currentHand.PalmPosition;
        transform.rotation = currentHand.Rotation;
    }

    public void UsingNovaGloves()
    {
        Transform currentHand = GameManager.NovaPalmNearby(this.transform, out bool isLeft);

        if (currentHand == null)
        {
            if (selectedThisTool)
            {
                if (leftHand == null || rightHand == null)
                    return;

                if (isHandLeft)
                {
                    //leftHand.SetMaterialToNormal();
                    GameManager.grabbingToolLeftHand = false;
                }

                else
                {
                    //rightHand.SetMaterialToNormal();
                    GameManager.grabbingToolRightHand = false;
                }

                selectedThisTool = false;

                DeselectTool();

            }

            return;
        }

        if (IsCurrentHandOccupied(isLeft))
        {
            if (selectedThisTool)
            {
                if (leftHand == null || rightHand == null)
                    return;

                if (isHandLeft)
                {
                    //leftHand.SetMaterialToNormal();
                    GameManager.grabbingToolLeftHand = false;
                }

                else
                {
                    //rightHand.SetMaterialToNormal();
                    GameManager.grabbingToolRightHand = false;
                }

                selectedThisTool = false;
                DeselectTool();

            }
            return;
        }

        Debug.Log(this.name + " " + GameManager.GetNovaFingerStrength(3, isLeft));
        if (GameManager.GetNovaFingerStrength(3, isLeft) <= .08d)
        {
            if (selectedThisTool)
            {
                //if (currentHand.IsLeft) leftHand.SetMaterialToNormal();
                //else rightHand.SetMaterialToNormal();
                selectedThisTool = false;
                DeselectTool();

                if (isHandLeft)
                {
                    GameManager.grabbingToolLeftHand = false;
                }

                else
                {
                    GameManager.grabbingToolRightHand = false;
                }

            }

            return;
        }

        if (!selectedThisTool)
        {
            if (isLeft)
            {
                if (GameManager.instance.grabbingWithLeft)
                {
                    return;
                }

                GameManager.grabbingToolLeftHand = true;
                isHandLeft = true;
                leftHand.SetTransparentHands();
            }

            else
            {
                if (GameManager.instance.grabbingWithRight)
                {
                    return;
                }

                GameManager.grabbingToolRightHand = true;
                isHandLeft = false;
                //rightHand.SetTransparentHands();
            }

            SelectTool();
            selectedThisTool = true;
        }

        transform.position = currentHand.position;
        transform.rotation = currentHand.rotation;
    }

    public void SelectTool()
    {
        //Debug.Log("Selected: " + this.gameObject.name);
        if (GameManager.instance.isLeapMotion)
        {

            if (GameManager.LeftHand == null &&
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

            if (GameManager.LeapPalmDistance(this.transform, GameManager.LeftHand) <
               GameManager.LeapPalmDistance(this.transform, GameManager.RightHand))
            {
                selectedTools[0] = gameObject;
            }

            else
            {
                selectedTools[1] = gameObject;
            }
        }

        //Debug.Log("Selected tool: " + this.gameObject.name);
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

    //Checks if there's a leap hand close enough to our tool, if it is it will return it
    protected Leap.Hand ClosestHandLeap()
    {
        float minDistance = 0.075f;
        Leap.Hand selectedHand = null;

        if (GameManager.LeftHand == null &&
           GameManager.RightHand != null)
        {
            if (GameManager.LeapPalmDistance(this.transform, GameManager.RightHand) < minDistance)
            {
                selectedHand = GameManager.RightHand;
            }
        }

        else if (GameManager.LeftHand != null &&
                 GameManager.RightHand == null)
        {
            if (GameManager.LeapPalmDistance(this.transform, GameManager.LeftHand) < minDistance)
            {
                selectedHand = GameManager.LeftHand;
            }
        }

        else if (GameManager.LeftHand == null ||
                 GameManager.RightHand == null)
        {
            return null;
        }

        else if (GameManager.LeapPalmDistance(this.transform, GameManager.LeftHand) <
                 GameManager.LeapPalmDistance(this.transform, GameManager.RightHand))
        {
            if (GameManager.LeapPalmDistance(this.transform, GameManager.LeftHand) < minDistance)
            {
                selectedHand = GameManager.LeftHand;
            }
        }

        else
        {
            if (GameManager.LeapPalmDistance(this.transform, GameManager.RightHand) < minDistance)
            {
                selectedHand = GameManager.RightHand;
            }
        }

        return selectedHand;
    }

    protected bool IsCurrentHandOccupied(bool isLeft)
    {
        if (isLeft)
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
