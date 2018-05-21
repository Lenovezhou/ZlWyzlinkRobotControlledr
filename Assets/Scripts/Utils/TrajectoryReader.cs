using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(RobotControllerSimple))]
[RequireComponent(typeof(RobotSkeleton))]
public class TrajectoryReader : MonoBehaviour
{
    public TextAsset trajectoryStream;
    public TextAsset trajectoryReferenceStream;
    private RobotControllerSimple robot;
    private RobotSkeleton skeleton;
    public IndicatorUIBase[] indicator;
    public PathRenderer pathRendererPrefab;
    private PathRenderer pathRenderer;
    public PathRenderer referencePathRendererPrefab;
    private PathRenderer referencePathRenderer;
    public bool showPath;
    public bool showReferencePath;

    // Use this for initialization
    void Start ()
    {
        Application.targetFrameRate = 50;
        robot = GetComponent<RobotControllerSimple>();
        skeleton = GetComponent<RobotSkeleton>();
        StartCoroutine(_StartTrajectory());

        //
        this.pathRenderer = Instantiate(pathRendererPrefab);
        this.referencePathRenderer = Instantiate(referencePathRendererPrefab);
	}

    private IEnumerator _StartTrajectory()
    {
        yield return new WaitForSeconds(1);
        using (StringReader reader = new StringReader(trajectoryStream.text))
        using (StringReader referenceReader = trajectoryReferenceStream != null ? new StringReader(trajectoryReferenceStream.text) : null)
        {
            reader.ReadLine();      // Drop the first line
            if (referenceReader != null)
            {
                referenceReader.ReadLine();
            }

            float[] anglesBuffer = new float[6];
            float[] consumptionBuffer = new float[6];
            while (true)
            {
                var line = reader.ReadLine();
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                ReadDataFormat(line, anglesBuffer, consumptionBuffer);
                //Debug.Log(ReadLog(anglesBuffer, consumptionBuffer));
                robot.SetAnglesImmediate(anglesBuffer, consumptionBuffer);

                if (showPath)
                {
                    this.pathRenderer.AppendPoint(skeleton.CalcEndEffectorPosition(anglesBuffer));
                }

                if (referenceReader != null && this.referencePathRenderer != null && showReferencePath)
                {
                    var referenceLine = referenceReader.ReadLine();
                    if (referenceLine != null)
                    {
                        ReadDataFormat(referenceLine, anglesBuffer, consumptionBuffer);
                        this.referencePathRenderer.AppendPoint(skeleton.CalcEndEffectorPosition(anglesBuffer));
                    }
                }

                if (indicator.Length > 0)
                {
                    for (int i = 0; i < indicator.Length; i++)
                    {
                        indicator[i].UpdateValues(anglesBuffer[i], consumptionBuffer[i]);
                    }
                }

                yield return -1;
            }
        }
    }

    private object ReadLog(float[] anglesBuffer, float[] consumptionBuffer)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var a in anglesBuffer)
        {
            sb.Append(a).Append(',');
        }
        sb.Append(" - ");
        foreach (var b in consumptionBuffer)
        {
            sb.Append(b).Append(',');
        }
        return sb.ToString();
    }

    private void ReadDataFormat(string line, float[] anglesBuffer, float[] consumptionBuffer)
    {
        try {
            string[] data = line.Split(',', ' ');

            for (int i = 0; i < data.Length; i++)
            {
                if (i < 6)
                {
                    anglesBuffer[i] = Mathf.Rad2Deg * float.Parse(data[i]);
                    anglesBuffer[i] *= (i == 4 ? 1 : -1);
                }
                else if (i >= 9 && i < 15)
                {
                    consumptionBuffer[i - 9] = float.Parse(data[i]);
                }
            }
        }
        catch (FormatException)
        {
            Debug.LogError("Failed to parse the text: " + line);
        }
    }
}
