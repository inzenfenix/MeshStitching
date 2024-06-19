using Leap.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutScissorsDeform : MedicalTool
{
    public event EventHandler OnScissorsJoin;
    public event EventHandler OnScissorsSeparate;

    [SerializeField] private float rotationAmount = -17.8f;

    private bool scissorsJoined = true;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        originalRotation1 = firstComponent.localRotation;
        goalRotation1 = Quaternion.Euler(firstComponent.localRotation.eulerAngles + new Vector3(0, 0, rotationAmount));

        originalRotation2 = secondComponent.localRotation;
        goalRotation2 = Quaternion.Euler(secondComponent.localRotation.eulerAngles + new Vector3(0, 0, rotationAmount));
    }

    protected override void Update()
    {
        base.Update();

        Leap.Hand currentHand = ClosestHandLeap();
        if (currentHand == null)
        {
            return;
        }

        //Formula to obtain a value between 0 and 1 from the distance between the middle finger and the thumb
        float value = currentHand.GetFingerPinchDistance(2) * 10 - 0.8f;

        key1 = key2 = Mathf.Clamp(value, 0f, 1f);

        //Moves the tool on its axis of rotation, if it passes the threshold then the tool's system activates
        firstComponent.localRotation = Quaternion.Slerp(originalRotation1, goalRotation1, key1);
        secondComponent.localRotation = Quaternion.Slerp(originalRotation2, goalRotation2, key2);

        if (key1 > 0.55f && key2 > 0.55f && scissorsJoined)
        {
            scissorsJoined = false;
            OnScissorsSeparate?.Invoke(this, EventArgs.Empty);
        }

        if (key1 <= 0.25f && key2 <= 0.25f && !scissorsJoined)
        {
            scissorsJoined = true;
            OnScissorsJoin?.Invoke(this, EventArgs.Empty);
        }
    }
}
