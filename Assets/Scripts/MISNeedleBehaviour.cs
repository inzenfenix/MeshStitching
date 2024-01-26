using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MISNeedleBehaviour : MonoBehaviour
{
    [SerializeField] private Transform needleTop;

    [SerializeField] private GameObject torusColliderPrefab;

    private List<GameObject> torusColliders;

    private float minTorusDistance = .15f;

    private void Start()
    {
        torusColliders = new List<GameObject>();
    }

    private void OnEnable()
    {
        NeedleDetector.onNeedleExit += NeedleDetector_onNeedleExit;
    }

    private void OnDisable()
    {
        NeedleDetector.onNeedleExit -= NeedleDetector_onNeedleExit;
    }

    private void NeedleDetector_onNeedleExit(object sender, Vector3 e)
    {
        Vector3 needleTopPos = needleTop.transform.position;

        if(torusColliders.Count > 0 )
        {
            for (int i = 0; i < torusColliders.Count; i++)
            {
                Vector3 otherColliderPos = torusColliders[i].transform.position;

                if (Vector3.Distance(otherColliderPos, needleTopPos) < minTorusDistance)
                {
                    return;
                }
            }
        }
        GameObject torusCollider = GameObject.Instantiate(torusColliderPrefab,
                                                          needleTopPos, 
                                                          Quaternion.identity);

        //torusCollider.transform.up = needleTop.right;

        torusColliders.Add(torusCollider);

    }
}
