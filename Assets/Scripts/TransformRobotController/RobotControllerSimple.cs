using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// Controller for a robot arm with multiple degrees of freedom (axes).
/// Joints must be setup with RobotArmJoint components and added to Joints.
/// </summary>
[RequireComponent(typeof(RobotSkeleton))]
public class RobotControllerSimple : MonoBehaviour
{
    public bool InAppDebugVisualsOn = false;
    public bool EditorDebugVisualsOn = true; // this will be set to true if InAppDebugVisualsOn is true

    [SerializeField]
    float defaultMaximumAngularSpeed = 30;
    [Tooltip("Add RobotArmJoints Here.")]
    [SerializeField]
    RobotArmJoint[] robotJoints;
    [SerializeField]
    public Transform EndEffectorGrabPoint;

    [SerializeField]
    GameObject clawModel;

    private RobotSkeleton skeleton;

    [SerializeField]
    int maximumFabrikPasses = 50;

    [SerializeField]
    UnityEngine.UI.Text RobotProgramText;
    [SerializeField]
    RemoteRobotController RemoteRobot;

    [SerializeField]
    private GameObject testPointObj;
    private WayPointManager wayPointManager;
    Vector3 prevPos;

    private RobotAggregator ActiveRobots;

    float distanceEpsilon = 0.1f; // if the target is less than this far away from EE, ignore the movement (see CheckDistanceFromTarget)
    float SecondsDelayBetweenTargets = 1.0f;
    private float[] leftRotateEE = new float[] { 0, 0, 0, 0, 0, 90 };
    private float[] rightRotateEE = new float[] { 0, 0, 0, 0, 0, -90 };

    private List<Vector3> debugDrawLines = new List<Vector3>();
    private List<Color> debugColors = new List<Color>();
    Coroutine lastRoutine = null;

    [Header("Debugging")]
    public bool displayJointLimits = false;

    // TODO: Make these private
    public string RunningProgramLine { get; set; }
    public bool IsRunning { get; set; }
    public GameObject ClawModel
    {
        get
        {
            return clawModel;
        }

        set
        {
            clawModel = value;
        }
    }

    public RobotArmJoint[] RobotJoints
    {
        get
        {
            return robotJoints;
        }

        private set
        {
            robotJoints = value;
        }
    }

    void Awake()
    {
        if (InAppDebugVisualsOn)
        {
            EditorDebugVisualsOn = true;
        }

        ActiveRobots = GetComponent<RobotAggregator>();

        this.RunningProgramLine = "";

        skeleton = this.GetComponent<RobotSkeleton>();
        skeleton.InitializeSkeleton(this);

        this.wayPointManager = WayPointManager.Instance;
        if (this.wayPointManager != null)
        {
            this.wayPointManager.RenderPath(this);
        }
    }

    private void Start()
    {
        bool debugState = false;
        if (InAppDebugVisualsOn)
        {
            debugState = true;
        }
        for (int i = 0; i < robotJoints.Length; i++)
        {
            //robotJoints[i].SetDebuggingVisuals(debugState);
        }

        //SetJointEnds();

        //ActiveRobots.RunProgramLine("HOME");
    }

    public int GetNumberOfJoints()
    {
        Debug.Assert(RobotJoints.Length > 0);
        return robotJoints.GetLength(0);
    }

    public RobotArmJoint[] GetJoints()
    {
        Debug.Assert(RobotJoints.Length > 0);
        return this.robotJoints;
    }

    public float[] GetAngles()
    {
        return GetJoints().Select(j => j.GetAngle()).ToArray();
    }

    public void SetAnglesImmediate(float[] angles, float[] powerConsumption = null)
    {
        for (int i = 0; i < this.RobotJoints.Length; i++)
        {
            this.RobotJoints[i].RotateToAngle(angles[i], powerConsumption == null ? 0 : powerConsumption[i]);
        }
    }
    
    private int ParsePoint(string s)
    {
        Debug.Assert(s.Length > 0);
        s = s.Substring(1, s.Length - 1);
        return int.Parse(s);
    }

    private bool CheckDistanceFromTarget(Vector3 target)
    {
        float distFromTarget = Vector3.Distance(target, this.EndEffectorGrabPoint.position);
        return distFromTarget < distanceEpsilon;
    }

    //public void IKmoveToTarget(Vector3 endEffectorDirection)
    //{
    //    Debug.Assert(endEffectorDirection != Vector3.zero, "Zero vector not implemented for end effector direction");

    //    if (testPointObj == null)
    //    {
    //        StartCoroutine(IKMoveToTargetArray(endEffectorDirection));
    //        return;
    //    }

    //    float[] newArmPosition = new float[robotJoints.Length];
    //    for (int i = 0; i < robotJoints.Length; i++)
    //    {
    //        newArmPosition[i] = robotJoints[i].GetAngle();
    //    }


    //    debugDrawLines.Clear();

    //    this.skeleton.GenerateSkeleton(robotJoints);
    //    this.skeleton.CalcPositionWithIK(testPointObj.transform.position, endEffectorDirection, newArmPosition, maximumFabrikPasses);

    //    StartCoroutine(ActiveRobots.MoveAxis_CR(true, newArmPosition, 1));
    //}

