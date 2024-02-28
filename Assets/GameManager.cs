using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;

public class GameManager : MonoBehaviour
{
    private Hand leftHand;
    private Hand rightHand;

    public static GameManager instance;

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

       
    }

    private void Update()
    {
        leftHand = Hands.Provider.GetHand(Chirality.Left);
        rightHand = Hands.Provider.GetHand(Chirality.Right);

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
