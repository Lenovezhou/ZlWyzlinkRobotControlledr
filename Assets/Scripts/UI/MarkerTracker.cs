
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

public class MarkerTracker : MonoBehaviour
{
    [SerializeField]
    private TrakingProcessUI trakingprocessui;
    private float eachPercent = 0.084f;
    private Animator animator;
    public Vuforia.VuforiaBehaviour vuforiaBehaviour;
    public Vuforia.ImageTargetBehaviour[] imageTargets = new Vuforia.ImageTargetBehaviour[6];
    public ImageTargetTracker[] trackers;
    private bool isDetected = false;
    private PositionRotation result;

    private Image InterA1;
    private Image InterA2;
    private Image InterA3;
    private Image InterA4;
    private Image BorderA1;
    private Image BorderA2;
    private Image BorderA3;
    private Image BorderA4;
    private Image Center;
    private Image CenterLine;

    private Coroutine ScaningPlayMusic;

    /// The format of the image target should be like this:
    ///            Robot Arm Position
    ///                   T5
    /// 
    ///        T4                    T3
    ///        
    ///        
    ///                   T0
    ///                   
    ///
    ///        T1                    T2

    // Use this for initialization
    void Start () {
        this.animator = GetComponent<Animator>();
        Debug.Assert(this.animator != null);
        if (vuforiaBehaviour != null)
        {

            trackers = transform.FindChildrenOfType<ImageTargetTracker>().ToArray();
            Debug.Assert(trackers.Length == imageTargets.Length);
            for (int i = 0; i < trackers.Length; i++)
            {
                trackers[i].SetTarget(imageTargets[i], LockedCallBack);
            }
            trackers[1].SetAngleNumbers(trackers[2], trackers[4]);
            trackers[2].SetAngleNumbers(trackers[1], trackers[3]);
            trackers[3].SetAngleNumbers(trackers[2], trackers[4]);
            trackers[4].SetAngleNumbers(trackers[1], trackers[3]);

            // Initialize indicator lines
            InterA1 = transform.Find("Indicators/InterA1").GetComponent<Image>();
            InterA2 = transform.Find("Indicators/InterA2").GetComponent<Image>();
            InterA3 = transform.Find("Indicators/InterA3").GetComponent<Image>();
            InterA4 = transform.Find("Indicators/InterA4").GetComponent<Image>();
            BorderA1 = transform.Find("Indicators/BorderA1").GetComponent<Image>();
            BorderA2 = transform.Find("Indicators/BorderA2").GetComponent<Image>();
            BorderA3 = transform.Find("Indicators/BorderA3").GetComponent<Image>();
            BorderA4 = transform.Find("Indicators/BorderA4").GetComponent<Image>();
            Center = transform.Find("Indicators/Center").GetComponent<Image>();
            CenterLine = transform.Find("Indicators/CenterLine").GetComponent<Image>();

        }
        else
        {
            Debug.LogError("The vuforia component is required");
        }
	}

    public void StartTraking()
    {
        // Start the detection process
        StartCoroutine(DetectMarkerStatusForCalibration());
    }

    private IEnumerator DetectMarkerStatusForCalibration()
    {
        yield return new WaitForSeconds(1);
        PlayEffect("openInterface");
        this.vuforiaBehaviour.enabled = true;

        yield return new WaitForSeconds(1);
        int trackerCount = 0;
        bool borderAligned = false;
        bool angleAligned = false;
        bool centerAligned = false;
        while (trackerCount < 4 || !borderAligned || !angleAligned/* || !centerAligned*/)
        {
            trackerCount = 0;

            /// 
            /// The rules to follow:
            /// 1. All 6 trackers are being tracked
            /// 
            foreach (var t in trackers)
            {
                if (t.isLocked())
                {
                    trackerCount++;
                }
            }

            /// 
            /// 2. Tracker 0-4 is on the same level
            /// 
            if (!this.BorderA1.enabled && IsYLevelInRange(1, 2, 4, 0.05f))      // 5cm range
            {
                FlashEnable(this.BorderA1);
            }
            if (!this.BorderA2.enabled && IsYLevelInRange(2, 1, 3, 0.05f))      // 5cm range
            {
                FlashEnable(this.BorderA2);
            }
            if (!this.BorderA3.enabled && IsYLevelInRange(3, 2, 4, 0.05f))      // 5cm range
            {
                FlashEnable(this.BorderA3);
            }
            if (!this.BorderA4.enabled && IsYLevelInRange(4, 1, 3, 0.05f))      // 5cm range
            {
                FlashEnable(this.BorderA4);
            }
            borderAligned = this.BorderA1.enabled && this.BorderA2.enabled && this.BorderA3.enabled && this.BorderA4.enabled;

            /// 
            /// 3. Found 4 right angles
            /// 
            if (!this.InterA1.enabled && IsRightAngle(1, 2, 4, 1.0f))      // 5cm range
            {
                FlashEnable(this.InterA1);
            }
            if (!this.InterA2.enabled && IsRightAngle(2, 1, 3, 1.0f))      // 5cm range
            {
                FlashEnable(this.InterA2);
            }
            if (!this.InterA3.enabled && IsRightAngle(3, 2, 4, 1.0f))      // 5cm range
            {
                FlashEnable(this.InterA3);
            }
            if (!this.InterA4.enabled && IsRightAngle(4, 1, 3, 1.0f))      // 5cm range
            {
                FlashEnable(this.InterA4);
            }
            angleAligned = this.InterA1.enabled && this.InterA2.enabled && this.InterA3.enabled && this.InterA4.enabled;

            /// 
            /// 4. Tracker 0 is on on the center of others
            /// 


            /// 
            /// 5. Tracker 5 is on the the middle line
            /// 
            if (!this.CenterLine.enabled && IsOnCenterLine(5, 1, 2, 3, 4, 0.05f))
            {
                FlashEnable(this.CenterLine);
            }
            centerAligned = this.CenterLine.enabled;

            yield return new WaitForSeconds(0.5f);
        }
        trakingprocessui.Process = 1;

        // Find out the position and verify the position
        var centerRay = GetCenterRay();
        var position = centerRay.GetPoint(Vector3.Distance(centerRay.origin, this.trackers[5].lockedPosition)) ;
        var rotation = Quaternion.FromToRotation(Vector3.forward, -centerRay.direction);
        var tempforward = new Vector3(centerRay.direction.x,0,centerRay.direction.z);
        var forward = tempforward;
        //this.result = new PositionRotation(position, forward);
        this.result = new PositionRotation(GetPosition(), GetForward());

        // Done
        this.isDetected = true;
        this.vuforiaBehaviour.enabled = false;
        this.animator.SetBool("Open", false);
        PlayEffect("closeInterface");
        Run.After(3, ClearTrackerMemeray);
    }


