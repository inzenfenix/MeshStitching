using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolTriggerSwitcher : MonoBehaviour
{
    [SerializeField] private Collider col;

    [SerializeField] private CutScissorsBehaviour cutScissors;
    [SerializeField] private HoldScissorsBehaviour holdScissors;

    private void OnEnable()
    {
        if (holdScissors != null)
        {
            HoldScissorsBehaviour.onHookedRope += OnScissorsTogether;
            HoldScissorsBehaviour.onUnhookedRope += OnScissorsSeparate;
        }

        if (cutScissors != null)
        {
            CutScissorsBehaviour.onCutRope += OnScissorsTogether;
            CutScissorsBehaviour.onSeparatedScissors += OnScissorsSeparate;
        }

    }

    private void OnDisable()
    {
        if (holdScissors != null)
        {
            HoldScissorsBehaviour.onHookedRope -= OnScissorsTogether;
            HoldScissorsBehaviour.onUnhookedRope -= OnScissorsSeparate;
        }

        if (cutScissors != null)
        {
            CutScissorsBehaviour.onCutRope -= OnScissorsTogether;
            CutScissorsBehaviour.onSeparatedScissors -= OnScissorsSeparate;
        }
    }

    private void OnScissorsTogether(object sender, Transform e)
    {
        col.isTrigger = false;
    }

    private void OnScissorsSeparate(object sender, Transform e)
    {
        col.isTrigger = true;
    }
}
