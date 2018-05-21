using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionUtils
{
    static public bool IsNaN(this Vector3 v)
    {
        return float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z);
    }
}
