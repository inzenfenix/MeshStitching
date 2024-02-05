using Obi;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class ThreadBehaviour : MonoBehaviour
{
    private static readonly float forcepsDistanceThreshold = 0.12f;

    private ObiRopeCursor cursor;
    private ObiRope rope;

    [SerializeField] private float ropeLengthSpeed;
    //[SerializeField] private float ropeLengthOffset;

    private List<ObiParticleAttachment> forcepsAttachments;

    private List<ObiParticleAttachment> stitchAttachments;

    private float stitchStretchThreshold;

    private readonly float stitchStretchThresholdOffset = 0.0075f;

    private Vector3 entrancePoint;
    private Vector3 exitPoint;


    private void Awake()
    {
        cursor = GetComponent<ObiRopeCursor>();
        rope = GetComponent<ObiRope>();

        forcepsAttachments = new List<ObiParticleAttachment>();
        stitchAttachments = new List<ObiParticleAttachment>();
    }

    private void OnEnable()
    {
        AnatomicalForcepsBehaviour.onHookedRope += AnatomicalForcepsBehaviour_onHookedRope;
        AnatomicalForcepsBehaviour.onUnhookedRope += AnatomicalForcepsBehaviour_onUnhookedRope;

        NeedleDetector.onNeedleEnter += NeedleDetector_onNeedleEnter;
        NeedleDetector.onNeedleExit += NeedleDetector_onNeedleExit;
    }

    private void NeedleDetector_onNeedleEnter(object sender, Vector3 e)
    {
        entrancePoint = e;
    }

    private void OnDisable()
    {
        AnatomicalForcepsBehaviour.onHookedRope -= AnatomicalForcepsBehaviour_onHookedRope;
        AnatomicalForcepsBehaviour.onUnhookedRope -= AnatomicalForcepsBehaviour_onUnhookedRope;

        NeedleDetector.onNeedleExit -= NeedleDetector_onNeedleExit;
        NeedleDetector.onNeedleExit -= NeedleDetector_onNeedleExit;
    }

    private void Start()
    {
        //cursor.ChangeLength(rope.restLength + ropeLengthOffset);

        //AddStitchAttachment(rope.GetParticlePosition(2));
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

        UpdateStichAttachments();
        //CustomParticlesVelocity();
    }

    private void UpdateStichAttachments()
    {

        if (stitchAttachments.Count <= 0)
        {
            stitchStretchThreshold = ThreadLength() + stitchStretchThresholdOffset;
            return;
        }

        if (ThreadLength() < stitchStretchThreshold)
        {
            return;
        }

        stitchStretchThreshold = ThreadLength() + stitchStretchThresholdOffset;

        MoveStitchParticles();
        ChangeCustomParticleProperties();
    }

    private void MoveStitchParticles()
    {

        for (int i = 0; i < stitchAttachments.Count; i++)
        {
            Vector3 curParticlePos = stitchAttachments[i].target.position;
            int curParticle = stitchAttachments[i].particleGroup.particleIndices[0];

            if (curParticle >= rope.elements.Count - 2)
            {
                DestroyStitchAttachmentAt(i);
                continue;
            }

            int nextParticle = curParticle + 1;

            stitchAttachments[i].enabled = false;

            Destroy(stitchAttachments[i].particleGroup);

            var particleGroup = ScriptableObject.CreateInstance<ObiParticleGroup>();
            rope.solver.positions[nextParticle] = curParticlePos;
            particleGroup.particleIndices.Add(nextParticle);

            stitchAttachments[i].particleGroup = particleGroup;
            stitchAttachments[i].enabled = true;
        }

    }

    private void CustomParticlesVelocity()
    {
        if(stitchAttachments.Count <= 0)
        {
            return;
        }

        for(int i = 0; i < stitchAttachments[0].particleGroup.particleIndices[0]; i++)
        {
            rope.solver.velocities[i] = Vector3.zero;
        }
    }

    private void ChangeCustomParticleProperties()
    {
        if(stitchAttachments.Count <= 0)
        {
            return;
        }

        //Doesn't collide with mask one
        for (int i = 0; i < stitchAttachments[0].particleGroup.particleIndices[0]; i++)
        {
            if (i > rope.elements.Count - 10)
            {
                rope.solver.invMasses[i] = 1.0f;
                int maskDefault = 0;
                rope.solver.filters[i] = ObiUtils.MakeFilter(maskDefault, 10);
                return;
            }

            rope.solver.invMasses[i] = 0.01f;

            int mask = (1 << 0) | (1 << 2) | (1 << 3) | (1 << 4) |
                       (1 << 5) | (1 << 6) | (1 << 7) | (1 << 8) |
                       (1 << 9) | (1 << 10) | (1 << 11) | (1 << 12) |
                       (1 << 13) | (1 << 14) | (1 << 15);
            rope.solver.filters[i] = ObiUtils.MakeFilter(mask, 10);
        }

        //Collides with everything
        for (int i = stitchAttachments[0].particleGroup.particleIndices[0]; i < rope.elements.Count; i++)
        {
            if(i > rope.elements.Count - 10)
            {
                rope.solver.invMasses[i] = 1.0f;
                int maskDefault = 0;
                rope.solver.filters[i] = ObiUtils.MakeFilter(maskDefault, 10);
                return;
            }

            rope.solver.invMasses[i] = 1.0f;

            int mask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) |
                       (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) |
                       (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11) |
                       (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);
            rope.solver.filters[i] = ObiUtils.MakeFilter(mask, 10);
        }
    }

    private void DestroyStitchAttachmentAt(int index, bool removeFromList = true)
    {
        stitchAttachments[index].enabled = false;

        Destroy(stitchAttachments[index].particleGroup);
        Destroy(stitchAttachments[index].target.gameObject);

        ObiParticleAttachment attachment = stitchAttachments[index];
        Destroy(attachment);

        if (removeFromList)
        {
            stitchAttachments.RemoveAt(index);
        }
    }

    private float ThreadLength()
    {
        if (stitchAttachments.Count <= 0)
        {
            return 0;
        }

        float length = 0;
        float lastParticle = stitchAttachments[0].particleGroup.particleIndices[0];

        for (int i = 0; i < lastParticle; i++)
        {
            Vector3 firstParticle = rope.GetParticlePosition(i);
            Vector3 secondParticle = rope.GetParticlePosition(i + 1);

            length += Vector3.Distance(firstParticle, secondParticle);
        }

        return length;
    }

    private void NeedleDetector_onNeedleExit(object sender, Vector3 e)
    {
        exitPoint = e;
        NeedleDetector needleDetector = sender as NeedleDetector;

        if (needleDetector.side != NeedleDetector.Side.LeftDown && needleDetector.side != NeedleDetector.Side.RightDown)
        {
            if (exitPoint.y > entrancePoint.y)
            {
                float exitOffset = 0.025f;
                e -= Vector3.up * exitOffset;
            }

            else
            {
                float entranceOffset = 0.075f;
                e += Vector3.up * entranceOffset;
            }
        }

        float stitchThreshold = .05f;

        for (int i = 0; i < stitchAttachments.Count; i++)
        {
            if (Vector3.Distance(stitchAttachments[i].target.position, e) < stitchThreshold)
            {
                return;
            }    
        }
        
        AddStitchAttachment(e);
        MoveStitchParticles();
    }

    private void AddStitchAttachment(Vector3 stitchPos)
    {
        GameObject spawnObject = new GameObject();
        Transform spawnPoint = spawnObject.transform;
        spawnPoint.position = stitchPos;

        ObiParticleAttachment curAttachment = CheckAttachments(spawnPoint, stitchAttachments);
        curAttachment.enabled = true;

        int particle = 1;

        var particleGroup = ScriptableObject.CreateInstance<ObiParticleGroup>();
        rope.solver.positions[particle] = spawnPoint.position;

        particleGroup.particleIndices.Add(particle);

        curAttachment.particleGroup = particleGroup;
        curAttachment.target = spawnPoint;

        stitchAttachments.Add(curAttachment);
    }

    private void AnatomicalForcepsBehaviour_onHookedRope(object sender, Transform e)
    {
        int element = ClosestParticle(e.position);
        Vector3 curParticlePos = rope.solver.positions[rope.elements[element].particle1];

        //Check that the forceps are not too far from the thread
        if (Vector3.Distance(curParticlePos, e.position) > forcepsDistanceThreshold)
        {
            return;
        }

        //Create the attachment
        ObiParticleAttachment curAttachment = CheckAttachments(e, forcepsAttachments);
        curAttachment.enabled = true;


        var particleGroup = ScriptableObject.CreateInstance<ObiParticleGroup>();

        particleGroup.particleIndices.Add(rope.elements[element].particle1);

        curAttachment.particleGroup = particleGroup;
        curAttachment.target = e;

        forcepsAttachments.Add(curAttachment);
    }

    private int ClosestParticle(Vector3 pos, int curElement = -1, int startPos = 0)
    {
        int element = startPos;
        Vector3 curParticlePos = rope.solver.positions[rope.elements[element].particle1];

        for (int i = startPos + 1; i < rope.elements.Count; i++)
        {
            if (i == curElement)
            {
                continue;
            }

            Vector3 possibleParticlePos = rope.solver.positions[rope.elements[i].particle1];

            if (Vector3.Distance(possibleParticlePos, pos) < Vector3.Distance(curParticlePos, pos))
            {
                curParticlePos = possibleParticlePos;
                element = i;
            }
        }

        return element;
    }

    private ObiParticleAttachment CheckAttachments(Transform e, List<ObiParticleAttachment> attachments)
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
        }

        //If there is no current attachments added, or no match was found
        return rope.solver.actors[0].AddComponent<ObiParticleAttachment>();
    }


    private void AnatomicalForcepsBehaviour_onUnhookedRope(object sender, Transform e)
    {
        if (forcepsAttachments.Count <= 0)
        {
            return;
        }

        for (int i = 0; i < forcepsAttachments.Count; i++)
        {
            if (forcepsAttachments[i].target == e)
            {
                forcepsAttachments[i].enabled = false;
                return;
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(rope == null)
        {
            return;
        }

        for(int i = 0; i < rope.elements.Count; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(rope.GetParticlePosition(rope.elements[i].particle1), 0.005f);
        }
    }
}
