using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClothCorrection : MonoBehaviour
{
    private Mesh mesh;

    private Vector3[] defaultVertices;

    private bool isBeingPenetrated = true;

    private float distanceThreshold = 0.01f;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;

        defaultVertices = mesh.vertices;
    }

    private void Update()
    {
        if(isBeingPenetrated)
        {
            //If the nail is going in we don't want to interfere with the cloth simulation
            return;
        }

        int defaultVerticesAmount = 0;

        for(int i = 0; i < defaultVertices.Length; i++)
        {
            //We measure the distance between the original shape and the current modified shape
            if (Vector3.Distance(mesh.vertices[i], defaultVertices[i]) < distanceThreshold)
            {
                //If all of the vertices are close to its original shape, we finish the process of revert
                defaultVerticesAmount++;
                continue;
            }

            float speed = 25f * Time.deltaTime;

            //We lerp to the original position
            mesh.vertices[i] = Vector3.Lerp(mesh.vertices[i], defaultVertices[i], speed);
        }

        if(defaultVerticesAmount == mesh.vertices.Length)
        {
            isBeingPenetrated = true;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Nail going in");
        isBeingPenetrated = true;
    }

    private void OnCollisionExit(Collision other)
    {
        Debug.Log("Nail going out");
        isBeingPenetrated = false;
    }
}
