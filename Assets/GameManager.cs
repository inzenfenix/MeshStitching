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

    [Header("\nCurrent controller")]
    public bool isLeapMotion = false;
    public bool isNovaGlove = false;

    [Header("\nNova transforms")]
    public Transform[] leftFingerTips;
    public Transform[] rightFingerTips;

    public Transform leftPalm;
    public Transform rightPalm;


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
        //If we are not grabbing anything we check if we should try to grab the thread with the hand
        if (!grabbingToolLeftHand)
        {
            leftHookPoint.position = leftFingerTips[0].position;

            if (NovaFingerDistance(0, 1, true) < minPinchDistance && !grabbingWithLeft)
            {
                onHookedRope?.Invoke(this, leftHookPoint);
            }

            else if (NovaFingerDistance(0, 1, true) > minPinchDistance && grabbingWithLeft)
            {
                onUnhookedRope?.Invoke(this, leftHookPoint);
            }
        }

        //We try the same with the right hand
        if (rightHandLeap != null && !grabbingToolRightHand)
        {
            rightHookPoint.position = rightFingerTips[0].position;

            if (NovaFingerDistance(0, 1, false) < minPinchDistance && !grabbingWithRight)
            {
                onHookedRope?.Invoke(this, rightHookPoint);
            }

            else if (NovaFingerDistance(0, 1, false) > minPinchDistance && grabbingWithRight)
            {
                onUnhookedRope?.Invoke(this, rightHookPoint);
            }
        }
    }

    public static float NovaFingerDistance(int finger, int finger2, bool isLeft)
    {
        if (!GameManager.instance.isNovaGlove)
            return -1;

        if(finger < 0 || finger > 4 || finger2 < 0 ||finger2 > 4)
            return -1;

        if(isLeft)
            return Vector3.Distance(GameManager.instance.leftFingerTips[finger].position, GameManager.instance.leftFingerTips[finger2].position);

        else
            return Vector3.Distance(GameManager.instance.rightFingerTips[finger].position, GameManager.instance.rightFingerTips[finger2].position);
    }

    public static float GetNovaFingerStrength(int finger, bool isLeft)
    {
        if (!GameManager.instance.isNovaGlove)
            return -1;

        if (finger < 0 || finger > 4)
            return -1;

        if (isLeft)
        {
            float fingerDistance = Vector3.Distance(GameManager.instance.leftFingerTips[finger].position, GameManager.instance.leftPalm.position);
            return Mathf.Clamp(1 - fingerDistance, 0, 1);
        }

        else
        {
            float fingerDistance = Vector3.Distance(GameManager.instance.rightFingerTips[finger].position, GameManager.instance.rightPalm.position);
            return Mathf.Clamp(1 - fingerDistance, 0, 1);
        }
    }

    public static float NovaPalmDistance(Transform pos, bool isLeft)
    {
        if (!GameManager.instance.isNovaGlove)
            return -1;

        if (isLeft)
            return Vector3.Distance(pos.position, GameManager.instance.leftPalm.position);

        else
            return Vector3.Distance(pos.position, GameManager.instance.rightPalm.position);
    }

    public static Transform NovaPalmNearby(Transform pos, out bool isLeft)
    {
        if (!GameManager.instance.isNovaGlove)
        {
            isLeft = false;
            return null;
        }

        if (Vector3.Distance(pos.position, GameManager.instance.leftPalm.position) < 0.1f)
        {
            isLeft = true;
            return GameManager.instance.leftPalm;
        }

        else if (Vector3.Distance(pos.position, GameManager.instance.rightPalm.position) < 0.1f)
        {   
            isLeft = false;
            return GameManager.instance.rightPalm;
        }

        isLeft = false;
        return null;
    }


    public static float LeapFingerPinchDistance(int finger, Hand hand)
    {
        if (hand == null)
            return -1;

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
