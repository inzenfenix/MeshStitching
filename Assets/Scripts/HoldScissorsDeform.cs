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

    [SerializeField] private float rotationAmount = -17.8f;

    private bool scissorsJoined = true;

    [SerializeField] private bool activated = true;

    [SerializeField] private float holdRotationAmount = 2.5f;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        originalRotation1 = Quaternion.Euler(firstComponent.localRotation.eulerAngles + new Vector3(0, 0, holdRotationAmount));
        goalRotation1 = Quaternion.Euler(firstComponent.localRotation.eulerAngles + new Vector3 (0, 0, rotationAmount));

        originalRotation2 = Quaternion.Euler(secondComponent.localRotation.eulerAngles + new Vector3(0, 0, holdRotationAmount));
        goalRotation2 = Quaternion.Euler(secondComponent.localRotation.eulerAngles + new Vector3(0, 0, rotationAmount));
    }

    protected override void Update()
    {
        base.Update();

        if (!selectedThisTool) return;

        if (GameManager.instance.isLeapMotion)
        {
            key1 = key2 = WithLeapMotion();
        }

        else if (GameManager.instance.isNovaGloveOrQuest)
        {
            key1 = key2 = WithNovaGloves();
        }

        else if (GameManager.instance.isQuestControllers)
        {
            key1 = key2 = WithQuestControllers();
        }

        //Moves the tool on its axis of rotation, if it passes the threshold then the tool's system activates
        firstComponent.localRotation = Quaternion.Slerp(originalRotation1, goalRotation1, key1);
        secondComponent.localRotation = Quaternion.Slerp(originalRotation2, goalRotation2, key2);

        if (key1 > 0.4f && key2 > 0.4f && scissorsJoined)
        {
            scissorsJoined = false;
            OnScissorsSeparate?.Invoke(this, EventArgs.Empty);
        }

        if (key1 <= 0.2f && key2 <= 0.2f && !scissorsJoined)
        {
            scissorsJoined = true;
            OnScissorsJoin?.Invoke(this, EventArgs.Empty);
        }
    }

    private float WithLeapMotion()
    {
        Leap.Hand currentHand = ClosestHandLeap();
        if (currentHand == null)
        {
            return key1;
        }

        if(IsCurrentHandOccupied(currentHand.IsLeft))
        {
            return key1;
        }

        //Formula to obtain a value between 0 and 1 from the distance between the middle finger and the thumb
        float value = currentHand.GetFingerPinchDistance(2) * 11 - 0.4f;

        return Mathf.Clamp(value, 0f, 1f);
    }


    private float WithNovaGloves()
    {
        Transform currentHand = GameManager.NovaQuestPalmNearby(this.transform, out bool isLeft);
        if (currentHand == null)
        {
            return key1;
        }

        if (IsCurrentHandOccupied(isLeft))
        {
            return key1;
        }


        float value = GameManager.NovaFingerDistance(0, 3, isLeft) * 22 - .8f;
        Debug.Log(value);

        return Mathf.Clamp(value, 0f, 1f);
    }

    private float WithQuestControllers()
    {
        Transform currentHand = GameManager.QuestControllerNearby(this.transform, out bool isLeft, 0, false, selectedThisTool, isHandLeft);
        if (currentHand == null)
        {
            return key1;
        }

        if (IsCurrentHandOccupied(isLeft))
        {
            return key1;
        }

        return isLeft ? 1 - OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) : 1 - OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
    }


}
