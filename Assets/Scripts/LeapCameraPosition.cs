using Leap.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraPosition
{
    Desktop,
    TopDown,
    HMD
};

[RequireComponent(typeof(LeapServiceProvider))]
public class LeapCameraPosition : MonoBehaviour
{

    public CameraPosition cameraPosition;
    
    private Vector3 desktopPosition  = new Vector3(0.267f, 0.509f, 1.116f);
    private Vector3 desktopRotation = new Vector3(0f, 90f, 0f);

    private Vector3 topDownPosition = new Vector3(0.246f, 1.816f, 1.116f);
    private Vector3 topDownRotation = new Vector3(180, -90, 0);

    private Vector3 HMDPosition = new Vector3(-0.375f, 1.174f, 1.116f);
    private Vector3 HMDRotation = new Vector3(-90, -90, 0);

    private LeapServiceProvider leapServiceProvider;

    private void Start()
    {
        leapServiceProvider = GetComponent<LeapServiceProvider>();

        if (cameraPosition == CameraPosition.Desktop)
        {
            this.transform.position = desktopPosition;
            this.transform.rotation = Quaternion.Euler(desktopRotation);

            leapServiceProvider.ChangeTrackingMode(LeapServiceProvider.TrackingOptimizationMode.Desktop);
        }

        else if (cameraPosition == CameraPosition.TopDown)
        {
            this.transform.position = topDownPosition;
            this.transform.rotation = Quaternion.Euler(topDownRotation);

            leapServiceProvider.ChangeTrackingMode(LeapServiceProvider.TrackingOptimizationMode.Screentop);
        }

        else if (cameraPosition == CameraPosition.HMD)
        {
            this.transform.position = HMDPosition;
            this.transform.rotation = Quaternion.Euler(HMDRotation);

            leapServiceProvider.ChangeTrackingMode(LeapServiceProvider.TrackingOptimizationMode.HMD);
        }
    }
}
