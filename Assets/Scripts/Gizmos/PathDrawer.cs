using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class PathDrawer : MonoBehaviour
{
    private LineRenderer lineRenderer;
    private LineRenderer lineRendererLeft;
    private LineRenderer lineRendererRight;
    private const float lineWidth = 0.005f;
    private const float subLineWidth = 0.001f;
    private const float subLineDistance = 0.04f;

    internal static PathDrawer Create(Material lineRendererMaterial, RotationPosition[] rotationPosition)
    {
        var gameObject = new GameObject("LineDrawer", typeof(PathDrawer), typeof(LineRenderer));
        var drawer = gameObject.GetComponent<PathDrawer>();
        drawer.lineRenderer = gameObject.GetComponent<LineRenderer>();
        drawer.lineRenderer.startWidth = lineWidth;
        drawer.lineRenderer.endWidth = lineWidth;
        drawer.lineRenderer.material = lineRendererMaterial;
        drawer.UpdateData(rotationPosition);
        drawer.lineRendererLeft = CreateLineRenderer("SubLineLeft", gameObject.transform, lineRendererMaterial);
        drawer.lineRendererRight = CreateLineRenderer("SubLineRight", gameObject.transform, lineRendererMaterial);
        return drawer;
    }

    private static LineRenderer CreateLineRenderer(string name, Transform parent, Material lineRendererMaterial)
    {
        var subLineLeft = new GameObject(name, typeof(LineRenderer));
        subLineLeft.transform.SetParent(parent);
        var lineRenderer = subLineLeft.GetComponent<LineRenderer>();
        lineRenderer.startWidth = subLineWidth;
        lineRenderer.endWidth = subLineWidth;
        lineRenderer.material = lineRendererMaterial;
        return lineRenderer;
    }

    internal void UpdateData(RotationPosition[] rotationPosition)
    {
        StartCoroutine(_Update(rotationPosition));

    }

    private IEnumerator _Update(RotationPosition[] rotationPosition)
    {
        var delay = new WaitForEndOfFrame();
        yield return delay;

        if (this.lineRenderer.positionCount <= 2)
        {
            for (int i = 0; i < rotationPosition.Length; i++)
            {
                UpdatePathRendererCount(i + 1);
                UpdatePathRenderer(i, rotationPosition);
                yield return delay;
            }
        }
        else
        {
            int i = 0;
            for (i = 0; i < Math.Min(this.lineRenderer.positionCount, rotationPosition.Length); i++)
            {
                UpdatePathRenderer(i, rotationPosition);
                yield return delay;
            }
            if (this.lineRenderer.positionCount > rotationPosition.Length)
            {
                UpdatePathRendererCount(rotationPosition.Length);
            }
            else
            {
                for (; i < rotationPosition.Length; i++)
                {
                    UpdatePathRendererCount(i + 1);
                    UpdatePathRenderer(i, rotationPosition);
                    yield return delay;
                }
            }
        }
    }

    private void UpdatePathRenderer(int i, RotationPosition[] rotationPosition)
    {
        this.lineRenderer.SetPosition(i, rotationPosition[i].Position);
        this.lineRendererLeft.SetPosition(i, rotationPosition[i].Position + rotationPosition[i].Rotation * Vector3.left * subLineDistance);
        this.lineRendererRight.SetPosition(i, rotationPosition[i].Position + rotationPosition[i].Rotation * Vector3.right * subLineDistance);
    }

    private void UpdatePathRendererCount(int i)
    {
        this.lineRenderer.positionCount = i;
        this.lineRendererLeft.positionCount = i;
        this.lineRendererRight.positionCount = i;
    }

    internal void CloseUp()
    {
        StartCoroutine(_CloseUp());
    }

    private IEnumerator _CloseUp()
    {
        var delay = new WaitForEndOfFrame();
        if (this.lineRenderer.positionCount > 0)
        {
            var lastPosition = this.lineRenderer.GetPosition(this.lineRenderer.positionCount - 1);
            for (int i = this.lineRenderer.positionCount; i > 0; i--)
            {
                UpdatePathRendererCount(i - 1);
                yield return delay;
            }
        }
        Destroy(this.gameObject, 0.1f);
    }
}
