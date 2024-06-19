using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceMeasurer : MonoBehaviour
{
    [SerializeField] private Transform point1;

    [SerializeField] private Transform point2;

    private void Start()
    {
        Debug.Log(Vector3.Distance(point1.position, point2.position));
    }

    public float Distance()
    {
        return Vector3.Distance(point1.position, point2.position);
    }
}
