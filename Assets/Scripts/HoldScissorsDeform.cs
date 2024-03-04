using Leap.Unity;
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
    private float scissorsSpeed = 5.0f;

    [SerializeField] private bool activated = true;

    [SerializeField] private float holdRotationAmount = 2.5f;

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

        Leap.Hand currentHand;

        if (selectedTools[0] == this.gameObject)
        {
            currentHand = GameManager.LeftHand;
        }

        else if (selectedTools[1] == this.gameObject)
        {
            currentHand = GameManager.RightHand;
        }

        else
        {
            return;
        }

        if(currentHand == null)
        {
            return;
        }


        float minValue = currentHand.GetFingerPinchDistance(3) * 10 - 0.75f;

        if (minValue >= 0.75f)
        {
            DeselectTool(this.gameObject);
        }

        else
        {
            SelectTool();
        }

        //Formula to obtain a value between 0 and 1 from the distance between the middle finger and the thumb
        float value = -currentHand.PinchDistance / 15 + 2.5f;

        leftKey = rightKey = Mathf.Clamp(value, 0f, 1f);

        if(leftKey >= 0.7f)
        {
            leftKey = rightKey = 1f;
        }

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
        LeftScissor.localRotation = Quaternion.Slerp(goalRotationLeft, originalRotationLeft, leftKey);
        RightScissor.localRotation = Quaternion.Slerp(goalRotationRight, originalRotationRight, rightKey);

        if (leftKey > 0.7f && rightKey > 0.7f && !scissorsJoined)
        {
            scissorsJoined = true;
            OnScissorsJoin?.Invoke(this, EventArgs.Empty);
        }

        if (leftKey <= 0.3f && rightKey <= 0.3f && scissorsJoined)
        {
            scissorsJoined = false;
            OnScissorsSeparate?.Invoke(this, EventArgs.Empty);
        }
    }
}
