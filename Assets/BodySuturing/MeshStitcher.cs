using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;


 //Object containing the data for the deformation of vertices
[System.Serializable]
public class DeformableVertices
{
    public Vector3 defaultLeftVertexPos;
    public Vector3 curLeftVertexPos;
    public int leftVertexIndex;

    public Vector3 defaultRightVertexPos;
    public Vector3 curRightVertexPos;
    public int rightVertexIndex;

    public Vector3 middlePointLeftPos;
    public Vector3 middlePointRightPos;

    public int clothLeftIndex;
    public int clothRightIndex;

    //Normal mesh version
    public DeformableVertices(Vector3 leftVertexPos, int leftVertexIndex, Vector3 rightVertexPos, int rightVertexIndex,
                              Vector3 midPointLeft, Vector3 midPoinRight)
    {
        defaultLeftVertexPos = curLeftVertexPos = leftVertexPos;
        this.leftVertexIndex = leftVertexIndex;

        defaultRightVertexPos = curRightVertexPos = rightVertexPos;
        this.rightVertexIndex = rightVertexIndex;

        middlePointLeftPos = midPointLeft;
        middlePointRightPos = midPoinRight;
    }

    //Cloth mesh version
    public DeformableVertices(Vector3 leftVertexPos, int leftVertexIndex, Vector3 rightVertexPos, int rightVertexIndex,
                              Vector3 midPointLeft, Vector3 midPoinRight, int clothLeftIndex = 0, int clothRightIndex = 0)
    {
        defaultLeftVertexPos = curLeftVertexPos = leftVertexPos;
        this.leftVertexIndex = leftVertexIndex;

        defaultRightVertexPos = curRightVertexPos = rightVertexPos;
        this.rightVertexIndex = rightVertexIndex;

        middlePointLeftPos = midPointLeft;
        middlePointRightPos = midPoinRight;

        this.clothLeftIndex = clothLeftIndex;
        this.clothRightIndex = clothRightIndex;
    }
}

//Struct with they keys for each vertex of the system
[System.Serializable]
public struct VertexDeformationKeys
{
    [Range(0f, 1f)]
    public float leftVertexKey;

    [Range(0f, 1f)]
    public float rightVertexKey;

    //Weights that the needle puts on the vertices after each insertion
    [HideInInspector]
    public float insertionWeightLeftVertex;
    [HideInInspector]
    public float insertionWeightRightVertex;
}

public class MeshStitcher : MonoBehaviour
{
    [SerializeField] private MeshFilter meshLeft;
    //[SerializeField] private SkinnedMeshRenderer skinnedMeshRendererLeft;

    [SerializeField] private MeshFilter meshRight;
    //[SerializeField] private SkinnedMeshRenderer skinnedMeshRendererRight;

    [SerializeField] private float minVertexDistance;
    [SerializeField] private float maxVertexDistance;

    private List<DeformableVertices> deformableVertices;

    [SerializeField] private VertexDeformationKeys[] vertexKeys;

    private VertexDeformationKeys[] defaultVertexKeys;

    //[SerializeField] private Cloth leftCloth;
    //[SerializeField] private Cloth rightCloth;

    //Suture system
    private int insertionTimes;
    private List<NeedleDetector> insertionTriggers;

