using System;
using UnityEngine;

[Serializable]
public class ScanData
{
    public Pose CameraPosition;
    public Texture2D Frame;

    public ScanData(Pose cameraPosition, Texture2D frame)
    {
        CameraPosition = cameraPosition;
        Frame = frame;
    }

}