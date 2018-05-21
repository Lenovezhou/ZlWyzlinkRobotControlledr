using System.IO;
using UnityEngine;

public class PNGCapture : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.MenuItem("HoloToolkit/CapturePNG")]
    static void Capture()
    {
        var flag = Camera.main.clearFlags;
        Camera.main.clearFlags = CameraClearFlags.Depth;

        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
        RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 32);

        Camera.main.targetTexture = rt;
        Camera.main.Render();
        var previuosRt = RenderTexture.active;
        RenderTexture.active = rt;

        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        RenderTexture.active = previuosRt;
        Camera.main.targetTexture = null;
        Camera.main.clearFlags = flag;

        var bits = screenShot.EncodeToPNG();
        File.WriteAllBytes("E:\\Temp\\abc.png", bits);
    }
#endif
}
