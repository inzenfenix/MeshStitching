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
    private float scissorsSpeed = 5.0f;

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
            return;
        }

        else
        {
            transform.position = currentHand.PalmPosition;
            transform.rotation = currentHand.Rotation;
        }

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

        if (currentHand == null)
        {
            return;
        }

        //Formula to obtain a value between 0 and 1 from the distance between the middle finger and the thumb
        float value = -currentHand.PinchDistance / 15 + 2.5f;

        leftKey = rightKey = Mathf.Clamp(value, 0f, 1f);

        //START TEST CODE
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

        //END TEST CODE

        //Moves the tool on its axis of rotation, if it passes the threshold then the tool's system activates
        LeftScissor.localRotation = Quaternion.Slerp(goalRotationLeft, originalRotationLeft, leftKey);
        RightScissor.localRotation = Quaternion.Slerp(goalRotationRight, originalRotationRight, rightKey);

        if (leftKey > 0.9f && rightKey > 0.9f && !scissorsJoined)
        {
            scissorsJoined = true;
            OnScissorsJoin?.Invoke(this, EventArgs.Empty);
        }

        if (leftKey <= 0.8f && rightKey <= 0.8f && scissorsJoined)
        {
            scissorsJoined = false;
        }
    }
}
