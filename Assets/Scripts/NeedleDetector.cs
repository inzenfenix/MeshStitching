using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeedleDetector : MonoBehaviour
{
    public enum Side
    {
        Left,
        Right
    };

    public static event EventHandler<Vector3> onNeedleEnter;

    public Side side;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BottomNeedle"))
        {
            Debug.Log("Needle inserted");
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            onNeedleEnter?.Invoke(this, collisionPoint);
        }
    }
}
