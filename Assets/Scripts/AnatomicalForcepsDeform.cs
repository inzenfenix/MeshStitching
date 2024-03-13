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

    [Header("TOP PART OF THE FORCEPS")]
    [SerializeField] private Transform TopBone;

    public Vector3 originalTopRotation;
    public Vector3 deformedTopRotation;

    [Range(0f, 1f)]
    public float topKey;

    [Header("\nBOTTOM PART OF THE FORCEPS")]
    [SerializeField] private Transform BottomBone;

    public Vector3 originalBottomRotation;
    public Vector3 deformedBottomRotation;

    [Range(0f, 1f)]
    public float bottomKey;

    private Quaternion originalTopRotation_Q;
    private Quaternion deformedlTopRotation_Q;

    private Quaternion originalBottomRotation_Q;
    private Quaternion deformedlBottomRotation_Q;

    private bool forcepsJoined = false;
    private float forcepsSpeed = 5.0f;

    protected override void Awake()
    {
        base.Awake();

        originalTopRotation_Q = Quaternion.Euler(originalTopRotation);
        deformedlTopRotation_Q = Quaternion.Euler(deformedTopRotation);

        originalBottomRotation_Q = Quaternion.Euler(originalBottomRotation);
        deformedlBottomRotation_Q = Quaternion.Euler(deformedBottomRotation);

        interactor = GetComponent<InteractionBehaviour>();
    }

    private void Update()
    {
        rb.isKinematic = true;

        Leap.Hand currentHand = ClosestHand();

        if (currentHand == null)
        {
            return;
        }

        if (IsCurrentHandOccupied(currentHand))
        {
            return;
        }

        if(currentHand.GrabStrength <= .075f)
        {
            if (selectedThisTool)
            {
                if (currentHand.IsLeft) leftHand.SetMaterialToNormal();
                else rightHand.SetMaterialToNormal();
                selectedThisTool = false;
                DeselectTool();

            }

            return;
        }

        transform.position = currentHand.PalmPosition;
        transform.rotation = currentHand.Rotation;

        if (!selectedThisTool)
        {
            if (currentHand.IsLeft) leftHand.SetTransparentHands();
            else rightHand.SetTransparentHands();
            SelectTool();
            selectedThisTool = true;
        }

        float value = -currentHand.PinchDistance / 15 + 2.25f;
        topKey = bottomKey = Mathf.Clamp(value, 0f, 1f);


        //START TEST CODE
        /*
        if (Input.GetKey(KeyCode.L))
        {
            float currentValue = topKey;
            float nextValue = topKey + Time.deltaTime * forcepsSpeed;

            currentValue = Mathf.Clamp(nextValue, 0f, 1f);

            topKey = currentValue;
            bottomKey = currentValue;
        }

        else
        {
            float currentValue = topKey;
            float nextValue = topKey - Time.deltaTime * forcepsSpeed;

            currentValue = Mathf.Clamp(nextValue, 0f, 1f);

            topKey = currentValue;
            bottomKey = currentValue;
        }*/

        //END TEST CODE

        //Moves the tool on its axis of rotation, if it passes the threshold then the tool's system activates
        TopBone.localRotation = Quaternion.Slerp(originalTopRotation_Q, deformedlTopRotation_Q, topKey);
        BottomBone.localRotation = Quaternion.Slerp(originalBottomRotation_Q, deformedlBottomRotation_Q, bottomKey);

        if (topKey > 0.85f && bottomKey > 0.85f && !forcepsJoined)
        {
            forcepsJoined = true;
            OnForcepsJoin?.Invoke(this, EventArgs.Empty);
        }

        if (topKey <= 0.7f && bottomKey <= 0.7f && forcepsJoined)
        {
            forcepsJoined = false;
            OnForcepsSeparate?.Invoke(this, EventArgs.Empty);
        }
    }

    public override void DeselectTool()
    {
        base.DeselectTool();

        topKey = 0;
        bottomKey = 0;
        forcepsJoined = false;
        OnForcepsSeparate?.Invoke(this, EventArgs.Empty);
    }
}
