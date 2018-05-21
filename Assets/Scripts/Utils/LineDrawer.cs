using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// some ideas for more functionality:
// -line renderer lines (so they can be seen in-app)
// -timed lines (delete themselves after time)
// -convert debug lines to real lines
// -hide some lines temporarily
// -UI for line toggling
// -pulsing/animated lines
// -boxes and spheres drawn with lines
// -scaling width lines (screenspace thickness)
// -groups for different files? that can be toggled
public class LineDrawer : Singleton<LineDrawer>
{
    private List<List<Vector3>> DrawLines = new List<List<Vector3>>();
    private List<Color> Colors = new List<Color>();

    private List<Color> debugColors = new List<Color>();
    private List<float> debugDuration = new List<float>();

    private List<GameObject> lineRendererObjs;
    public Material lineMat;

    private LineRenderer AllocNewLine()
    {
        lineRendererObjs.Add(new GameObject());
        var lr = lineRendererObjs[lineRendererObjs.Count - 1].AddComponent<LineRenderer>();
        lr.positionCount = 0;
        lr.widthMultiplier = 0.002f;
        lr.material = lineMat;
        return lr;
    }

    /// <summary>
    /// Render the skeleton piece
    /// </summary>
    public static void DrawSkeleton(Transform rootTransform, SkeletonTransform skeleton, Vector3 offset, Color color, float duration)
    {
        color.a = 0.75f;
        var position = rootTransform.TransformPoint(skeleton.Position + offset);
        var endPosition = rootTransform.TransformPoint(skeleton.EndPosition + offset);
        DrawPolyArm(position, endPosition, color, duration);
        DrawAxisPlane(position,
            rootTransform.TransformDirection(skeleton.Normal), 
            rootTransform.TransformDirection(skeleton.GetLinkOriginVector()), 
            color, duration);
    }

    private static void DrawAxisPlane(Vector3 position, Vector3 normal, Vector3 linkOriginVector, Color color, float duration)
    {
        float radius = 0.015f;
        var axis = Vector3.Angle(normal, Vector3.up) < 30 ? Vector3.left : Vector3.up;
        var k0 = Vector3.Cross(normal, axis).normalized * radius;
        var k1 = Quaternion.AngleAxis(90, normal) * k0;
        DrawSquare(position, k0, k1, color, duration);
        Debug.DrawRay(position, linkOriginVector.normalized * radius * 2, color, duration);
        //Debug.DrawRay(position, normal.normalized * radius * 2, color, duration);
    }

    private static void DrawSquare(Vector3 position, Vector3 left, Vector3 up, Color color, float duration)
    {
        Debug.DrawLine(position + left, position + up, color, duration);
        Debug.DrawLine(position + up, position - left, color, duration);
        Debug.DrawLine(position - left, position - up, color, duration);
        Debug.DrawLine(position - up, position + left, color, duration);
    }

    private static void DrawPolyArm(Vector3 position, Vector3 endPosition, Color color, float duration)
    {
        var dir = (endPosition - position).normalized * 0.01f;
        var axis = Vector3.Angle(dir, Vector3.up) < 5 ? Vector3.forward : Vector3.up;
        var k1 = Quaternion.AngleAxis(60, Vector3.Cross(dir, axis)) * dir;
        Debug.DrawLine(position + k1, endPosition, color, duration);
        Debug.DrawLine(position, position + k1, color, duration);
        var k2 = Quaternion.AngleAxis(90, endPosition - position) * k1;
        Debug.DrawLine(position + k2, endPosition, color, duration);
        Debug.DrawLine(position, position + k2, color, duration);
        var k3 = Quaternion.AngleAxis(90, endPosition - position) * k2;
        Debug.DrawLine(position + k3, endPosition, color, duration);
        Debug.DrawLine(position, position + k3, color, duration);
        var k4 = Quaternion.AngleAxis(90, endPosition - position) * k3;
        Debug.DrawLine(position + k4, endPosition, color, duration);
        Debug.DrawLine(position, position + k4, color, duration);
    }

    public static void DrawCross(Vector3 position, Color c, float duration)
    {
        float radius = 0.005f;
        Debug.DrawLine(position + Vector3.up * radius, position + Vector3.down * radius, c, duration);
        Debug.DrawLine(position + Vector3.left * radius, position + Vector3.right * radius, c, duration);
        Debug.DrawLine(position + Vector3.forward * radius, position + Vector3.back * radius, c, duration);
    }
}
