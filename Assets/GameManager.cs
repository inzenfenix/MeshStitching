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

    private Hand leftHandLeap;
    private Hand rightHandLeap;

    private float minPinchDistance = 42f;

    public bool grabbingWithLeft = false;
    public bool grabbingWithRight = false;

    private Transform leftHookPoint = null;
    private Transform rightHookPoint = null;

    public static bool grabbingToolLeftHand = false;
    public static bool grabbingToolRightHand = false;

    public static bool isLeapMotion = false;
    public static bool isNovaGlove = false;

    public Transform[] leftFingerTips;
    public Transform[] rightFingerTips;


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

        if(isLeapMotion)
            LeapMotion();

        else if(isNovaGlove)
            NovaGlove();

    }

    private void LeapMotion()
    {
        //Check if any of the hands are in the scene
        leftHandLeap = Hands.Provider.GetHand(Chirality.Left);
        rightHandLeap = Hands.Provider.GetHand(Chirality.Right);

        //If we are not grabbing anything we check if we should try to grab the thread with the hand
        if (leftHandLeap != null && !grabbingToolLeftHand)
        {
            leftHookPoint.position = leftHandLeap.Fingers[0].TipPosition;

            if (leftHandLeap.PinchDistance < minPinchDistance && !grabbingWithLeft)
            {
                onHookedRope?.Invoke(this, leftHookPoint);
            }

            else if (leftHandLeap.PinchDistance > minPinchDistance && grabbingWithLeft)
            {
                onUnhookedRope?.Invoke(this, leftHookPoint);
            }
        }

        //We try the same with the right hand
        if (rightHandLeap != null && !grabbingToolRightHand)
        {
            rightHookPoint.position = rightHandLeap.Fingers[0].TipPosition;

            if (rightHandLeap.PinchDistance < minPinchDistance && !grabbingWithRight)
            {
                onHookedRope?.Invoke(this, rightHookPoint);
            }

            else if (rightHandLeap.PinchDistance > minPinchDistance && grabbingWithRight)
            {
                onUnhookedRope?.Invoke(this, rightHookPoint);
            }
        }

        if (leftHandLeap == null)
        {
            if (grabbingWithLeft)
            {
                onUnhookedRope?.Invoke(this, leftHookPoint);
            }
        }

        if (rightHandLeap == null)
        {
            if (grabbingWithRight)
            {
                onUnhookedRope?.Invoke(this, rightHookPoint);
            }
        }
    }

    private void NovaGlove()
    {

    }

    public static float LeapFingerPinchDistance(int finger, Hand hand)
    {
        if (hand == null)
            return 0;

        return hand.GetFingerPinchDistance(finger);
    }

    public static float LeapPalmDistance(Transform transform, Hand hand)
    {
        if (hand == null)
            return 0;

        return Vector3.Distance(hand.PalmPosition, transform.position);
    }



    public static Hand LeftHand
    {
        get => instance.leftHandLeap;
    }

    public static Hand RightHand
    {
        get => instance.rightHandLeap;
    }
}
