using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookPointBehaviour : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;

    public Rigidbody GetRb()
    {
        return rb;
    }
}
