using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutScissorsDeform : MedicalTool
{
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

    private bool isHandLeft = false;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        originalRotationLeft = LeftScissor.localRotation;
        goalRotationLeft = Quaternion.Euler(LeftScissor.localRotation.eulerAngles + new Vector3(0, 0, rotationAmount));

        originalRotationRight = RightScissor.localRotation;
        goalRotationRight = Quaternion.Euler(RightScissor.localRotation.eulerAngles + new Vector3(0, 0, rotationAmount));
    }

    private void Update()
    {
        rb.isKinematic = true;

        Leap.Hand currentHand = ClosestHand();

        if (currentHand == null)
        {
            if (selectedThisTool)
            {
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

        if (IsCurrentHandOccupied(currentHand))
        {
            if (selectedThisTool)
            {
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

        //Formula to obtain a value between 0 and 1 from the distance between the middle finger and the thumb
        float value = currentHand.GetFingerPinchDistance(2) * 10 - 0.8f;

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

        if (leftKey <= 0.25f && rightKey <= 0.25f && !scissorsJoined)
        {
            scissorsJoined = true;
            OnScissorsJoin?.Invoke(this, EventArgs.Empty);
        }
    }
}
