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
        if (GUILayout.Button("Parent Fence"))
        {
            fb.ParentFenceToRoot();
        }
        if (GUILayout.Button("Clean Fence Objects"))
        {
            fb.DestroyFence();
        }
    }
    public void OnSceneGUI()
    {
        
        //HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive)); // Useless?
        Tools.hidden = false;
        Tools.current = Tool.None;
        FenceBuilder fb = (FenceBuilder)target;

        //Flag build fence in advance TODO: figure out why this works

        if (Event.current.type == EventType.MouseMove ||
            Event.current.type == EventType.MouseUp)
        {
            if (fb.anyPosModified && fb.autoUpdateFence)
            {
                fb.anyPosModified = false;
                BuildFenceFlag = true;
            }
        }
        
        GUIStyle guiStyle = new GUIStyle();

        guiStyle.normal.textColor = Color.white;
        guiStyle.fontStyle = FontStyle.Bold;
        for (int i = 0; i < fb.points.Length; i++)
        {
            
            Handles.Label(fb.points[i], new GUIContent("Point "+(i+1).ToString()), guiStyle);

            Quaternion handleRotation = Quaternion.identity;

            Vector3 newPos = Handles.PositionHandle(fb.points[i], handleRotation);
            if (newPos != fb.points[i])
            {
                fb.SetPoint(newPos,i);

            }
        }
        if (BuildFenceFlag)
        {
            fb.BuildFence();
            BuildFenceFlag = false;
        }




    }
}
