using Obi;
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

    private List<ObiParticleAttachment> forcepsAttachments;

    private List<ObiParticleAttachment> stitchAttachments;

    private float stitchStretchThreshold;

    private readonly float stitchStretchThresholdOffset = 0.0075f;

    [SerializeField] private int inBetweenAttachmentsParticles = 10;
    [SerializeField] private int maxInBetweenAttachmentsParticles = 20;


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

        NeedleDetector.onNeedleExit += NeedleDetector_onNeedleExit;
    }

    private void OnDisable()
    {
        AnatomicalForcepsBehaviour.onHookedRope -= AnatomicalForcepsBehaviour_onHookedRope;
        AnatomicalForcepsBehaviour.onUnhookedRope -= AnatomicalForcepsBehaviour_onUnhookedRope;

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
        ChangeInBetweenParticlesProperties();
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
        //ChangeInBetweenParticlesProperties();
    }

    private void MoveStitchParticles(bool moveLastParticles = true)
    {

        for (int i = 0; i < stitchAttachments.Count; i++)
        {
            if(i == 0 && !moveLastParticles)
            {
                continue;
            }

            Vector3 curParticlePos = stitchAttachments[i].target.position;
            int curParticle = stitchAttachments[i].particleGroup.particleIndices[0];

            if (curParticle >= rope.elements.Count - 1)
            {
                DestroyStitchAttachmentAt(i);
                continue;
            }

            rope.solver.invMasses[curParticle + 1] = 0.1f;

            MoveAttachedParticle(curParticle, i, curParticlePos);
        }

    }

    private void MoveAttachedParticle(int particle, int stitchIndex, Vector3 attachmentPos, int times = 1)
    {
        stitchAttachments[stitchIndex].enabled = false;

        int nextParticle = particle + 1;

        for (int i = 0; i < times; i++) 
        {
            Destroy(stitchAttachments[stitchIndex].particleGroup);

            var particleGroup = ScriptableObject.CreateInstance<ObiParticleGroup>();
            rope.solver.positions[nextParticle] = attachmentPos;
            particleGroup.particleIndices.Add(nextParticle);

            stitchAttachments[stitchIndex].particleGroup = particleGroup;

            nextParticle++;
        }

        stitchAttachments[stitchIndex].enabled = true;
    }

    private void CapInBetweenAttachmentParticles(int firstParticle, int lastParticle, int index)
    {
        int particleAmount = lastParticle - (firstParticle + 1);
        Vector3 pos = rope.GetParticlePosition(firstParticle);

        if(particleAmount > maxInBetweenAttachmentsParticles)
        {
            MoveAttachedParticle(firstParticle, index, pos, maxInBetweenAttachmentsParticles - particleAmount);
        }
    }

    private void ChangeInBetweenParticlesProperties()
    {
        if(stitchAttachments.Count <= 1)
        {
            return;
        }

        for(int i = stitchAttachments.Count - 1; i > 0; i--)
        {
            int firstParticle = stitchAttachments[i].particleGroup.particleIndices[0];
            int lastParticle = stitchAttachments[i - 1].particleGroup.particleIndices[0];

            if(Mathf.Abs(firstParticle - lastParticle) < 4)
            {
                continue;
            }

            float lerpDen = lastParticle - firstParticle;
            int lerpNum = 1;

            Vector3 firstPos = rope.GetParticlePosition(firstParticle);
            Vector3 secondPos = rope.GetParticlePosition(lastParticle);

            float minDistance = 0.1f;

            if (Vector3.Distance(firstPos, secondPos) < minDistance) continue;

            firstParticle += 1;
            lastParticle -= 1;

            for (int j = firstParticle; j < lastParticle; j++)
            {
                if (IsAttachedParticle(j)) continue;

                Vector3 particlePos = Vector3.Lerp(firstPos,
                                                   secondPos,
                                                   lerpNum / lerpDen);

                rope.solver.positions[j] = particlePos;
                rope.solver.velocities[j] = Vector3.zero;
                rope.solver.invMasses[j] = 0;

                lerpNum++;
            }
        }
    }

    private bool IsAttachedParticle(int particle)
    {
        if(stitchAttachments.Count <= 0)
        {
            return false;
        }

        for(int i = 0; i < stitchAttachments.Count; i++)
        {
            if (stitchAttachments[i].particleGroup.particleIndices[0] == particle)
            {
                return true;
            }
        }

        return false;
    }

    private void ChangeCustomParticleProperties()
    {
        //Return back to normal if there's no attachments
        if(stitchAttachments.Count <= 0)
        {
            for (int i = 10; i < rope.elements.Count - 10; i++)
            {
                rope.solver.invMasses[i] = 0.1f;

                int particle1 = rope.elements[i].particle1;
                int particle2 = rope.elements[i].particle2;

                ChangeParticleColliders(particle1, true);
                ChangeParticleColliders(particle2, true);
            }
            return;
        }

        //Doesn't collide with mask 1
        for (int i = 0; i < stitchAttachments[0].particleGroup.particleIndices[0]; i++)
        {
            if (i > rope.elements.Count - 8 || i < 5)
            {
                rope.solver.invMasses[i] = 0.1f;
                int nullMask = 0;
                rope.solver.filters[i] = ObiUtils.MakeFilter(nullMask, 10);
                continue;
            }
            rope.solver.invMasses[i] = 0.01f;

            ChangeParticleColliders(i, false);
        }

        //Collides with everything
        for (int i = stitchAttachments[0].particleGroup.particleIndices[0]; i < rope.elements.Count; i++)
        {
            if(i > rope.elements.Count - 8 || i < 5)
            {
                rope.solver.invMasses[i] = 0.1f;
                int maskDefault = 0;
                rope.solver.filters[i] = ObiUtils.MakeFilter(maskDefault, 10);
                continue;
            }

            rope.solver.invMasses[i] = 1.0f;

            ChangeParticleColliders(i, true);
        }
    }

    private void ChangeParticleColliders(int particle, bool bodyCollision)
    {
        int mask;

        if(bodyCollision)
        {
            mask = (1 << 0) | (1 << 1) | (1 << 2) | (1 << 3) |
                   (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) |
                   (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11) |
                   (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);
        }

        else
        {
            mask = (1 << 0) | (1 << 2) | (1 << 3) |
                   (1 << 4) | (1 << 5) | (1 << 6) | (1 << 7) |
                   (1 << 8) | (1 << 9) | (1 << 10) | (1 << 11) |
                   (1 << 12) | (1 << 13) | (1 << 14) | (1 << 15);
        }

        rope.solver.filters[particle] = ObiUtils.MakeFilter(mask, 10);
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
        NeedleDetector needleDetector = sender as NeedleDetector;

        if (needleDetector.side != NeedleDetector.Side.LeftDown && needleDetector.side != NeedleDetector.Side.RightDown)
        {
            e.y = .97f;
        }

        float stitchThreshold = .035f;

        for (int i = 0; i < stitchAttachments.Count; i++)
        {
            if (Vector3.Distance(stitchAttachments[i].target.position, e) < stitchThreshold)
            {
                return;
            }    
        }
        
        AddStitchAttachment(e);

        for(int i = 0; i < inBetweenAttachmentsParticles; i++)
        {
            bool moveLastParticles = true;
            
            int firstAttachmentParticle = stitchAttachments[0].particleGroup.particleIndices[0];
            int lastParticle = rope.elements[rope.elements.Count - 1].particle2;

            if (lastParticle - (firstAttachmentParticle + 1) <= maxInBetweenAttachmentsParticles)
            {
                moveLastParticles = false;
            }

            MoveStitchParticles(moveLastParticles);
        }
        
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

    private void AnatomicalForcepsBehaviour_onHookedRope(object sender, Transform forceps)
    {
        int element = ClosestParticle(forceps.position);
        Vector3 curParticlePos = rope.solver.positions[rope.elements[element].particle1];

        //Check that the forceps are not too far from the thread
        if (Vector3.Distance(curParticlePos, forceps.position) > forcepsDistanceThreshold)
        {
            return;
        }

        //Create the attachment
        ObiParticleAttachment curAttachment = CheckAttachments(forceps, forcepsAttachments);
        curAttachment.enabled = true;


        var particleGroup = ScriptableObject.CreateInstance<ObiParticleGroup>();

        particleGroup.particleIndices.Add(rope.elements[element].particle1);

        curAttachment.particleGroup = particleGroup;
        curAttachment.target = forceps;

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
