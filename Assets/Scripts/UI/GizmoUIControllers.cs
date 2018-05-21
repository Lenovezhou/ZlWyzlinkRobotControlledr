using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GizmoUIControllers : MonoBehaviour
{
    private RobotControllerSimple Robot;
    private RobotAggregator robotAggregator;
    private void Start()
    {
        RobotsMain.Instance.OnRobotReady += RobotsMain_OnRobotReady;
    }

    private void RobotsMain_OnRobotReady(RobotControllerSimple robot)
    {
        RobotsMain.Instance.OnRobotReady -= RobotsMain_OnRobotReady;

        this.Robot = robot;
        this.robotAggregator = this.Robot.GetComponent<RobotAggregator>();
        Debug.Assert(this.robotAggregator != null);
    }

    public void StartRobot()
    {
        //Robot.IKmoveToTarget(new Vector3(-1, -1, 1));=
        this.robotAggregator.RunProgramLine("IKMoveToTarget");
    }

    public void CreateGizmo()
    {
        WayPointManager.Instance.CreateWayPoint(new Vector3(-0.5f, 0.5f, 0));
    }

    public void ResetRobot()
    {
        this.robotAggregator.RunProgramLine("HOME");
    }

}
