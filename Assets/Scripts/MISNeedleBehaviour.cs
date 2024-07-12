using Obi;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MISNeedleBehaviour : MedicalTool
{
    [Header("Are you using keyboard?")]
    [SerializeField] private bool usingKeyboard;

    [Header("\nNeedle components")]
    [SerializeField] private Transform needleTop;
    [SerializeField] private Transform needleBottom;

    [Header("\nInsertion Components")]
    [SerializeField] private Collider[] colliders;

    private int isNeedleInserted;

    [SerializeField] Transform[] forcepsHookPoints;

    private Transform currentTool;

    [SerializeField] Transform suturingTransform;

    private bool startedSuturing = false;

    private Transform currentParent;

    private Vector3 posOffset;
    Quaternion rotOffset = Quaternion.identity;

    private bool selectedNeedle = true;


    protected override void Awake()
    {
        base.Awake();

        selectedNeedle = true;
    }

    private void OnEnable()
    { 
        NeedleDetector.onNeedleExit += NeedleDetector_onNeedleExit;
        NeedleDetector.onNeedleEnter += NeedleDetector_onNeedleEnter;
        NeedleDetector.onNeedleMidEnter += NeedleDetector_onNeedleMidEnter;
        NeedleDetector.onNeedleMidExit += NeedleDetector_onNeedleMidExit;

        AnatomicalForcepsBehaviour.onHookedRope += OnHookedNeedle;
        AnatomicalForcepsBehaviour.onUnhookedRope += OnUnhookedNeedle;

        HoldScissorsBehaviour.onHookedRope += OnHookedNeedle;
        HoldScissorsBehaviour.onUnhookedRope += OnUnhookedNeedle;
    }

    private void OnDisable()
    {
        NeedleDetector.onNeedleExit -= NeedleDetector_onNeedleExit;
        NeedleDetector.onNeedleEnter -= NeedleDetector_onNeedleEnter;
        NeedleDetector.onNeedleMidEnter -= NeedleDetector_onNeedleMidEnter;
        NeedleDetector.onNeedleMidExit -= NeedleDetector_onNeedleMidExit;

        AnatomicalForcepsBehaviour.onHookedRope -= OnHookedNeedle;
        AnatomicalForcepsBehaviour.onUnhookedRope -= OnUnhookedNeedle;

        HoldScissorsBehaviour.onHookedRope -= OnHookedNeedle;
        HoldScissorsBehaviour.onUnhookedRope -= OnUnhookedNeedle;
    }

    protected override void Update()
    {

        if (selectedNeedle)
        {
            rb.isKinematic = true;
        }

        else
        {
            rb.isKinematic = false;
        }

        //Test code
        if (usingKeyboard)
        {
            KeyboardMovement();
        }

        if (GameManager.instance.isLeapMotion)
        {
            //UsingLeapMotion();
        }

        if (isNeedleInserted > 0)
        {
            if(!startedSuturing)
            {
                
                startedSuturing = true;
                currentParent = suturingTransform;
                transform.parent = suturingTransform;

                posOffset = transform.position - currentParent.position;
                rotOffset = Quaternion.Inverse(currentParent.rotation) * transform.rotation;
                rb.velocity = Vector3.zero;

            }

            if (currentTool == null) return;

            float forcepsDistanceThreshold = .05f;
            bool isTooFar = true;

            for (int i = 0; i < forcepsHookPoints.Length; i++)
            {
                if (Vector3.Distance(forcepsHookPoints[i].position, currentTool.position) < forcepsDistanceThreshold)
                {
                    isTooFar = false;
                    break;
                }
            }

            if (isTooFar)
            {
                OnUnhookedNeedle(isTooFar, currentTool);
                return;
            }

            Transform tool = currentTool;
            Rigidbody toolRb = tool.GetComponent<HookPointBehaviour>().GetRb();

            if (toolRb == null) return;

            float direction = 1;

            if (toolRb.velocity.magnitude < 0.0000001f && toolRb.velocity.magnitude > -0.0000001f) return;

            if(suturingTransform.rotation.eulerAngles.z < 147)
            {
                if(toolRb.velocity.y < 0 && toolRb.name.Contains("Hold"))
                {
                    return;
                }

                else if (toolRb.velocity.y < 0 && toolRb.name.Contains("Forceps"))
                {
                    return;
                }
            }

            else if (toolRb.velocity.y > 0 && toolRb.name.Contains("Hold"))
            {
                return;
            }

            float magnitude = Mathf.Clamp(toolRb.velocity.magnitude + toolRb.angularVelocity.magnitude, 0, .5f);

            suturingTransform.RotateAround(suturingTransform.position, suturingTransform.up, 200f * -magnitude * Time.deltaTime * direction);

            return;
        }

        if(isNeedleInserted == 0 && startedSuturing)
        {
            startedSuturing = false;
            suturingTransform.parent = this.transform;
            transform.parent = null;
            currentTool = null;
            if(currentParent == suturingTransform)
            {
                currentParent = null;
            }
        }

        if (currentParent == null) return;

        Vector3 newPos = posOffset + currentParent.position;

        rb.MoveRotation(currentParent.rotation * rotOffset);
        rb.MovePosition(newPos);
    }

    private void KeyboardMovement()
    {
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
            Vector3 rightRotation = new Vector3(0, 0, rightRotationSpeed * Time.deltaTime);
            transform.Rotate(rightRotation, Space.World);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            float lefttRotationSpeed = -100f;
            Vector3 leftRotation = new Vector3(0, 0, lefttRotationSpeed * Time.deltaTime);
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
        if (currentParent == null) return;

        if (!startedSuturing)
        {
            selectedNeedle = true;
            suturingTransform.parent = null;
            
            transform.parent = null;
            currentParent = null;

            transform.parent = suturingTransform;
        }

        isNeedleInserted++;
    }

    private void NeedleDetector_onNeedleExit(object sender, Vector3 e)
    {
        if (currentParent == null) return;

        isNeedleInserted--;

        if (isNeedleInserted < 0) isNeedleInserted = 0;

        for(int i = 0; i < colliders.Length; i++)
        {
            //colliders[i].isTrigger = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log(collision.gameObject);
    }

    private void NeedleDetector_onNeedleMidEnter(object sender, Collider e)
    {
        if(isNeedleInserted == 0)
        {
            return;
        }

        //SetTriggerOnOff(e, true);
    }

    private void NeedleDetector_onNeedleMidExit(object sender, Collider e)
    {
        if(isNeedleInserted == 0)
        {
            return;
        }

        //SetTriggerOnOff(e, false);
    }

    private void SetTriggerOnOff(Collider collider, bool enabled)
    {
        collider.isTrigger = enabled;
    }

    //If we want to grab the needle with the forceps
    private void OnHookedNeedle(object sender, Transform forceps)
    {
        float forcepsDistanceThreshold = .04f;
        Vector3 selectedHookPoint = new Vector3(int.MinValue, int.MinValue, int.MinValue);

        for(int i = 0; i < forcepsHookPoints.Length; i++)
        {
            if (Vector3.Distance(forcepsHookPoints[i].position, forceps.position) < forcepsDistanceThreshold)
            {
                selectedHookPoint = forcepsHookPoints[i].position;
                break;
            }
        }

        //Check that the forceps are not too far from the needle
        if(selectedHookPoint == new Vector3(int.MinValue, int.MinValue, int.MinValue))
        {
            return;
        }

        currentTool = forceps;

        if (isNeedleInserted <= 0)
        {
            selectedNeedle = true;
            posOffset = transform.position - forceps.position;
            rotOffset = Quaternion.Inverse(forceps.rotation) * transform.rotation;
            currentParent = forceps;
            rb.velocity = Vector3.zero;
            //this.transform.parent = forceps;
        }
    }

    private void OnUnhookedNeedle(object sender, Transform forceps)
    {
        if(currentTool == null)
        {
            return;
        }

        currentTool = null;
        if (isNeedleInserted <= 0)
        {
            selectedNeedle = false;
            currentParent = null;
            this.transform.parent = null;
            rb.velocity = Vector3.zero;
        }
       
    }
}
