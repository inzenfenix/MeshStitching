using Obi;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ThreadBehaviour : MonoBehaviour
{
    private static readonly float forcepsDistanceThreshold = 0.12f;

    private ObiRopeCursor cursor;
    private ObiRope rope;

    [SerializeField] private float ropeLengthSpeed;
    //[SerializeField] private float ropeLengthOffset;

    private List<ObiParticleAttachment> attachments;

    private void Awake()
    {
        cursor = GetComponent<ObiRopeCursor>();
        rope = GetComponent<ObiRope>();

        attachments = new List<ObiParticleAttachment>();

        //attachment = rope.solver.actors[0].AddComponent<ObiParticleAttachment>();
        //attachment.enabled = false;
    }

    private void OnEnable()
    {
        AnatomicalForcepsBehaviour.onHookedRope += AnatomicalForcepsBehaviour_onHookedRope;
        AnatomicalForcepsBehaviour.onUnhookedRope += AnatomicalForcepsBehaviour_onUnhookedRope;
    }

    private void OnDisable()
    {
        AnatomicalForcepsBehaviour.onHookedRope -= AnatomicalForcepsBehaviour_onHookedRope;
        AnatomicalForcepsBehaviour.onUnhookedRope -= AnatomicalForcepsBehaviour_onUnhookedRope;
    }

    private void AnatomicalForcepsBehaviour_onHookedRope(object sender, Transform e)
    {
        int element = 0;
        Vector3 curParticlePos = new Vector3(-1000000, -1000000, -1000000);

        //Check for the closest particle to the forceps
        for (int i = 1; i < rope.elements.Count; i++)
        {
            curParticlePos = rope.solver.positions[rope.elements[element].particle1];
            Vector3 possibleParticlePos = rope.solver.positions[rope.elements[i].particle1];

            if (Vector3.Distance(possibleParticlePos, e.position) < Vector3.Distance(curParticlePos, e.position))
            {
                element = i;
            }
        }

        //Check that the forceps are not too far from the thread
        if(Vector3.Distance(curParticlePos, e.position) > forcepsDistanceThreshold)
        {
            return;
        }

        //Create the attachment
        ObiParticleAttachment curAttachment = CheckAttachments(e);
        curAttachment.enabled = true;

        
        var particleGroup = ScriptableObject.CreateInstance<ObiParticleGroup>();
        particleGroup.particleIndices.Add(element);

        curAttachment.particleGroup = particleGroup;
        curAttachment.target = e;

        attachments.Add(curAttachment);
    }

    private ObiParticleAttachment CheckAttachments(Transform e)
    {
        if (attachments.Count > 0)
        {
            //In case we found a match
            for (int i = 0; i < attachments.Count; i++)
            {
                if (attachments[i].target == e)
                {
                    return attachments[i];
                }
            }

            //In case we didn't find a match
            return rope.solver.actors[0].AddComponent<ObiParticleAttachment>();

        }

        //If there is no current attachments added
        else
        {
            return rope.solver.actors[0].AddComponent<ObiParticleAttachment>();
        }
    }


    private void AnatomicalForcepsBehaviour_onUnhookedRope(object sender, Transform e)
    {
        if (attachments.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < attachments.Count; i++)
        {
            if (attachments[i].target == e)
            {
                attachments[i].enabled = false;
                return;
            }
        }
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
