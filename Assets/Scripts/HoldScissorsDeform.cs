using Leap.Unity;
using LeapInternal;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class HoldScissorsDeform : MedicalTool
{
    //public static GameObject currentSelectedScissor;

    public event EventHandler OnScissorsJoin;
    public event EventHandler OnScissorsSeparate;

    [Header("Left Scissor")]
    [SerializeField] private Transform LeftScissor;

    [Range(0f, 1f)]
    public float leftKey;

    [Header("\nRight Scissor")]
    [SerializeField] private Transform RightScissor;

    [Range(0f, 1f)]
    public float rightKey;

    private Quaternion originalRotationLeft;
    private Quaternion originalRotationRight;

    private Quaternion goalRotationLeft;
    private Quaternion goalRotationRight;

    [SerializeField] private float rotationAmount = -17.8f;

    private bool scissorsJoined = true;

    [SerializeField] private bool activated = true;

    [SerializeField] private float holdRotationAmount = 2.5f;

    private bool isHandLeft = false;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        originalRotationLeft = Quaternion.Euler(LeftScissor.localRotation.eulerAngles + new Vector3(0, 0, holdRotationAmount));
        goalRotationLeft = Quaternion.Euler(LeftScissor.localRotation.eulerAngles + new Vector3 (0, 0, rotationAmount));

        originalRotationRight = Quaternion.Euler(RightScissor.localRotation.eulerAngles + new Vector3(0, 0, holdRotationAmount));
        goalRotationRight = Quaternion.Euler(RightScissor.localRotation.eulerAngles + new Vector3(0, 0, rotationAmount));
    }

    private void Update()
    {
        rb.isKinematic = true;

        Leap.Hand currentHand = ClosestHandLeap();


        if (currentHand == null)
        {
            if (selectedThisTool)
            {
                if (isHandLeft)
                {
                    GameManager.grabbingToolLeftHand = false;
                    leftHand.SetMaterialToNormal();
                }

                else
                {
                    GameManager.grabbingToolRightHand = false;
                    rightHand.SetMaterialToNormal();
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
                if (isHandLeft)
                {
                    GameManager.grabbingToolLeftHand = false;
                    leftHand.SetMaterialToNormal();
                }

                else
                {
                    GameManager.grabbingToolRightHand = false;
                    rightHand.SetMaterialToNormal();
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

        //Formula to obtain a value between 0 and 1 from the distance between the middle finger and the thumb
        float value = currentHand.GetFingerPinchDistance(2) * 10 - 0.8f;


        Debug.Log(value);

        leftKey = rightKey = Mathf.Clamp(value, 0f, 1f);

        //START TEST CODE
        /*
        if (Input.GetKey(KeyCode.L))
        {
            float currentValue = leftKey;
            float nextValue = leftKey + Time.deltaTime * scissorsSpeed;

            currentValue = Mathf.Clamp(nextValue, 0f, 1f);

            leftKey = currentValue;
            rightKey = currentValue;
        }

        else if (Input.GetKey(KeyCode.K))
        {
            float currentValue = leftKey;
            float nextValue = leftKey - Time.deltaTime * scissorsSpeed;

            currentValue = Mathf.Clamp(nextValue, 0f, 1f);

            leftKey = currentValue;
            rightKey = currentValue;
        }
        */
        //END TEST CODE

        //Moves the tool on its axis of rotation, if it passes the threshold then the tool's system activates
        LeftScissor.localRotation = Quaternion.Slerp(originalRotationLeft, goalRotationLeft, leftKey);
        RightScissor.localRotation = Quaternion.Slerp(originalRotationRight, goalRotationRight, rightKey);

        if (leftKey > 0.55f && rightKey > 0.55f && scissorsJoined)
        {
            scissorsJoined = false;
            OnScissorsSeparate?.Invoke(this, EventArgs.Empty);
        }

        if (leftKey <= 0.3f && rightKey <= 0.3f && !scissorsJoined)
        {
            scissorsJoined = true;
            OnScissorsJoin?.Invoke(this, EventArgs.Empty);
        }
    }

    
}
