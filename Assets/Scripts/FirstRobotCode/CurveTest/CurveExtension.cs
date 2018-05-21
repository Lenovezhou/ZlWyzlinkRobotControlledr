using UnityEngine;
using System.Collections;

public static class CurveExtension {
    public static float EvaluateTangent(this AnimationCurve curve, float time, float deltaT = 0.001f)
    {
        float v0 = curve.Evaluate(time) * 50.0f;
        float v1 = curve.Evaluate(time + deltaT) * 50.0f;
        return (v1 - v0) / deltaT;
    }
    public static float EvaluateTangentNegative(this AnimationCurve curve, float time, float deltaT = 0.001f)
    {
        float v0 = curve.Evaluate(time) * 50.0f;
        float v1 = curve.Evaluate(time - deltaT) * 50.0f;
        return (v0 - v1) / deltaT;
    }
}
