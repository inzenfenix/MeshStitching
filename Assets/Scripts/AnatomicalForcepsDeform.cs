using Leap.Unity;
using Leap.Unity.Interaction;
using Leap.Unity.PhysicalHands;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class AnatomicalForcepsDeform : MedicalTool
{

    public event EventHandler OnForcepsJoin;
    public event EventHandler OnForcepsSeparate;

    private bool forcepsJoined = false;

    [Header("TOP PART OF THE FORCEPS")]
    public Vector3 originalTopRotation;
    public Vector3 deformedTopRotation;

    [Header("\nBOTTOM PART OF THE FORCEPS")]
    public Vector3 originalBottomRotation;
    public Vector3 deformedBottomRotation;

    protected override void Awake()
    {
        base.Awake();

        //Rotations of the scissors, original is closed and deformed is open
        originalRotation1 = Quaternion.Euler(originalTopRotation);
        goalRotation1 = Quaternion.Euler(deformedTopRotation);

        originalRotation2 = Quaternion.Euler(originalBottomRotation);
        goalRotation2 = Quaternion.Euler(deformedBottomRotation);

        interactor = GetComponent<InteractionBehaviour>();
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
        firstComponent.localRotation = Quaternion.Slerp(goalRotation1, originalRotation1, key1);
        secondComponent.localRotation = Quaternion.Slerp(goalRotation2, originalRotation2, key2);

        if (key1 <= 0.2f && key2 <= 0.2f && !forcepsJoined)
        {
            forcepsJoined = true;
            OnForcepsJoin?.Invoke(this, EventArgs.Empty);
        }

        if (key1 > 0.4f && key2 > 0.4f && forcepsJoined)
        {
            forcepsJoined = false;
            OnForcepsSeparate?.Invoke(this, EventArgs.Empty);
        }
    }

    private float WithLeapMotion()
    {
        Leap.Hand currentHand = ClosestHandLeap();
        if (currentHand == null)
        {
            return key1;
        }

        if (IsCurrentHandOccupied(currentHand.IsLeft))
        {
            return key1;
        }

        float value = currentHand.PinchDistance / 16 - 1.01f;

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

        float value = GameManager.NovaFingerDistance(0, 1, isLeft) * 11 - .08f;
        return Mathf.Clamp(value, 0f, 1f);
    }

    private float WithQuestControllers()
    {
        Transform currentHand = GameManager.QuestControllerNearby(this.transform, out bool isLeft);
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

    public override void DeselectTool()
    {
        base.DeselectTool();

        key1 = 0;
        key2 = 0;
        forcepsJoined = false;
        OnForcepsSeparate?.Invoke(this, EventArgs.Empty);
    }
}
