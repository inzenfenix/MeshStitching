using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ToolTriggerSwitcher : MonoBehaviour
{
    /*
     In case we want a collider inside of a tool, if, for example, we want
     to have collisions with the thread when the tool is closed.

    Specially useful to make knots with threads using scissors.
    */


    [SerializeField] private CutScissorsBehaviour cutScissors;
    [SerializeField] private HoldScissorsBehaviour holdScissors;

    private Collider collider;

    private void Start()
    {
        collider = GetComponent<Collider>();
    }

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
        collider.isTrigger = !collider.isTrigger;
    }

    private void OnScissorsSeparate(object sender, Transform e)
    {
        collider.isTrigger = !collider.isTrigger;
    }
}
