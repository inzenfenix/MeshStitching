using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NailBehaviour : MonoBehaviour
{
    public Transform rayPivotPoint;
    public LayerMask bodyLayer;
    public float rayDistance;

    public float nailPressure;


    private void Update()
    {
        Ray ray = new Ray(rayPivotPoint.position, rayPivotPoint.forward.normalized);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, bodyLayer))
        {
            hit.transform.GetComponent<DeformableMesh>().ApplyPressureToPoint(hit.point, nailPressure);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(rayPivotPoint.position, rayPivotPoint.forward.normalized * rayDistance);
    }
}
