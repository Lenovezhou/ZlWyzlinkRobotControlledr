using UnityEngine;
using System.Collections;
using System;

public class CurvePanel : MonoBehaviour {

    public AnimationCurve curve01;
    public Material lineRendererMat;

    // Use this for initialization
    void Start () {
        //StartCoroutine(RenderALine(this.curve01));
	}

    public IEnumerator RenderALine(AnimationCurve curve, Color color)
    {
        Debug.Log("Line Rendering started");
        yield return new WaitForEndOfFrame();

        Debug.Log("Line Rendering going");
        var lineRenderer = CreateLineRenderer(color);
        yield return new WaitForEndOfFrame();

        Debug.Log("Line Rendering");
        for (int i = 0; i < 101; i+=5)
        {
            lineRenderer.UpdateCurve(curve, ((float)i) / 100);
            yield return new WaitForSeconds(0.2f);
            Debug.Log("Updating " + 100 + "%");
        }
    }

    private CurveRenderer CreateLineRenderer(Color color)
    {
        Debug.Log("Creating line renderer");
        var gameObject = new GameObject("LineRenderer", typeof(LineRenderer), typeof(CurveRenderer));
        gameObject.transform.SetParent(this.transform);
        gameObject.transform.localPosition = Vector3.zero;
        gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        Debug.Log("Created line renderer");

        var lineRenderer = gameObject.GetComponent<LineRenderer>();
        lineRenderer.SetWidth(0.01f, 0.01f);
        lineRenderer.material = lineRendererMat;
        lineRenderer.SetColors(color, color);

        var rectTransform = this.GetComponent<RectTransform>();
        Debug.Log("Size of rect is: " + rectTransform.rect.ToString());
        var curveRenderer = gameObject.GetComponent<CurveRenderer>();
        curveRenderer.SetWidth(rectTransform.rect.width * 0.2f / 100);     // TODO: Refernece pixels per unity and parent transform
        curveRenderer.SetHeight(rectTransform.rect.height * 0.2f / 100);

        return gameObject.GetComponent<CurveRenderer>();
    }

    // Update is called once per frame
    void Update () {
	
	}
}