    public IEnumerator IKMoveToTargetArray()
    {
        int localCounter = 0;
        foreach (var wayPoint in wayPointManager.wayPoints())
        {
            yield return IKMoveToTarget(wayPoint.Position, wayPoint.Rotation);
            wayPointManager.RefreshIfDancing(localCounter, (localCounter == wayPointManager.transform.childCount - 1));
            localCounter++;
            yield return new WaitForSeconds(this.SecondsDelayBetweenTargets);
            // Do post action for the waypoints
            GizmoController.GizmoPostAction gpa = wayPoint.gizmocontroller.GetPoatAction;
            if (gpa == GizmoController.GizmoPostAction.ClawOpen)
            {
                yield return this.ActiveRobots.IO_InOut_CR(false, true);
            }
            else if (gpa == GizmoController.GizmoPostAction.ClawClose)
            {
                yield return this.ActiveRobots.IO_InOut_CR(false, false);
            }
            else if (gpa == GizmoController.GizmoPostAction.RotateLeft)
            {
                yield return StartCoroutine(ActiveRobots.MoveAxis_CR(false, leftRotateEE, 1));
            }
            else if (gpa == GizmoController.GizmoPostAction.RotateRight)
            {
                yield return StartCoroutine(ActiveRobots.MoveAxis_CR(false, rightRotateEE, 1));
            }
            yield return new WaitForSeconds(this.SecondsDelayBetweenTargets);
        }
    }

    public IEnumerator IKMoveToTarget(Vector3 target, Quaternion rotation)
    {
        this.skeleton.SetAngles(GetAngles());
        var newArmPosition = this.skeleton.CalculateIK(target, rotation);
        // TODO: Also need to take this into the IK process though
        newArmPosition = PositionRegulation(GetJoints(), newArmPosition);
        Debug.Log("Move To Position: " + String.Join(", ", newArmPosition.Select(i => i.ToString()).ToArray()));
        yield return StartCoroutine(ActiveRobots.MoveAxis_CR(true, newArmPosition, 1));
    }

    /// <summary>
    /// Generate a new position based on current position and target angles
    /// </summary>
    /// <param name="joints"></param>
    /// <param name="targetAngles"></param>
    /// <returns></returns>
    private float[] PositionRegulation(RobotArmJoint[] joints, float[] targetAngles)
    {
        // TODO: With multi path, we should have more options here
        Debug.Assert(joints.Length == targetAngles.Length);
        float[] newAngles = new float[joints.Length];
        for (int i = 0; i < joints.Length; i++)
        {
            newAngles[i] = BalanceAngle(joints[i], targetAngles[i]);
        }
        return newAngles;
    }

    private float BalanceAngle(RobotArmJoint robotArmJoint, float targetAngle)
    {
        var currentAngle = robotArmJoint.GetAngle();
        var dir = currentAngle < targetAngle ? 1 : -1;
        var angle = targetAngle;
        var diff = Mathf.Abs(currentAngle - targetAngle);
        var result = angle;
        if (dir == 1)
        {
            while (angle > Mathf.Max(currentAngle - 360, robotArmJoint.MinAngle))
            {
                angle -= dir * 360;
                var diff1 = Mathf.Abs(currentAngle - angle);
                if (diff > diff1)
                {
                    diff = diff1;
                    result = angle;
                }
            }
        }
        else
        {
            while (angle < Mathf.Min(currentAngle + 360, robotArmJoint.MaxAngle))
            {
                angle -= dir * 360;
                var diff1 = Mathf.Abs(currentAngle - angle);
                if (diff > diff1)
                {
                    diff = diff1;
                    result = angle;
                }
            }
        }
        return result;
    }

    private void SetRelativeAngle(int index, float angle)
    {
        var angles = this.skeleton.SetRelativeAngle(index, angle);
        StartCoroutine(ActiveRobots.MoveAxis_CR(true, angles, 1));
        //SetAnglesImmediate(angles);
    }

    public RotationPosition[] GetPath(RotationPosition from, RotationPosition to)
    {
        return this.skeleton.GetPath(from, to, 0.02f).ToArray();
    }

    public RotationPosition GetEndEffectorRotationPosition()
    {
        return RotationPosition.GetRotationPosition(this.EndEffectorGrabPoint.transform);
    }

    private void Update()
    {
        for (int i = 0; i < debugDrawLines.Count; i += 2)
        {
            Debug.DrawLine(debugDrawLines[i], debugDrawLines[i + 1], debugColors[i]);
        }
    }

    public void OnGUI()
    {
        if (!this.InAppDebugVisualsOn)
        {
            return;
        }

        float y = 0;
        for (int i = 0; i < this.GetNumberOfJoints(); i++)
        {
            GUI.color = Color.blue;
            if (GUI.Button(new Rect(5, y + 5, 18, 18), "+"))
            {
                SetRelativeAngle(i, 10);
            }
            if (GUI.Button(new Rect(24, y + 5, 18, 18), "-"))
            {
                SetRelativeAngle(i, -10);
            }
            GUI.Label(new Rect(48, y + 5, 800, 25), this.skeleton.GetDebugInfo(i));
            y += 20;
        }
    }

    void LogExecutionLine(string message = "")
    {
        if (RobotProgramText != null)
        {
            RobotProgramText.text += (message + "\n");
        }
    }
}