    private void Awake()
    {
        //I'll leave the old code that works with cloth physics
        //Just in case it could be used later on

        //Create an instance of the mesh for each part of the body, that way we don't accidentally override original model
        //skinnedMeshRendererLeft.sharedMesh = meshLeft.mesh;

        //skinnedMeshRendererRight.sharedMesh = meshRight.mesh;

        deformableVertices = new List<DeformableVertices>();
        insertionTriggers = new List<NeedleDetector>();

        for (int i = 0; i < meshLeft.mesh.vertices.Length; i++)
        {
            //Transform vertex position from local space to world space to measure the distance
            Vector3 leftVertexPos = meshLeft.transform.TransformPoint(meshLeft.mesh.vertices[i]);

            //Variables to get the vertex that is at the minimum possible distance from the current vertex
            float minDistance = float.MaxValue;
            Vector3 minRightVertexPos = Vector3.zero;
            int rightVertexIndex = 0;

            for (int j = 0; j < meshRight.mesh.vertices.Length; j++)
            {
                Vector3 rightVertexPos = meshRight.transform.TransformPoint(meshRight.mesh.vertices[j]);

                float vertexDistance = Vector3.Distance(leftVertexPos, rightVertexPos);

                if (vertexDistance > minVertexDistance && vertexDistance < maxVertexDistance && vertexDistance < minDistance)
                {
                    minDistance = vertexDistance;
                    minRightVertexPos = rightVertexPos;
                    rightVertexIndex = j;
                }
            }

            //Checking for duplicates makes it possible to use different type of meshes besides simple planes
            if (minRightVertexPos == Vector3.zero ||
                CheckForDuplicates(meshLeft.transform.InverseTransformPoint(leftVertexPos),
                                   meshRight.transform.InverseTransformPoint(minRightVertexPos)))
            {
                continue;
            }

            Vector3 middlePoint = MidPointVertex(leftVertexPos, minRightVertexPos);

            //Using inverse transform point we get where the point is in the local space of each mesh/game object
            Vector3 middlePointLeft = meshLeft.transform.InverseTransformPoint(middlePoint);
            Vector3 middlePointRight = meshRight.transform.InverseTransformPoint(middlePoint);

            leftVertexPos = meshLeft.transform.InverseTransformPoint(leftVertexPos);
            minRightVertexPos = meshRight.transform.InverseTransformPoint(minRightVertexPos);

            //int leftClothIndex = FindClothVertices(true, leftVertexPos);
            //int rightClothIndex = FindClothVertices(false, minRightVertexPos);

            DeformableVertices vertices = new DeformableVertices(leftVertexPos, i, minRightVertexPos, rightVertexIndex,
                                                                 middlePointLeft, middlePointRight);

            deformableVertices.Add(vertices);

        }

        vertexKeys = new VertexDeformationKeys[deformableVertices.Count];
        defaultVertexKeys = new VertexDeformationKeys[deformableVertices.Count];

        for(int i = 0; i < vertexKeys.Length; i++)
        {
            vertexKeys[i].leftVertexKey = defaultVertexKeys[i].leftVertexKey = 0.0f;
            vertexKeys[i].rightVertexKey = defaultVertexKeys[i].rightVertexKey = 0.0f;
        }
    }

    private void OnEnable()
    {
        NeedleDetector.onNeedleEnter += NeedleDetector_onNeedleEnter;
    }

    private void OnDisable()
    {
        NeedleDetector.onNeedleEnter -= NeedleDetector_onNeedleEnter;
    }

    private void NeedleDetector_onNeedleEnter(object sender, Vector3 insertionPoint)
    {
        //Currently using a simple system that checks if the four parts of the body where touched by the needle
        //This is test code
        NeedleDetector curDetector = sender as NeedleDetector;

        if(insertionTriggers.Contains(curDetector))
        {
            return;
        }

        insertionTriggers.Add(curDetector);
        insertionTimes++;

        AddWeights(insertionPoint, curDetector.side);

        //This part will stitch both vertices, later we could change it so that it works with the thread instead
        if(insertionTimes == 4)
        {
            StitchMeshes();
            insertionTriggers = new List<NeedleDetector>();
            insertionTimes = 0;

            for(int i = 0; i < vertexKeys.Length;i++) 
            {
                vertexKeys[i].insertionWeightLeftVertex = 0.0f;
                vertexKeys[i].insertionWeightRightVertex = 0.0f;
            }
        }
    }

    private void AddWeights(Vector3 insertionPoint, NeedleDetector.Side side)
    {
        for(int i = 0; i < vertexKeys.Length; i++)
        {
            if(side == NeedleDetector.Side.Left || side == NeedleDetector.Side.LeftDown)
            {
                Vector3 vertexPosWorldSpace = meshLeft.transform.TransformPoint(deformableVertices[i].curLeftVertexPos);
                float distance = Vector3.Distance(vertexPosWorldSpace, insertionPoint);

                if(distance < 0.005f && distance > .6f) 
                {
                    return;
                }

                float weight = .05f / distance;
                vertexKeys[i].insertionWeightLeftVertex += Mathf.Clamp(weight, 0, 1);
                vertexKeys[i].insertionWeightLeftVertex = Mathf.Clamp(vertexKeys[i].insertionWeightLeftVertex, 0, 1);
            }

            else if(side == NeedleDetector.Side.Right || side == NeedleDetector.Side.RightDown)
            {
                Vector3 vertexPosWorldSpace = meshRight.transform.TransformPoint(deformableVertices[i].curRightVertexPos);
                float distance = Vector3.Distance(vertexPosWorldSpace, insertionPoint);

                //This changes how many vertices gets affected from the weight of the current suturing
                if (distance < 0.009f && distance > .5f)
                {
                    return;
                }

                // At a higher distance, the weight is smaller
                float weight = .05f / distance;
                vertexKeys[i].insertionWeightRightVertex += Mathf.Clamp(weight, 0, 1);
                vertexKeys[i].insertionWeightRightVertex = Mathf.Clamp(vertexKeys[i].insertionWeightRightVertex, 0, 1);
            }
        }
    }

