using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CutScissorsDeform))]
public class CutScissorsBehaviour : MonoBehaviour
{
    public static event EventHandler<Transform> onCutRope;
    public static event EventHandler<Transform> onSeparatedScissors;

    [SerializeField] private Transform hookPoint;

    [SerializeField] private bool hookRope;

    private void OnEnable()
    {
        CutScissorsDeform holdScissorsDeform = GetComponent<CutScissorsDeform>();

        holdScissorsDeform.OnScissorsJoin += HoldScissorsDeform_OnScissorsJoin;
        holdScissorsDeform.OnScissorsSeparate += HoldScissorsDeform_OnScissorsSeparate;
    }

    private void OnDisable()
    {
        CutScissorsDeform holdScissorsDeform = GetComponent<CutScissorsDeform>();

        holdScissorsDeform.OnScissorsJoin -= HoldScissorsDeform_OnScissorsJoin;
        holdScissorsDeform.OnScissorsSeparate -= HoldScissorsDeform_OnScissorsSeparate;
    }

    private void HoldScissorsDeform_OnScissorsJoin(object sender, EventArgs e)
    {
        CutRope();
    }

    private void CutRope()
    {
        onCutRope?.Invoke(this, hookPoint);
    }

    private void HoldScissorsDeform_OnScissorsSeparate(object sender, EventArgs e)
    {
        onSeparatedScissors?.Invoke(this, hookPoint);
    }
}
