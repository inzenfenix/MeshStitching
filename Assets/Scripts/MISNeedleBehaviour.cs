using Obi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MISNeedleBehaviour : MonoBehaviour
{

    [Header("Needle components")]
    [SerializeField] private Transform needleTop;
    [SerializeField] private Transform needleBottom;

    [Header("Prefab for the thread collider")]
    [SerializeField] private GameObject torusColliderPrefab;

    private List<GameObject> torusColliders;

    private float minTorusDistance = .15f;

    private Rigidbody rb;


    private void Start()
    {
        torusColliders = new List<GameObject>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    { 
        NeedleDetector.onNeedleExit += NeedleDetector_onNeedleExit;
    }

    private void OnDisable()
    {
        NeedleDetector.onNeedleExit -= NeedleDetector_onNeedleExit;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            float zSpeed = 1.5f;
            Vector3 zMovement = transform.position + Vector3.forward * zSpeed * Time.deltaTime;
            rb.MovePosition(zMovement);
        }

        if (Input.GetKey(KeyCode.S))
        {
            float zSpeed = -1.5f;
            Vector3 zMovement = transform.position + Vector3.forward * zSpeed * Time.deltaTime;
            rb.MovePosition(zMovement);
        }

        if (Input.GetKey(KeyCode.A))
        {
            float xSpeed = -1.5f;
            Vector3 zMovement = transform.position + Vector3.right * xSpeed * Time.deltaTime;
            rb.MovePosition(zMovement);
        }

        if (Input.GetKey(KeyCode.D))
        {
            float xSpeed = 1.5f;
            Vector3 zMovement = transform.position + Vector3.right * xSpeed * Time.deltaTime;
            rb.MovePosition(zMovement);
        }

        if (Input.GetKey(KeyCode.E))
        {
            float rightRotationSpeed = 100f;
            Vector3 rightRotation =  new Vector3(0, 0, rightRotationSpeed * Time.deltaTime);
            transform.Rotate(rightRotation, Space.World);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            float lefttRotationSpeed = -100f;
            Vector3 leftRotation =  new Vector3(0, 0, lefttRotationSpeed * Time.deltaTime);
            transform.Rotate(leftRotation, Space.World);
        }

        if (Input.GetKey(KeyCode.Mouse0))
        {
            float ySpeed = -1.5f;
            Vector3 yMovement = transform.position + Vector3.up * ySpeed * Time.deltaTime;
            rb.MovePosition(yMovement);
        }

        if (Input.GetKey(KeyCode.Mouse1))
        {
            float ySpeed = 1.5f;
            Vector3 yMovement = transform.position + Vector3.up * ySpeed * Time.deltaTime;
            rb.MovePosition(yMovement);
        }
    }


    private void NeedleDetector_onNeedleExit(object sender, Vector3 e)
    {
        NeedleDetector curDetector = sender as NeedleDetector;

        Quaternion direction = Quaternion.identity;

        if(curDetector.side == NeedleDetector.Side.LeftDown || curDetector.side == NeedleDetector.Side.LeftDown)
        {
            direction = Quaternion.Euler(90f, 0f, 90f);
        }

        Vector3 spawnPos = needleTop.transform.position + TorusOffset(needleBottom.position, needleTop.position);

        if(torusColliders.Count > 0 )
        {
            for (int i = 0; i < torusColliders.Count; i++)
            {
                Vector3 otherColliderPos = torusColliders[i].transform.position;

                if (Vector3.Distance(otherColliderPos, spawnPos) < minTorusDistance)
                {
                    return;
                }
            }
        }
        
        GameObject torusCollider = Instantiate(torusColliderPrefab, spawnPos, direction);

        torusColliders.Add(torusCollider);

    }

    private Vector3 TorusOffset(Vector3 needleStartPos, Vector3 needleEndPos)
    {
        if(needleStartPos.y > needleEndPos.y)
        {
            float outOffset = 0.035f;
            return Vector3.up * outOffset;
        }

        else
        {
            float inOffset = -0.075f;
            return Vector3.up * inOffset;
        }
    }
}
