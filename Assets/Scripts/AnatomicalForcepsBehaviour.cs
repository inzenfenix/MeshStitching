using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AnatomicalForcepsDeform))]
public class AnatomicalForcepsBehaviour : MonoBehaviour
{
    public static event EventHandler<Transform> onHookedRope;
    public static event EventHandler<Transform> onUnhookedRope;

    [SerializeField] private Transform hookPoint;

    private AnatomicalForcepsDeform anatomicalForcepsDeform;

    private void OnEnable()
    {
        anatomicalForcepsDeform = GetComponent<AnatomicalForcepsDeform>();

        anatomicalForcepsDeform.OnForcepsJoin += AnatomicalForcepsBehaviour_OnForcepsJoin;
        anatomicalForcepsDeform.OnForcepsSeparate += AnatomicalForcepsDeform_OnForcepsSeparate;
    }

    private void OnDisable()
    {
        anatomicalForcepsDeform.OnForcepsJoin -= AnatomicalForcepsBehaviour_OnForcepsJoin;
        anatomicalForcepsDeform.OnForcepsSeparate -= AnatomicalForcepsDeform_OnForcepsSeparate;
    }

    private void AnatomicalForcepsDeform_OnForcepsSeparate(object sender, EventArgs e)
    {
        UnhookRope();
    }

    private void AnatomicalForcepsBehaviour_OnForcepsJoin(object sender, EventArgs e)
    {
        HookRope();
    }

    private void HookRope()
    {
        onHookedRope?.Invoke(this, hookPoint);
    }

    private void UnhookRope()
    {
        onUnhookedRope?.Invoke(this, hookPoint);
    }

}
