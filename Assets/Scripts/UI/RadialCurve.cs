using ProtoTurtle.BitmapDrawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class RadialCurve : MonoBehaviour
{
    private float angularVerlocity = 10f;
    private float dataScale = 0.5f;
    private float dataOffset = 100.0f;

    private float startTime;

    private int size = 1024;

    private Texture2D texture;
    private RawImage rawImage;

    private void Start()
    {
        this.startTime = Time.time;

        this.texture = new Texture2D(size, size);
        this.texture.DrawFilledRectangle(new Rect(0, 0, size, size), new Color(0, 0, 0, 0));
        this.texture.DrawLine(0, 0, 256, 256, Color.white);
        this.texture.Apply();
        this.rawImage = GetComponent<RawImage>();
        this.rawImage.texture = this.texture;
    }

    public void UpdateData(float data)
    {
        float angle = -(Time.time - this.startTime) * angularVerlocity;
        data = data * dataScale + dataOffset;
        var startPoint = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad) * dataOffset + size / 2, Mathf.Cos(angle * Mathf.Deg2Rad) * dataOffset + size / 2);
        var point = new Vector2(Mathf.Sin(angle * Mathf.Deg2Rad) * data + size/2, Mathf.Cos(angle * Mathf.Deg2Rad) * data + size/2);

        this.texture.DrawLine(startPoint, point, Color.white);
        this.texture.Apply();
    }
}
