using UnityEngine;
using System.Collections;
using ProtoTurtle.BitmapDrawing;

public class CurveTest : MonoBehaviour {

    private Texture2D texture;

    public AnimationCurve curve;

    public float width = 500f;
    public float height = 300f;

	// Use this for initialization
	void Start () {
        texture = new Texture2D((int)width, (int)height);
	}
	
	// Update is called once per frame
	void Update () {
        texture.DrawFilledRectangle(new Rect(0, 0, width, height), new Color(0, 0, 0, 0));
        texture.DrawLine(new Vector2(0, height / 2), new Vector2(width, height / 2), Color.cyan);
        texture.DrawLine(new Vector2(0, 0), new Vector2(width, height), Color.cyan);

        float previousI = 0.0f;
        for (float i = 0.01f; i < 1; i += 0.001f)
        {
            //texture.DrawLine(new Vector2(previousI * width, curve.Evaluate(previousI) * height / 2 + height / 2), 
            //    new Vector2(i * width, curve.Evaluate(i) * height / 2 + height / 2), 
            //    Color.red);
            //texture.DrawLine(new Vector2(previousI * width, curve.EvaluateTangent(previousI) * height / 8 + height / 2), 
            //    new Vector2(i * width, curve.EvaluateTangent(i) * height / 8 + height / 2), 
            //    Color.blue);
            texture.DrawPixel((int)(i * width), (int)(curve.Evaluate(i) * height / 2 + height / 2), Color.red);
            texture.DrawPixel((int)(i * width), (int)(curve.EvaluateTangent(i) + height / 2), Color.blue);
            texture.DrawPixel((int)(i * width), (int)(curve.EvaluateTangentNegative(i) + 5 + height / 2), Color.blue);
            previousI = i;
        }

        foreach (var key in this.curve.keys)
        {
            drawCross(texture, (int)(key.time * width), (int)(key.value * height / 2 + height / 2), 4, Color.yellow);
        }
        texture.Apply();
    }

    private void drawCross(Texture2D texture, int x, int y, int size, Color color)
    {
        texture.DrawLine(new Vector2(x - size, y), new Vector2(x + size, y), color);
        texture.DrawLine(new Vector2(x, y - size), new Vector2(x, y + size), color);
    }

    void OnGUI()
    {
        GUI.DrawTexture(new Rect(0, 0, width, height), texture);
    }
}
