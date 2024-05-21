using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using System;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static event EventHandler<Transform> onHookedRope;
    public static event EventHandler<Transform> onUnhookedRope;

    //[SerializeField] private CapsuleHand leftHandC;
    //[SerializeField] private CapsuleHand rightHandC;

    private Hand leftHand;
    private Hand rightHand;

    private float minPinchDistance = 36f;

    public bool grabbingWithLeft = false;
    public bool grabbingWithRight = false;

    private Transform leftHookPoint = null;
    private Transform rightHookPoint = null;

    public static bool grabbingToolLeftHand = false;
    public static bool grabbingToolRightHand = false;


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
            leftHookPoint.name = "LeftHook";
        }

        if (rightHookPoint == null)
        {
            rightHookPoint = (new GameObject()).transform;
            rightHookPoint.name = "RightHook";
        }
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        //Check if any of the hands are in the scene
        leftHand = Hands.Provider.GetHand(Chirality.Left);
        rightHand = Hands.Provider.GetHand(Chirality.Right);

        //If we are not grabbing anything we check if we should try to grab the thread with the hand
        if(leftHand != null && !grabbingToolLeftHand)
        {
            leftHookPoint.position = leftHand.Fingers[0].TipPosition;

            if (leftHand.PinchDistance < minPinchDistance && !grabbingWithLeft)
            {
                onHookedRope?.Invoke(this, leftHookPoint);
            }

            else if(leftHand.PinchDistance > minPinchDistance && grabbingWithLeft)
            {
                onUnhookedRope?.Invoke(this, leftHookPoint);
            }
        }

        //We try the same with the right hand
        if (rightHand != null && !grabbingToolRightHand)
        {
            rightHookPoint.position = rightHand.Fingers[0].TipPosition;

            if (rightHand.PinchDistance < minPinchDistance && !grabbingWithRight)
            {
                onHookedRope?.Invoke(this, rightHookPoint);
            }

            else if (rightHand.PinchDistance > minPinchDistance && grabbingWithRight)
            {
                onUnhookedRope?.Invoke(this, rightHookPoint);
            }
        }

        if (leftHand == null)
        {
            if(grabbingWithLeft)
            {
                onUnhookedRope?.Invoke(this, leftHookPoint);
            }
        }

        if (rightHand == null)
        {
            if (grabbingWithRight)
            {
                onUnhookedRope?.Invoke(this, rightHookPoint);
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