    private void StitchMeshes()
    {
        for(int i = 0; i < vertexKeys.Length; i++)
        {
            vertexKeys[i].leftVertexKey = Mathf.Lerp(vertexKeys[i].leftVertexKey, 1, vertexKeys[i].insertionWeightLeftVertex);
            vertexKeys[i].rightVertexKey = Mathf.Lerp(vertexKeys[i].rightVertexKey, 1, vertexKeys[i].insertionWeightRightVertex);
        }
    }

    private void Update()
    {


        //skinnedMeshRendererLeft.sharedMesh = meshLeft.mesh;
        //skinnedMeshRendererRight.sharedMesh = meshRight.mesh;

        //Check for changes in the vertex keys so that it doesn't update the mesh every frame
        for (int i = 0; i < vertexKeys.Length; i++)
        {

            if (vertexKeys[i].leftVertexKey != defaultVertexKeys[i].leftVertexKey ||
                vertexKeys[i].rightVertexKey != defaultVertexKeys[i].rightVertexKey)
            {
                //Changes the current default position of the vertices to the current one
                defaultVertexKeys[i].leftVertexKey = vertexKeys[i].leftVertexKey;
                defaultVertexKeys[i].rightVertexKey = vertexKeys[i].rightVertexKey;

                Vector3[] verticesLeft = meshLeft.mesh.vertices;
                Vector3[] verticesRight = meshRight.mesh.vertices;

                //Lerp each vertex closer to the space
                deformableVertices[i].curLeftVertexPos = Vector3.Lerp(deformableVertices[i].defaultLeftVertexPos,
                                                                      deformableVertices[i].middlePointLeftPos, vertexKeys[i].leftVertexKey);

                deformableVertices[i].curRightVertexPos = Vector3.Lerp(deformableVertices[i].defaultRightVertexPos,
                                                                      deformableVertices[i].middlePointRightPos, vertexKeys[i].rightVertexKey);

                verticesLeft[deformableVertices[i].leftVertexIndex] = deformableVertices[i].curLeftVertexPos;
                verticesRight[deformableVertices[i].rightVertexIndex] = deformableVertices[i].curRightVertexPos;

                meshLeft.mesh.vertices = verticesLeft;
                meshRight.mesh.vertices = verticesRight;
            }
        }

    }

    private void OnDrawGizmos()
    {
        if (deformableVertices == null)
        {
            return;
        }

        Gizmos.color = Color.black;

        for (int i = 0; i < deformableVertices.Count; i++)
        {
            Vector3 worldSpaceLeftVertex = meshLeft.transform.TransformPoint(deformableVertices[i].curLeftVertexPos);
            Vector3 worldSpaceRightVertex = meshRight.transform.TransformPoint(deformableVertices[i].curRightVertexPos);

            Gizmos.DrawSphere(worldSpaceLeftVertex, .01f);
            Gizmos.DrawSphere(worldSpaceRightVertex, .01f);
        }
    }

    //To get the center between a pair
    private Vector3 MidPointVertex(Vector3 leftVertexPos, Vector3 rightVertexPos)
    {
        float x = (leftVertexPos.x + rightVertexPos.x) / 2;
        float y = (leftVertexPos.y + rightVertexPos.y) / 2;
        float z = (leftVertexPos.z + rightVertexPos.z) / 2;

        return new Vector3(x, y, z);
    }

    //Checks that no vertex has more than one pair
    private bool CheckForDuplicates(Vector3 leftPos, Vector3 rightPos)
    {
        for (int i = 0; i < deformableVertices.Count; i++)
        {
            if (deformableVertices[i].defaultLeftVertexPos == leftPos ||
                deformableVertices[i].defaultRightVertexPos == rightPos)
            {
                return true;
            }
        }

        return false;
    }

    /*private int FindClothVertices(bool isLeftSide, Vector3 pos)
    {
        if (isLeftSide)
        {
            for (int i = 0; i < leftCloth.vertices.Length; i++)
            {
                if (leftCloth.vertices[i] == pos)
                {
                    return i;
                }
            }
        }

        else
        {
            for (int i = 0; i < rightCloth.vertices.Length; i++)
            {
                if (rightCloth.vertices[i] == pos)
                {
                    return i;
                }
            }
        }

        return -1;
    }*/
}
