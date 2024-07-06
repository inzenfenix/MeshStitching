using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleDetector : MonoBehaviour
{
    public enum Side
    {
        Left,
        Right,
        LeftDown,
        RightDown
    };

    public static event EventHandler<Vector3> onNeedleEnter;
    public static event EventHandler<Vector3> onNeedleExit;

    public static event EventHandler<Collider> onNeedleMidEnter;
    public static event EventHandler<Collider> onNeedleMidExit;

    public Side side;

    private void Start()
    {
        int a = 1 + 1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BottomNeedle"))
        {
            Debug.Log("Needle inserted");
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            onNeedleEnter?.Invoke(this, collisionPoint);
        }

        /*if (other.CompareTag("TopNeedle"))
        {
            Debug.Log("Needle inserted");
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            onNeedleExit?.Invoke(this, collisionPoint);
        }*/

        if (other.CompareTag("TopNeedle"))
        {
            Debug.Log("Needle out");
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            onNeedleExit?.Invoke(this, collisionPoint);
        }

        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("MidNeedle"))
        {
            onNeedleMidEnter.Invoke(this, collision.collider);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MidNeedle"))
        {
            onNeedleMidExit.Invoke(this, other);
        }
    }
}
