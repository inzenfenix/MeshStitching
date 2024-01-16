using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeformableMesh : MonoBehaviour
{
    private DeformVertex[] deformableVertices;
    private Vector3[] currentMeshVertices;
    private Mesh mesh;

    public float bounceSpeed;
    public float fallForce;
    public float stiffness;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        GetVertices();
    }

    private void Update()
    {
        UpdateVertices();
    }

    private void GetVertices()
    {
        //Initialize the deformable vertices array with each vertex of the mesh
        deformableVertices = new DeformVertex[mesh.vertices.Length];
        currentMeshVertices = new Vector3[mesh.vertices.Length];

        for (int i = 0; i < deformableVertices.Length; i++)
        {
            deformableVertices[i] = new DeformVertex(i, mesh.vertices[i], mesh.vertices[i], Vector3.zero);
            currentMeshVertices[i] = mesh.vertices[i];
        }
    }

    private void UpdateVertices()
    {
        for(int i = 0; i < deformableVertices.Length; i++)
        {
            deformableVertices[i].UpdateVelocity(bounceSpeed);
            deformableVertices[i].Settle(stiffness);

            deformableVertices[i].currentVertexPosition += deformableVertices[i].currentVelocity * Time.deltaTime;
            currentMeshVertices[i] = deformableVertices[i].currentVertexPosition;
        }

        mesh.vertices = currentMeshVertices;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }

    public void ApplyPressureToPoint(Vector3 point, float pressure)
    {
        for(int i = 0;i < deformableVertices.Length;i++)
        {
            deformableVertices[i].ApplyPressureToVertex(transform, point, pressure);
        }
    }
}