    private Vector3 GetPosition()
    {
        return ((this.trackers[1].lockedPosition + this.trackers[2].lockedPosition + this.trackers[4].lockedPosition + this.trackers[3].lockedPosition) / 4);
    }

    private Vector3 GetForward()
    {
        Vector3 forword = (this.trackers[3].lockedPosition - this.trackers[4].lockedPosition).normalized;
        //只需要y轴方向旋转
        forword = new Vector3(forword.x, 0, forword.z);
        return forword;
    }


    private void ClearTrackerMemeray()
    {
        for (int i = 0; i < trackers.Length; i++)
        {
            trackers[i].ClearMemery();
        }
        this.BorderA1.enabled = false;
        this.BorderA2.enabled = false;
        this.BorderA3.enabled = false;
        this.BorderA4.enabled = false;
        this.InterA1.enabled = false;
        this.InterA2.enabled = false;
        this.InterA3.enabled = false;
        this.InterA4.enabled = false;
        this.Center.enabled = false;
        this.CenterLine.enabled = false;

        this.isDetected = false;
    }


    private bool IsOnCenterLine(int point5, int rightFront, int leftFront, int leftBack, int rightBack, float maxError)
    {
        if (this.trackers[point5].isLocked() && 
            this.trackers[leftFront].isLocked() && this.trackers[rightFront].isLocked() &&
            this.trackers[leftBack].isLocked() && this.trackers[rightBack].isLocked())
        {
            var centerRay = GetCenterRay();
            var distanceToCenterLine = Vector3.Cross(centerRay.direction, this.trackers[point5].lockedPosition - centerRay.origin).magnitude;
            if (distanceToCenterLine < maxError)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsRightAngle(int v1, int v2, int v3, float maxError)
    {
        if (this.trackers[v1].isLocked() && this.trackers[v2].isLocked() && this.trackers[v3].isLocked())
        {
            var angle = Vector3.Angle(this.trackers[v2].lockedPosition - this.trackers[v1].lockedPosition,
                this.trackers[v3].lockedPosition - this.trackers[v1].lockedPosition);
            if (Mathf.Abs(angle - 90) < maxError)
            {
                return true;
            }
        }
        return false;
    }

    private bool IsYLevelInRange(int v1, int v2, int v3, float maxError)
    {
        if (this.trackers[v1].isLocked() && this.trackers[v2].isLocked() && this.trackers[v3].isLocked())
        {
            if (Mathf.Abs(this.trackers[v1].lockedPosition.y - this.trackers[v2].lockedPosition.y) < maxError &&
                Mathf.Abs(this.trackers[v1].lockedPosition.y - this.trackers[v3].lockedPosition.y) < maxError)
            {
                return true;
            }
        }
        return false;
    }

    private void FlashEnable(Image image)
    {
        StartCoroutine(_FlashEnableImage(image));
    }

    private IEnumerator _FlashEnableImage(Image image)
    {
        trakingprocessui.Process += eachPercent;
        yield return -1;
        var alpha = image.color.a;
        WaitForSeconds waitForSeconds = new WaitForSeconds(0.5f);

        image.enabled = true;
        yield return waitForSeconds;
        image.color = image.color.SetAlpha(0);
        yield return waitForSeconds;
        image.color = image.color.SetAlpha(alpha);
        yield return waitForSeconds;
        image.color = image.color.SetAlpha(0);
        yield return waitForSeconds;
        image.color = image.color.SetAlpha(alpha);
        PlayEffect("FlashEnableImage",0.25f);
    }

    private Ray GetCenterRay()
    {
        var p1 = (this.trackers[1].lockedPosition + this.trackers[2].lockedPosition) / 2;
        var p2 = (this.trackers[4].lockedPosition + this.trackers[3].lockedPosition) / 2;
        return new Ray(p1, p2 - p1);
    }

    private float CheckYAxis(int v1, int v2, int v3)
    {
        //return Mathf.Max(v1.)
        return 0.0f;
    }

    internal bool FinishedDetection()
    {
        // Check all the children Vuforia tracker, and test if we have collected them all;
        return this.isDetected;
    }

    internal PositionRotation GetTrackingPositionRotation()
    {
        return result;
    }

    void PlayEffect(string path, float volume = 1)
    {
        Sound.Instance.PlayerEffect(path, volume);
    }
    void LockedCallBack()
    {
        Sound.Instance.PlayerEffect("LockedTarget");
        trakingprocessui.Process += eachPercent;
    }
}
