using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HoldScissorsDeform))]
public class HoldScissorsBehaviour : MonoBehaviour
{
    public static event EventHandler<Transform> onHookedRope;
    public static event EventHandler<Transform> onUnhookedRope;

    [SerializeField] private Transform hookPoint;

    [SerializeField] private bool hookRope;

    private bool hookedRope;

    private void OnEnable()
    {
        HoldScissorsDeform holdScissorsDeform = GetComponent<HoldScissorsDeform>();

        holdScissorsDeform.OnScissorsJoin += HoldScissorsDeform_OnScissorsJoin;
        holdScissorsDeform.OnScissorsSeparate += HoldScissorsDeform_OnScissorsSeparate;
    }

    private void OnDisable()
    {
        HoldScissorsDeform holdScissorsDeform = GetComponent<HoldScissorsDeform>();

        holdScissorsDeform.OnScissorsJoin -= HoldScissorsDeform_OnScissorsJoin;
        holdScissorsDeform.OnScissorsSeparate -= HoldScissorsDeform_OnScissorsSeparate;
    }

    private void HoldScissorsDeform_OnScissorsSeparate(object sender, EventArgs e)
    {
        UnhookRope();
    }

    private void HoldScissorsDeform_OnScissorsJoin(object sender, EventArgs e)
    {
        HookRope();
    }

    private void Update()
    {
        if (!hookRope)
        {
            if (hookedRope)
            {
                //UnhookRope();
            }
            //return;
        }

        if (hookedRope)
        {
            //return;
        }

        //HookRope();

    }

    private void HookRope()
    {
        onHookedRope?.Invoke(this, hookPoint);
        hookedRope = true;
    }

    private void UnhookRope()
    {
        onUnhookedRope?.Invoke(this, hookPoint);
        hookedRope = false;
    }
}
