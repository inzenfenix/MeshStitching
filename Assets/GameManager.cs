using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using System;
using UnityEngine.SceneManagement;
using Meta.WitAi.Attributes;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static event EventHandler<Transform> onHookedRope;
    public static event EventHandler<Transform> onUnhookedRope;

    //[SerializeField] private CapsuleHand leftHandC;
    //[SerializeField] private CapsuleHand rightHandC;

    private Hand leftHandLeap;
    private Hand rightHandLeap;

    private float minPinchDistanceLeap = 49f;
    private float minPinchDistanceNova = .015f;

    public bool grabbingWithLeft = false;
    public bool grabbingWithRight = false;

    private Transform leftHookPoint = null;
    private Transform rightHookPoint = null;

    public static bool grabbingToolLeftHand = false;
    public static bool grabbingToolRightHand = false;

    [Header("\nCurrent controller")]
    public bool isLeapMotion = false;
    public bool isNovaGloveOrQuest = false;
    public bool isQuestControllers = false;

    [Header("\nNova/Quest transforms")]
    public Transform[] leftFingerTips;
    public Transform[] rightFingerTips;

    [Header("\nNova/Quest palms")]
    public Transform leftPalm;
    public Transform rightPalm;

    [Header("\nNova/Quest palms for forceps")]
    public Transform leftPalmForceps;
    public Transform rightPalmForceps;

    [Header("\nQuest VR Rig Camera")]
    [SerializeField] private Transform cameraRig;

    [Header("\nQuest controllers")]
    public Transform leftController;
    public Transform rightController;


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
            if (isNovaGloveOrQuest)
            {
                leftHookPoint.position = leftFingerTips[0].position;
                leftHookPoint.parent = leftFingerTips[0];
            }
            
            else if(isQuestControllers)
            {
                leftHookPoint.position = leftController.position;
                leftHookPoint.parent = leftController.parent;
            }
            
        }

        if (rightHookPoint == null)
        {
            rightHookPoint = (new GameObject()).transform;
            rightHookPoint.name = "RightHook";

            if (isNovaGloveOrQuest)
            {
                rightHookPoint.position = rightFingerTips[0].position;
                rightHookPoint.parent = rightFingerTips[0];
            }

            else if (isQuestControllers)
            {
                rightHookPoint.position = rightController.position;
                rightHookPoint.parent = rightController.parent;
            }
        }
    }

    private void Update()
    {
        if(Input.GetKey(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        if (isLeapMotion)
            LeapMotion();

        else if (isNovaGloveOrQuest)
            NovaGloveOrQuestHands();

        else if (isQuestControllers)
            QuestControllers();

        //leftHookPoint.rotation = leftPalm.rotation;
        //rightHookPoint.rotation = rightPalm.rotation;

        //if(OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger))
            //Debug.Log("Trigger activated");

        //Debug.Log(OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger));

    }

    private void OnEnable()
    {
        FinishMeshButton.OnButtonTouchedDecrease += FinishMeshButton_OnButtonTouchedDecrease;
        FinishMeshButton.OnButtonTouchedIncrease += FinishMeshButton_OnButtonTouchedIncrease;
        FinishMeshButton.OnButtonResetGame += FinishMeshButton_OnButtonResetGame;
    }

    

    private void OnDisable()
    {
        FinishMeshButton.OnButtonTouchedDecrease -= FinishMeshButton_OnButtonTouchedDecrease;
        FinishMeshButton.OnButtonTouchedIncrease -= FinishMeshButton_OnButtonTouchedIncrease;
        FinishMeshButton.OnButtonResetGame -= FinishMeshButton_OnButtonResetGame;
    }

    private void FinishMeshButton_OnButtonTouchedIncrease(object sender, EventArgs e)
    {
        cameraRig.position += Vector3.up * 5f * Time.deltaTime;
    }

    private void FinishMeshButton_OnButtonTouchedDecrease(object sender, EventArgs e)
    {
        cameraRig.position -= Vector3.up * 5f * Time.deltaTime;
    }

    private void FinishMeshButton_OnButtonResetGame(object sender, EventArgs e)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

            if (leftHandLeap.PinchDistance < minPinchDistanceLeap && !grabbingWithLeft)
            {
                onHookedRope?.Invoke(this, leftHookPoint);
            }

            else if (leftHandLeap.PinchDistance > minPinchDistanceLeap && grabbingWithLeft)
            {
                onUnhookedRope?.Invoke(this, leftHookPoint);
            }
        }

        //We try the same with the right hand
        if (rightHandLeap != null && !grabbingToolRightHand)
        {
            rightHookPoint.position = rightHandLeap.Fingers[0].TipPosition;

            if (rightHandLeap.PinchDistance < minPinchDistanceLeap && !grabbingWithRight)
            {
                onHookedRope?.Invoke(this, rightHookPoint);
            }

            else if (rightHandLeap.PinchDistance > minPinchDistanceLeap && grabbingWithRight)
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

    private void NovaGloveOrQuestHands()
    {
        //If we are not grabbing anything we check if we should try to grab the thread with the hand
        if (!grabbingToolLeftHand)
        {
            //leftHookPoint.position = leftFingerTips[0].position;

            if (NovaFingerDistance(0, 1, true) < minPinchDistanceNova && !grabbingWithLeft)
            {
                onHookedRope?.Invoke(this, leftHookPoint);
            }

            else if (NovaFingerDistance(0, 1, true) > minPinchDistanceNova && grabbingWithLeft)
            {
                onUnhookedRope?.Invoke(this, leftHookPoint);
            }
        }

        //We try the same with the right hand
        if (!grabbingToolRightHand)
        {

            //rightHookPoint.position = rightFingerTips[0].position;

            if (NovaFingerDistance(0, 1, false) < minPinchDistanceNova && !grabbingWithRight)
            {
                onHookedRope?.Invoke(this, rightHookPoint);
            }

            else if (NovaFingerDistance(0, 1, false) > minPinchDistanceNova && grabbingWithRight)
            {
                onUnhookedRope?.Invoke(this, rightHookPoint);
            }
        }
    }

    private void QuestControllers()
    {
        if (!grabbingToolLeftHand)
        {
            //leftHookPoint.position = leftFingerTips[0].position;

            if (OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && !grabbingWithLeft)
            {
                onHookedRope?.Invoke(this, leftHookPoint);
            }

            else if (!OVRInput.Get(OVRInput.Button.PrimaryHandTrigger) && grabbingWithLeft)
            {
                onUnhookedRope?.Invoke(this, leftHookPoint);
            }
        }

        //We try the same with the right hand
        if (!grabbingToolRightHand)
        {

            //rightHookPoint.position = rightFingerTips[0].position;

            if (OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && !grabbingWithRight)
            {
                onHookedRope?.Invoke(this, rightHookPoint);
            }

            else if (!OVRInput.Get(OVRInput.Button.SecondaryHandTrigger) && grabbingWithRight)
            {
                onUnhookedRope?.Invoke(this, rightHookPoint);
            }
        }
    }

    public static float NovaFingerDistance(int finger, int finger2, bool isLeft)
    {
        if (!GameManager.instance.isNovaGloveOrQuest)
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
        if (!GameManager.instance.isNovaGloveOrQuest)
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
        if (!GameManager.instance.isNovaGloveOrQuest)
            return -1;

        if (isLeft)
            return Vector3.Distance(pos.position, GameManager.instance.leftPalm.position);

        else
            return Vector3.Distance(pos.position, GameManager.instance.rightPalm.position);
    }

    public static Transform NovaQuestPalmNearby(Transform pos,  out bool isLeft, float offset = 0, bool isForceps = false)
    {
        if (!GameManager.instance.isNovaGloveOrQuest)
        {
            isLeft = false;
            return null;
        }

        if (Vector3.Distance(pos.position, GameManager.instance.leftPalm.position) < 0.05f + offset)
        {
            isLeft = true;

            if(isForceps) return GameManager.instance.leftPalmForceps;

            return GameManager.instance.leftPalm;
        }

        else if (Vector3.Distance(pos.position, GameManager.instance.rightPalm.position) < 0.05f + offset)
        {   
            isLeft = false;

            if (isForceps) return GameManager.instance.rightPalmForceps;

            return GameManager.instance.rightPalm;
        }

        isLeft = false;
        return null;
    }

    public static Transform QuestControllerNearby(Transform pos, out bool isLeft, float offset = 0, bool isForceps = false, bool isToolSelected = false, bool isHandLeft = false)
    {
        if (!GameManager.instance.isQuestControllers)
        {
            isLeft = false;
            return null;
        }

        if (isToolSelected)
        {
            if (Vector3.Distance(pos.position, GameManager.instance.leftController.position) < 0.05f + offset && isHandLeft)
            {
                isLeft = true;

                if (GameManager.instance.grabbingWithLeft) return null;

                return GameManager.instance.leftController;
            }

            else if (Vector3.Distance(pos.position, GameManager.instance.rightController.position) < 0.05f + offset && !isHandLeft)
            {
                isLeft = false;

                if (GameManager.instance.grabbingWithRight) return null;

                return GameManager.instance.rightController;
            }
        }

        else
        {
            if (Vector3.Distance(pos.position, GameManager.instance.leftController.position) < 0.05f + offset)
            {
                isLeft = true;

                if (GameManager.instance.grabbingWithLeft) return null;

                return GameManager.instance.leftController;
            }

            else if (Vector3.Distance(pos.position, GameManager.instance.rightController.position) < 0.05f + offset)
            {
                isLeft = false;

                if (GameManager.instance.grabbingWithRight) return null;

                return GameManager.instance.rightController;
            }
        }

        isLeft = false;
        return null;
    }

    public static Transform NovaPalmNearbyRadius(Transform pos, float radius, out bool isLeft)
    {
        if (!GameManager.instance.isNovaGloveOrQuest)
        {
            isLeft = false;
            return null;
        }

        if (Vector3.Distance(pos.position, GameManager.instance.leftPalm.position) < radius)
        {
            isLeft = true;
            return GameManager.instance.leftPalm;
        }

        else if (Vector3.Distance(pos.position, GameManager.instance.rightPalm.position) < radius)
        {
            isLeft = false;
            return GameManager.instance.rightPalm;
        }

        isLeft = false;
        return null;
    }

    public static Transform QuestControllerNearbyRadius(Transform pos, float radius, out bool isLeft)
    {
        if (!GameManager.instance.isQuestControllers)
        {
            isLeft = false;
            return null;
        }

        if (Vector3.Distance(pos.position, GameManager.instance.leftController.position) < radius)
        {
            isLeft = true;
            return GameManager.instance.leftController;
        }

        else if (Vector3.Distance(pos.position, GameManager.instance.rightController.position) < radius)
        {
            isLeft = false;
            return GameManager.instance.rightController;
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
       /* if(leftHookPoint != null)
            Gizmos.DrawSphere(leftHookPoint.position, .025f);*/
    }
}
