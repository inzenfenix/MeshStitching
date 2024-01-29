using Obi;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThreadBehaviour : MonoBehaviour
{
    private ObiRopeCursor cursor;
    private ObiRope rope;

    [SerializeField] private float ropeLengthSpeed;

    [SerializeField] private float ropeLengthOffset;

    private void Awake()
    {
        cursor = GetComponent<ObiRopeCursor>();
        rope = GetComponent<ObiRope>();
    }

    private void Start()
    {
        //cursor.ChangeLength(rope.restLength + ropeLengthOffset);
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.T))
        {
            cursor.ChangeLength(rope.restLength - ropeLengthSpeed * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.G))
        {
            cursor.ChangeLength(rope.restLength + ropeLengthSpeed * Time.deltaTime);
        }

    }
}
