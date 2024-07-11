using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

public class FinishMeshButton : MonoBehaviour
{
    public static event EventHandler OnButtonTouched;

    private float delay;

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

        else if(GameManager.instance.isNovaGlove)
        {
            Transform pos = GameManager.NovaPalmNearby(transform, 0.1f,out bool isLeft);

            if(pos == null)
            {
                return;
            }
        }

        else
        {
            return;
        }

        delay = 2.5f;
        OnButtonTouched?.Invoke(this, EventArgs.Empty);
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
