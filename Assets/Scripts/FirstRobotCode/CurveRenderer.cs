using UnityEngine;
using System.Collections;
using System;

public class CurveRenderer : MonoBehaviour {

    private LineRenderer lineRenderer;
    private float sampleRate = 100;
    private float width;
    private float height;

    void Start()
    {
        this.lineRenderer = GetComponent<LineRenderer>();

        Debug.Assert(this.lineRenderer != null);
    }

    public void SetWidth(float width)
    {
        this.width = width;
    }

    public void SetHeight(float height)
    {
        this.height = height;
    }

    internal void UpdateCurve(AnimationCurve curve01, float ratio)
    {
        Vector3[] positions = new Vector3[100];
        int lastIndex = 0;
        for (int i = 0; i < 100; i++)
        {
            if (i < 100 * ratio)
            {
                Vector3 point = new Vector3(
                    (float)i * width / 100.0f - width / 2,
                    curve01.Evaluate(((float)i) / sampleRate) / 10,
                    -0.02f);
                positions[i] = this.transform.TransformPoint(point);
                lastIndex = i;
            }
            else
            {
                positions[i] = positions[lastIndex];
            }
        }
        Debug.Log("Position is " + (positions == null ? "null":positions.Length.ToString()));
        Debug.Log("lineRenderer is " + (this.GetComponent<LineRenderer>() == null ? "null" : "Valid"));
        this.GetComponent<LineRenderer>().SetVertexCount(100);
        this.GetComponent<LineRenderer>().SetPositions(positions);
    }
}
