using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(FenceBuilder))]
public class FenceBuilderEditor : Editor
{

    FenceBuilder fb;
    Vector3 handle;
    bool BuildFenceFlag = false;
    public override void OnInspectorGUI()
    {
        FenceBuilder fb = (FenceBuilder)target;
        DrawDefaultInspector();

        if (GUILayout.Button("Build Fence"))
        {
            fb.BuildFence(true);
        }
    }
    public void OnSceneGUI()
    {
        
        HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive)); // Useless?
        FenceBuilder fb = (FenceBuilder)target;

        //Flag build fence in advance TODO: figure out why this works
        if (Event.current.type == EventType.MouseMove ||
            Event.current.type == EventType.MouseUp)
        {
            if (fb.endPosModified)
            {
                fb.endPosModified = false;
                BuildFenceFlag = true;
            }
        }
        Vector3 newPos = Handles.PositionHandle(fb.endPos, Quaternion.identity);
        
        if (newPos != fb.endPos)
        {
            fb.SetEndPosCorrected(newPos);
            
        }
        // if build fence was flagged then build fence
        if (BuildFenceFlag)
        {
            fb.BuildFence();
            BuildFenceFlag = false;
        }

    }
}
