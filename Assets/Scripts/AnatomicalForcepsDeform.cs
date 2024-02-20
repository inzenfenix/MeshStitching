using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnatomicalForcepsDeform : MonoBehaviour
{
    public static GameObject currentSelectedForcep;

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


    private void Start()
    {
        originalTopRotation_Q = Quaternion.Euler(originalTopRotation);
        deformedlTopRotation_Q = Quaternion.Euler(deformedTopRotation);

        originalBottomRotation_Q = Quaternion.Euler(originalBottomRotation);
        deformedlBottomRotation_Q = Quaternion.Euler(deformedBottomRotation);
    }

    private void Update()
    {
        if (currentSelectedForcep != this.gameObject)
        {
            return;
        }

        //START TEST CODE
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
        }

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

    public void SelectForcep()
    {
        currentSelectedForcep = this.gameObject;
    }

}
