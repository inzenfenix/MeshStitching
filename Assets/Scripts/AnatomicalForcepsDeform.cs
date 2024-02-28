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
    [SerializeField]private Transform BottomBone;

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
        if (selectedTool != this.gameObject)
        {
            return;
        }

        Leap.Hand currentHand = GameManager.RightHand;

        if(handIsLeft)
        {
            currentHand = GameManager.LeftHand;
        }

        float value = -currentHand.PinchDistance / 15 + 2.5f;
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

        TopBone.localRotation = Quaternion.Slerp(originalTopRotation_Q, deformedlTopRotation_Q, topKey);
        BottomBone.localRotation = Quaternion.Slerp(originalBottomRotation_Q, deformedlBottomRotation_Q, bottomKey);

        if(topKey > 0.9f && bottomKey > 0.9f && !forcepsJoined)
        {
            forcepsJoined = true;
            OnForcepsJoin?.Invoke(this, EventArgs.Empty);
        }

        if(topKey <= 0.8f && bottomKey <= 0.8f && forcepsJoined)
        {
            forcepsJoined = false;
            OnForcepsSeparate?.Invoke(this, EventArgs.Empty);
        }
    }

    public override void DeselectTool(GameObject tool)
    {
        base.DeselectTool(tool);

        topKey = 0;
        bottomKey = 0;
        forcepsJoined = false;
        OnForcepsSeparate?.Invoke(this, EventArgs.Empty);
    }
}
