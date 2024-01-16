using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailBehaviourTest2 : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("Nail going in");
    }

    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log("Nail going out");
    }
}
