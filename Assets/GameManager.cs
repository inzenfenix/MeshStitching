using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static event EventHandler<Transform> onHookedRope;
    public static event EventHandler<Transform> onUnhookedRope;

    //[SerializeField] private CapsuleHand leftHandC;
    //[SerializeField] private CapsuleHand rightHandC;

    private Hand leftHand;
    private Hand rightHand;

    private float minPinchDistance = 18f;

    private bool grabbingWithLeft = false;
    private bool grabbingWithRight = false;

    private Transform leftHookPoint = null;
    private Transform rightHookPoint = null;


    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }

        else
        {
            Destroy(gameObject);
        }

        if(leftHookPoint == null)
        {
            leftHookPoint = (new GameObject()).transform;
        }

        if (rightHookPoint == null)
        {
            rightHookPoint = (new GameObject()).transform;
        }
    }

    private void Update()
    {

        leftHand = Hands.Provider.GetHand(Chirality.Left);
        rightHand = Hands.Provider.GetHand(Chirality.Right);


        if(leftHand != null)
        {
            leftHookPoint.position = leftHand.Fingers[0].TipPosition;

            if (leftHand.PinchDistance < minPinchDistance && !grabbingWithLeft)
            {
                grabbingWithLeft = true;
                onHookedRope?.Invoke(this, leftHookPoint);
            }

            else if(leftHand.PinchDistance > minPinchDistance && grabbingWithLeft)
            {
                grabbingWithLeft = false;
                onUnhookedRope?.Invoke(this, leftHookPoint);
            }
        }

        if (rightHand != null)
        {
            rightHookPoint.position = rightHand.Fingers[0].TipPosition;

            if (rightHand.PinchDistance < minPinchDistance && !grabbingWithRight)
            {
                grabbingWithRight = true;
                onHookedRope?.Invoke(this, rightHookPoint);
            }

            else if (rightHand.PinchDistance > minPinchDistance && grabbingWithRight)
            {
                grabbingWithRight = false;
                onUnhookedRope?.Invoke(this, rightHookPoint);
            }
        }

        if (leftHand == null)
        {
            if(grabbingWithLeft)
            {
                onUnhookedRope?.Invoke(this, leftHookPoint);
                grabbingWithLeft = false;
            }
        }

        if (rightHand == null)
        {
            if (grabbingWithRight)
            {
                onUnhookedRope?.Invoke(this, rightHookPoint);
                grabbingWithRight = false;
            }
        }
    }

    public static Hand LeftHand
    {
        get => instance.leftHand;
    }

    public static Hand RightHand
    {
        get => instance.rightHand;
    }
}
