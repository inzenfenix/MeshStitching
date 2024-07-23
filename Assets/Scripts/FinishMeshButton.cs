using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public enum OptionsButton
{
    finish,
    increaseHeight,
    decreaseHeight,
    resetGame
};

public class FinishMeshButton : MonoBehaviour
{
    public static event EventHandler OnButtonTouchedFinish;

    public static event EventHandler OnButtonTouchedIncrease;

    public static event EventHandler OnButtonTouchedDecrease;

    public static event EventHandler OnButtonResetGame;


    private float delay;

    [SerializeField] private OptionsButton buttonOptions;

    private void Update()
    {
        if(delay > 0)
        {
            delay -= Time.deltaTime;
            return;
        }

        if (GameManager.instance.isLeapMotion)
        {
            Leap.Hand hand = ClosestHand();

            if (hand == null)
            {
                return;
            }
        }

        else if(GameManager.instance.isNovaGloveOrQuest)
        {
            Transform pos = GameManager.NovaPalmNearbyRadius(transform, 0.1f,out bool isLeft);

            if(pos == null)
            {
                return;
            }
        }

        else if (GameManager.instance.isQuestControllers)
        {
            Transform pos = GameManager.QuestControllerNearbyRadius(transform, 0.1f, out bool isLeft);

            if (pos == null)
            {
                return;
            }
        }

        else
        {
            return;
        }

        delay = 2.5f;

        switch(buttonOptions)
        {
            case OptionsButton.finish:
                OnButtonTouchedFinish?.Invoke(this, EventArgs.Empty);
                break;
            case OptionsButton.increaseHeight:
                OnButtonTouchedIncrease?.Invoke(this, EventArgs.Empty);
                break;
            case OptionsButton.decreaseHeight:
                OnButtonTouchedDecrease?.Invoke(this, EventArgs.Empty);
                break;
            case OptionsButton.resetGame:
                OnButtonResetGame?.Invoke(this, EventArgs.Empty);
                break;
            default:
                break;
        }
        
    }

    Leap.Hand ClosestHand()
    {
        float minDistance = 0.1f;
        Leap.Hand selectedHand = null;

        if (GameManager.LeftHand == null &&
           GameManager.RightHand != null)
        {
            if (Vector3.Distance(GameManager.RightHand.PalmPosition, this.transform.position) < minDistance)
            {
                selectedHand = GameManager.RightHand;
            }
        }

        else if (GameManager.LeftHand != null &&
                 GameManager.RightHand == null)
        {
            if (Vector3.Distance(GameManager.LeftHand.PalmPosition, this.transform.position) < minDistance)
            {
                selectedHand = GameManager.LeftHand;
            }
        }

        else if (GameManager.LeftHand == null ||
                 GameManager.RightHand == null)
        {
            return null;
        }

        else if (Vector3.Distance(GameManager.LeftHand.PalmPosition, this.transform.position) <
                 Vector3.Distance(GameManager.RightHand.PalmPosition, this.transform.position))
        {
            if (Vector3.Distance(GameManager.LeftHand.PalmPosition, this.transform.position) < minDistance)
            {
                selectedHand = GameManager.LeftHand;
            }
        }

        else
        {
            if (Vector3.Distance(GameManager.RightHand.PalmPosition, this.transform.position) < minDistance)
            {
                selectedHand = GameManager.RightHand;
            }
        }

        return selectedHand;
    }
}
