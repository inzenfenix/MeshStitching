using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseTouchManager : MonoBehaviour
{
    public LayerMask mouseTouchPositionMask;

    public Transform nailTransform;

    private void Update()
    {
        if (Input.GetMouseButton(2))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue))
            {
                if(hit.collider.gameObject.TryGetComponent(out MedicalTool medicalTool))
                {
                    medicalTool.SelectTool();
                }
            }
        }

#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, mouseTouchPositionMask))
            {
                nailTransform.position = hit.point;
            }
        }
#endif

#if UNITY_ANDROID
        if (Input.touchCount == 0)
        {
            return;
        }

        if (Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

            if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, mouseTouchPositionMask))
            {
                nailTransform.position = hit.point;
            }
        }

#endif
    }
}
