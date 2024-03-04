using Obi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MISNeedleBehaviour : MedicalTool
{
    [Header("Needle components")]
    [SerializeField] private Transform needleTop;
    [SerializeField] private Transform needleBottom;

    [Header("Insertion Components")]
    [SerializeField] private Collider[] colliders;
    private bool isNeedleInserted;

    [SerializeField] Transform forcepsHookPoint;


    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    { 
        NeedleDetector.onNeedleExit += NeedleDetector_onNeedleExit;
        NeedleDetector.onNeedleEnter += NeedleDetector_onNeedleEnter;
        NeedleDetector.onNeedleMidEnter += NeedleDetector_onNeedleMidEnter;
        NeedleDetector.onNeedleMidExit += NeedleDetector_onNeedleMidExit;

        AnatomicalForcepsBehaviour.onHookedRope += OnHookedRope;
        AnatomicalForcepsBehaviour.onUnhookedRope += OnUnhookedRope;
    }

    private void OnDisable()
    {
        NeedleDetector.onNeedleExit -= NeedleDetector_onNeedleExit;
        NeedleDetector.onNeedleEnter -= NeedleDetector_onNeedleEnter;
        NeedleDetector.onNeedleMidEnter -= NeedleDetector_onNeedleMidEnter;
        NeedleDetector.onNeedleMidExit -= NeedleDetector_onNeedleMidExit;

        AnatomicalForcepsBehaviour.onHookedRope -= OnHookedRope;
        AnatomicalForcepsBehaviour.onUnhookedRope -= OnUnhookedRope;
    }

    private void Update()
    {

        rb.velocity = Vector3.zero;

        //Test code
        if (Input.GetKey(KeyCode.W))
        {
            float zSpeed = 1.5f;
            Vector3 zMovement = transform.position + Vector3.forward * zSpeed * Time.deltaTime;
            rb.MovePosition(zMovement);

        }

        if (Input.GetKey(KeyCode.S))
        {
            float zSpeed = -1.5f;
            Vector3 zMovement = transform.position + Vector3.forward * zSpeed * Time.deltaTime;
            rb.MovePosition(zMovement);
        }

        if (Input.GetKey(KeyCode.A))
        {
            float xSpeed = -1.5f;
            Vector3 zMovement = transform.position + Vector3.right * xSpeed * Time.deltaTime;
            rb.MovePosition(zMovement);
        }

        if (Input.GetKey(KeyCode.D))
        {
            float xSpeed = 1.5f;
            Vector3 zMovement = transform.position + Vector3.right * xSpeed * Time.deltaTime;
            rb.MovePosition(zMovement);
        }

        if (Input.GetKey(KeyCode.E))
        {
            float rightRotationSpeed = 100f;
            Vector3 rightRotation =  new Vector3(0, 0, rightRotationSpeed * Time.deltaTime);
            transform.Rotate(rightRotation, Space.World);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            float lefttRotationSpeed = -100f;
            Vector3 leftRotation =  new Vector3(0, 0, lefttRotationSpeed * Time.deltaTime);
            transform.Rotate(leftRotation, Space.World);
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            float ySpeed = -1.5f;
            Vector3 yMovement = transform.position + Vector3.up * ySpeed * Time.deltaTime;
            rb.MovePosition(yMovement);
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            float ySpeed = 1.5f;
            Vector3 yMovement = transform.position + Vector3.up * ySpeed * Time.deltaTime;
            rb.MovePosition(yMovement);
        }
    }


    private void NeedleDetector_onNeedleEnter(object sender, Vector3 e)
    {
        isNeedleInserted = true;
    }

    private void NeedleDetector_onNeedleExit(object sender, Vector3 e)
    {
        isNeedleInserted = false;

        for(int i = 0; i < colliders.Length; i++)
        {
            colliders[i].isTrigger = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject);
    }

    private void NeedleDetector_onNeedleMidEnter(object sender, Collider e)
    {
        if(!isNeedleInserted)
        {
            return;
        }

        SetTriggerOnOff(e, true);
    }

    private void NeedleDetector_onNeedleMidExit(object sender, Collider e)
    {
        if(!isNeedleInserted)
        {
            return;
        }

        SetTriggerOnOff(e, false);
    }

    private void SetTriggerOnOff(Collider collider, bool enabled)
    {
        collider.isTrigger = enabled;
    }

    private void OnHookedRope(object sender, Transform forceps)
    {
        float forcepsDistanceThreshold = .1f;

        //Check that the forceps are not too far from the needle
        if (Vector3.Distance(forcepsHookPoint.position, forceps.position) > forcepsDistanceThreshold)
        {
            return;
        }

        this.transform.parent = forceps;
    }

    private void OnUnhookedRope(object sender, Transform forceps)
    {
        this.transform.parent = null;
    }
}
