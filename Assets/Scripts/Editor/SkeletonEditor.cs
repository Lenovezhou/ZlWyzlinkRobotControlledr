using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RobotSkeleton))]
public class SkeletonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var myTarget = (RobotSkeleton)target;

        if (myTarget.debugVisual)
        {
            EditorGUILayout.LabelField("Angles", GetAnglesString(myTarget));
        }
    }

    private string GetAnglesString(RobotSkeleton myTarget)
    {
        var angles = myTarget.GetAngles();
        string anglesStr = "";
        foreach (var angle in angles)
        {
            anglesStr += angle.ToString("0.00") + ", ";
        }
        return anglesStr;
    }
}
