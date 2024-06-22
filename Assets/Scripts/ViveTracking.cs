using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using TMPro;

public class ViveTracking : MonoBehaviour
{
    //public TMP_Text ctrl1;
    //public TMP_Text ctrl2;

    public Transform t1;
    public Transform t2;

    [StructLayout(LayoutKind.Sequential)]
    public struct Pose
    {
        public float x, y, z;
        public float cw, cx, cy, cz;
    }

    const string dllLocation = @"ViveTracker.dll";

    [DllImport(dllLocation, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool Init();

    [DllImport(dllLocation, CallingConvention = CallingConvention.Cdecl)]
    public static extern void Shutdown();

    [DllImport(dllLocation, CallingConvention = CallingConvention.Cdecl)]
    public static extern bool GetControllerPose(uint deviceIndex, out Pose pose);

    [DllImport(dllLocation, CallingConvention = CallingConvention.Cdecl)]
    public static extern void UpdateTracker();


    bool isVRInitialized = false;

    void Start()
    {
        if (!Init())
        {
            Debug.LogError("Failed to initialize VR system");
            return;
        }
        isVRInitialized = true;
    }

    void Update()
    {

        Pose[] pose = new Pose[2];
        string[] poseStr = new string[2];

        if (isVRInitialized)
            for (uint ctr = 0; ctr < 2; ctr++)
            {
                GetControllerPose(ctr + 1, out pose[ctr]);
                poseStr[ctr] = "X = " + pose[ctr].x + "|" + " Y = " + pose[ctr].y + "|" + " Z = " + pose[ctr].z;// + "|" + " AX = " + pose[ctr].pitch * Mathf.Rad2Deg + "|" + " AY = " + pose[ctr].yaw * Mathf.Rad2Deg + "|" + " AZ = " + pose[ctr].roll * Mathf.Rad2Deg;
            }

        //ctrl1.text = poseStr[0].ToString();
        //ctrl2.text = poseStr[1].ToString();

        //control 1

        Vector3 curPos1 = new Vector3(pose[0].x, pose[0].y, -pose[0].z);
        if(curPos1 != Vector3.zero)
        {
            t1.position = this.transform.position + curPos1;
            Quaternion prerot = Quaternion.Euler(0, -90, 0); //Prerotaci�n
            Quaternion rotTracker = new Quaternion(-pose[0].cx, -pose[0].cy, pose[0].cz, pose[0].cw);
            //t1.rotation = prerot * rotTracker * this.transform.rotation;
        }


        //control 2

        Vector3 curPos2 = new Vector3(pose[1].x, pose[1].y, -pose[1].z);
        if(curPos2 != Vector3.zero)
        {
            t2.position = this.transform.position + curPos2;
            Quaternion prerot1 = Quaternion.Euler(0, -90, 0); //Prerotaci�n
            Quaternion rotTracker1 = new Quaternion(-pose[1].cx, -pose[1].cy, pose[1].cz, pose[1].cw);
            //t2.rotation = prerot1 * rotTracker1 * this.transform.rotation;
        }

    }

    void OnDestroy()
    {
        Shutdown();
    }
}

