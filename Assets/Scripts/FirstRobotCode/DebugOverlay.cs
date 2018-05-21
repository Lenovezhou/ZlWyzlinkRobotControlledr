using UnityEngine;
using System.Collections;
using ProtoTurtle.BitmapDrawing;
using System;

public class DebugOverlay {
    private Texture2D screenTexture;
    private float totalWidth;
    private float totalHeight;

    private const int sectionCount = 5;
    private float sectionHeight;
    private Color axisColor = new Color(0.4f, 0.2f, 0.8f, 0.5f);

    private bool isEnabled;

    public DebugOverlay(float width, float height, bool isEnabled)
    {
        this.isEnabled = isEnabled;
        if (!isEnabled) return;

        this.totalWidth = width;
        this.totalHeight = height;
        this.sectionHeight = totalHeight / sectionCount;

        this.screenTexture = new Texture2D((int)width, (int)height);
        this.Clear();
    }

    public void DrawCurve(int index, AnimationCurve curve, Color color)
    {
        if (!isEnabled) return;
        Debug.Assert(index < sectionCount);
        
        for (float i = 0; i < 1; i += 0.01f)
        {
            DrawPoint(index, i, curve.Evaluate(i), color);
        }
        this.screenTexture.Apply();
    }

    public void Clear()
    {
        if (!isEnabled) return;

        this.screenTexture.DrawFilledRectangle(new Rect(0, 0, totalWidth, totalHeight), new Color(0.1f, 0.1f, 0.5f, 0.2f));
        for (int i = 0; i < sectionCount; i++)
        {
            var y = this.sectionHeight * i + this.sectionHeight / 2;
            this.screenTexture.DrawLine(new Vector2(0, y), new Vector2(totalWidth, y), axisColor);
        }
        this.screenTexture.Apply();
    }

    public void DrawPoint(int index, float x, float y, Color color)
    {
        if (!isEnabled) return;

        this.screenTexture.SetPixel((int)(x * totalWidth), (int)(y * sectionHeight / 2 + sectionHeight / 2 + sectionHeight * index), color);
    }

    public void Apply()
    {
        if (!isEnabled) return;

        this.screenTexture.Apply();
    }

    internal void DrawTexture()
    {
        GUI.DrawTexture(new Rect(5, 60, totalWidth, totalHeight), this.screenTexture);
    }
}
