using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class PathRenderer : MonoBehaviour
{
    public int renderingLength = 300;
    private LineRenderer lineRenderer;
    private Queue<Vector3> positions = new Queue<Vector3>();

    private void Start()
    {
        this.lineRenderer = GetComponent<LineRenderer>();
    }

    public void AppendPoint(Vector3 point)
    {
        positions.Enqueue(point);
        if (positions.Count > renderingLength)
        {
            positions.Dequeue();
        }
        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
    }
}
