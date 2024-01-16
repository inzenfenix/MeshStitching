using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformVertex
{
    public int vertexIndex;
    public Vector3 startingVertexPosition;
    public Vector3 currentVertexPosition;
    public Vector3 currentVelocity;

    public DeformVertex(int vertexIndex, Vector3 startingVertexPosition, Vector3 currentVertexPosition, Vector3 currentVelocity)
    {
        this.vertexIndex = vertexIndex;
        this.startingVertexPosition = startingVertexPosition;
        this.currentVertexPosition = currentVertexPosition;
        this.currentVelocity = currentVelocity;
    }

    public Vector3 GetCurrentDisplacement()
    {
        return currentVertexPosition - startingVertexPosition;
    }

    public void UpdateVelocity(float bounceSpeed)
    {
        currentVelocity -= GetCurrentDisplacement() * bounceSpeed * Time.deltaTime;
    }

    public void Settle(float stiffness)
    {
        currentVelocity *= 1f - stiffness * Time.deltaTime;
    }

    public void ApplyPressureToVertex(Transform otherTransform, Vector3 pos, float pressure)
    {
        Vector3 distanceVerticePoint = currentVertexPosition - otherTransform.InverseTransformPoint(pos);
        float adaptedPressure = pressure / (1f + distanceVerticePoint.sqrMagnitude);

        float velocity = adaptedPressure * Time.deltaTime;
        currentVelocity += distanceVerticePoint.normalized * velocity;
    }
}
