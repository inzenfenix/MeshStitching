using Obi;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ThreadBehaviour : MonoBehaviour
{
    //The distance between the forceps and the thread to start interacting with each other
    private static readonly float forcepsDistanceThreshold = 0.12f;

    //How much should the user stretch the thread before it moves the particles
    private readonly float stitchStretchThresholdOffset = 0.0075f;

    private readonly int bodyMask = 1 << 1;

    private ObiRopeCursor cursor;
    private ObiRope rope;


    private List<ObiParticleAttachment> forcepsAttachments;
    private List<ObiParticleAttachment> stitchAttachments;

    private bool isNeedleInserted;
    private float stitchStretchThreshold;

    [SerializeField] private int inBetweenAttachmentsParticles = 10;
    [SerializeField] private int maxInBetweenAttachmentsParticles = 20;
    [SerializeField] private float ropeLengthSpeed = 1.1f;


    private void Awake()
    {
        cursor = GetComponent<ObiRopeCursor>();
        rope = GetComponent<ObiRope>();

        forcepsAttachments = new List<ObiParticleAttachment>();
        stitchAttachments = new List<ObiParticleAttachment>();
    }

    private void OnEnable()
    {
        AnatomicalForcepsBehaviour.onHookedRope += OnHookedRope;
        AnatomicalForcepsBehaviour.onUnhookedRope += OnUnhookedRope;

        HoldScissorsBehaviour.onHookedRope += OnHookedRope;
        HoldScissorsBehaviour.onUnhookedRope += OnUnhookedRope;

        NeedleDetector.onNeedleEnter += NeedleDetector_onNeedleEnter;
        NeedleDetector.onNeedleExit += NeedleDetector_onNeedleExit;
    }



    private void OnDisable()
    {
        AnatomicalForcepsBehaviour.onHookedRope -= OnHookedRope;
        AnatomicalForcepsBehaviour.onUnhookedRope -= OnUnhookedRope;

        HoldScissorsBehaviour.onHookedRope -= OnHookedRope;
        HoldScissorsBehaviour.onUnhookedRope -= OnUnhookedRope;

        NeedleDetector.onNeedleEnter -= NeedleDetector_onNeedleEnter;
        NeedleDetector.onNeedleExit -= NeedleDetector_onNeedleExit;
    }

    private void Start()
    {
        //cursor.ChangeLength(rope.restLength + ropeLengthOffset);

        //AddStitchAttachment(rope.GetParticlePosition(2));
    }

    private void NeedleDetector_onNeedleEnter(object sender, Vector3 e)
    {
        isNeedleInserted = true;
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

    //Takes care of updatinge the particles of the thread
    private void UpdateStichAttachments()
    {
        //If no stitch attachment, the length needed to unstretch the thread goes back to default
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

    private void MoveStitchParticles(bool moveLastParticles = true)
    {

        for (int i = 0; i < stitchAttachments.Count; i++)
        {
            //In case we don't want to move the last particles of the thread
            if (i == 0 && !moveLastParticles)
            {
                continue;
            }

            //We get the position of the current particle and the index of it
            Vector3 curParticlePos = stitchAttachments[i].target.position;
            int curParticle = stitchAttachments[i].particleGroup.particleIndices[0];

            //In case the current particle is the last particle
            if (curParticle >= rope.elements.Count - 1)
            {
                DestroyStitchAttachmentAt(i);
                continue;
            }

            MoveAttachedParticle(curParticle, i, curParticlePos);
        }
    }

    //We move the particles n times to the right into a certain attachment, in a certain position
    private void MoveAttachedParticle(int particle, int stitchIndex, Vector3 attachmentPos, int times = 1)
    {

        int nextParticle = particle + 1;

        for (int i = 0; i < times; i++)
        {
            //First disable the attachment so it doesn't move the old particle
            stitchAttachments[stitchIndex].enabled = false;

            Destroy(stitchAttachments[stitchIndex].particleGroup);

            var particleGroup = ScriptableObject.CreateInstance<ObiParticleGroup>();
            rope.solver.positions[nextParticle] = attachmentPos;
            particleGroup.particleIndices.Add(nextParticle);

            stitchAttachments[stitchIndex].particleGroup = particleGroup;

            nextParticle++;

            stitchAttachments[stitchIndex].enabled = true;
        }


    }

    //This funcion takes care of giving stiffness to the particles between attachments
    private void ChangeInBetweenParticlesProperties()
    {
        if (stitchAttachments.Count <= 1)
        {
            return;
        }

        for (int i = stitchAttachments.Count - 1; i > 0; i--)
        {
            //Get pair particles from attachments
            int firstParticle = stitchAttachments[i].particleGroup.particleIndices[0];
            int lastParticle = stitchAttachments[i - 1].particleGroup.particleIndices[0];

            if (Mathf.Abs(firstParticle - lastParticle) < 4)
            {
                continue;
            }

            float lerpDen = lastParticle - firstParticle;
            int lerpNum = 1;

            Vector3 firstPos = rope.GetParticlePosition(firstParticle);
            Vector3 secondPos = rope.GetParticlePosition(lastParticle);

            float minDistance = 0.16f;

            //We check that the distance between the two particles is far enough that it makes sense to stiff it
            //Also make sure that the amount of particles is not bigger than the recommended amount between attachments
            if (Vector3.Distance(firstPos, secondPos) < minDistance ||
                lerpDen > maxInBetweenAttachmentsParticles)
            {
                continue;
            }

            //We move to the particles in between the attachments
            firstParticle += 1; //One particle to the right
            lastParticle -= 1; //One particle to the left

            for (int j = firstParticle; j < lastParticle; j++)
            {
                //The final position of each particle is going to be
                //Somewhere in between the attached particles
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

    //Goes through each attached particle to check if the particle has an attachment
    private bool IsAttachedParticle(int particle)
    {
        if (stitchAttachments.Count <= 0)
        {
            return false;
        }

        for (int i = 0; i < stitchAttachments.Count; i++)
        {
            if (stitchAttachments[i].particleGroup.particleIndices[0] == particle)
            {
                return true;
            }
        }

        return false;
    }

    //Changes the properties of each particle at runtime
    private void ChangeCustomParticleProperties()
    {
        //Return particles back to normal if there's no attachments
        if (stitchAttachments.Count <= 0)
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

        //Makes the particles between attachments not collide with the body
        for (int i = 0; i < stitchAttachments[0].particleGroup.particleIndices[0]; i++)
        {

            //In the case that we are at the start or end of the thread, we want this particles to not have any
            //Physical interaction and have them always with the default properties
            if (i > rope.elements.Count - 8 || i < 5)
            {
                rope.solver.invMasses[i] = 0.1f;
                int nullMask = 0;
                rope.solver.filters[i] = ObiUtils.MakeFilter(nullMask, 10);
                continue;
            }
            rope.solver.invMasses[i] = 0.1f;

            ChangeParticleColliders(i, false);
        }

        //Makes the particles collide with everything
        for (int i = stitchAttachments[0].particleGroup.particleIndices[0]; i < rope.elements.Count; i++)
        {
            //In the case that we are at the start or end of the thread, we want this particles to not have any
            //Physical interaction and have them always with the default properties
            if (i > rope.elements.Count - 8 || i < 5)
            {
                rope.solver.invMasses[i] = 0.1f;
                int maskDefault = 0;
                rope.solver.filters[i] = ObiUtils.MakeFilter(maskDefault, 10);
                continue;
            }

            rope.solver.invMasses[i] = 0.1f;

            ChangeParticleColliders(i, true);
        }
    }

    //If bodyCollision = true, then it will add the body mask to the colliders of a particle
    private void ChangeParticleColliders(int particle, bool bodyCollision)
    {
        int mask;

        //Obi has 16 different masks, i used the mask 1 as the body mask
        if (bodyCollision)
        {
            mask = (1 << 0) | (bodyMask) | (1 << 2) | (1 << 3) |
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

    //Removes an attachment, if removeFromList = false, it will no remove it from the "stitchAttachments" list
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

    //Goes through each particle from the thread, from the first one to the last attached one to get the current
    //Length of the used thread
    private float ThreadLength()
    {
        if (stitchAttachments.Count <= 0)
        {
            return 0;
        }

        float length = 0;
        int lastParticle = stitchAttachments[0].particleGroup.particleIndices[0];
        int firstParticle = rope.elements[0].particle1;

        for (int i = firstParticle; i < lastParticle; i++)
        {
            Vector3 particle1 = rope.GetParticlePosition(i);
            Vector3 particle2 = rope.GetParticlePosition(i + 1);

            length += Vector3.Distance(particle1, particle2);
        }

        return length;
    }

    //Takes care of spawning a new attachment when the needle exits the body
    private void NeedleDetector_onNeedleExit(object sender, Vector3 e)
    {
        //In case the needle hasn't been inserted
        if (!isNeedleInserted)
        {
            return;
        }

        isNeedleInserted = false;

        NeedleDetector needleDetector = sender as NeedleDetector;

        //If the side the needle exited from is either the left or right one, we want the y position to always be the same
        if (needleDetector.side != NeedleDetector.Side.LeftDown && needleDetector.side != NeedleDetector.Side.RightDown)
        {
            e.y = .97f;
        }

        //How far each stitching has to be from each other
        float stitchThreshold = .035f;

        for (int i = 0; i < stitchAttachments.Count; i++)
        {
            if (Vector3.Distance(stitchAttachments[i].target.position, e) < stitchThreshold)
            {
                return;
            }
        }

        AddStitchAttachment(e);

        //We add at least the minimum amount of particles between attachments
        for (int i = 0; i < inBetweenAttachmentsParticles; i++)
        {
            //In the case theres not enough particles at the first attachment, we don't move more of the first particles
            bool moveLastParticles = true;

            int firstAttachmentParticle = stitchAttachments[0].particleGroup.particleIndices[0];
            int lastParticle = rope.elements[rope.elements.Count - 1].particle2;

            if (Mathf.Abs(lastParticle - (firstAttachmentParticle + 1)) <= maxInBetweenAttachmentsParticles)
            {
                moveLastParticles = false;
            }

            MoveStitchParticles(moveLastParticles);

        }

    }

    //Adds an attachment at a given position
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

    //If the forceps tried to grab something, we check for which particle (if any) was grabbed
    private void OnHookedRope(object sender, Transform forceps)
    {
        //Checks for the closest particle
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

    //Looks for the closes particle to a given position, can also specify if we don't want a particle
    //To be chosen, or if we want a certain starting position
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

    //Check if there's an attachment at given position
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

    //Disables attachment of forceps if the thread is currently being hooked by it
    private void OnUnhookedRope(object sender, Transform e)
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

    private void CutRope(Vector3 cutPosition)
    {
        for(int i = 0; i < rope.elements.Count; i++)
        {
            int particle = rope.elements[i].particle1;
            Vector3 particlePos = rope.GetParticlePosition(particle);

            float minDistance = 0.1f;

            if(Vector3.Distance(particlePos, cutPosition) < minDistance)
            {
                rope.Tear(rope.elements[i]);
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
