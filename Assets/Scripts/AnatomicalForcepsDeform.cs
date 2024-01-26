using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnatomicalForcepsDeform : MonoBehaviour
{
    [Header("TOP PART OF THE FORCEPS")]
    [SerializeField] private Transform TopBone;

    public Vector3 originalTopRotation;
    public Vector3 deformedTopRotation;

    [Range(0f, 1f)]
    public float topKey;

    [Header("\nBOTTOM PART OF THE FORCEPS")]
    [SerializeField]private Transform BottomBone;

    public Vector3 originalBottomRotation;
    public Vector3 deformedBottomRotation;

    [Range(0f, 1f)]
    public float bottomKey;

    private Quaternion originalTopRotation_Q;
    private Quaternion deformedlTopRotation_Q;

    private Quaternion originalBottomRotation_Q;
    private Quaternion deformedlBottomRotation_Q;

    private void Start()
    {
        originalTopRotation_Q = Quaternion.Euler(originalTopRotation);
        deformedlTopRotation_Q = Quaternion.Euler(deformedTopRotation);

        originalBottomRotation_Q = Quaternion.Euler(originalBottomRotation);
        deformedlBottomRotation_Q = Quaternion.Euler(deformedBottomRotation);
    }

    private void Update()
    {

        TopBone.localRotation = Quaternion.Slerp(originalTopRotation_Q, deformedlTopRotation_Q, topKey);
        BottomBone.localRotation = Quaternion.Slerp(originalBottomRotation_Q, deformedlBottomRotation_Q, bottomKey);
    }

}
