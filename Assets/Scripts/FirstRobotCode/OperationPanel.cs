using UnityEngine;
using System.Collections;

public class OperationPanel : Singleton<OperationPanel> {

    private CurvePanel curvePanel;
    private Color[] colors = new Color[] { Color.blue, Color.cyan, Color.red, Color.yellow, Color.magenta };
    private AudioSource audioSource;

    public AudioClip clickSound;

    void Start()
    {
        this.curvePanel = this.transform.FindDescendentOfType<CurvePanel>();
        this.audioSource = this.transform.GetComponentInChildren<AudioSource>();
    }

    //
    // Behavior 1
    //
    public void ButtonActionStart()
    {
        this.audioSource.PlayOneShot(clickSound);
        // Start action
        RobotController.Instance.StartAction();
    }

    //
    // Behavior 2
    //
    public void ButtonStartCurves()
    {
        this.audioSource.PlayOneShot(clickSound);

        this.StartRenderingCurves(RobotController.Instance.testCurves);
    }

    //
    // Behavior 3
    //
    public void ButtonStartTracks()
    {
        this.audioSource.PlayOneShot(clickSound);

        RobotController.Instance.StartTrack();
    }

    public void StartRenderingCurves(params AnimationCurve[] curves)
    {
        int i = 0;
        foreach (var curve in curves)
        {
            StartCoroutine(curvePanel.RenderALine(curve, colors[i % colors.Length]));
            i++;
        }
    }
}
