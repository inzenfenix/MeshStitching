using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoldScissorsBehaviour : MonoBehaviour
{
    public static event EventHandler<Transform> onHookedRope;
    public static event EventHandler<Transform> onUnhookedRope;

    [SerializeField] private Transform hookPoint;

    [SerializeField] private bool hookRope;

    private bool hookedRope;

    private void Update()
    {
        if (!hookRope)
        {
            if (hookedRope)
            {
                UnhookRope();
            }
            return;
        }

        if (hookedRope)
        {
            return;
        }

        HookRope();

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
