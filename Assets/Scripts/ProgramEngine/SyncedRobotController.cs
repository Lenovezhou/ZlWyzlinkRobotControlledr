using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using SpectatorView;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncedRobotController : MonoBehaviour
{
    private RobotControllerSimple robot;
    private RobotAggregator robotAggregator;
    private SyncRobot syncModel;

	// Use this for initialization
	void Start () {
        SetParent();
        this.robot = this.transform.FindChildOfType<RobotControllerSimple>();
        Debug.Assert(this.robot != null);
        this.robotAggregator = this.robot.GetComponent<RobotAggregator>();
        Debug.Assert(this.robotAggregator != null);
        this.robotAggregator.SetSyncedRobotController(this);
        var accessor = GetComponent<DefaultSyncModelAccessor>();

        ResetCounter();

        // Notify back the robot is ready
        RobotsMain.Instance.UpdateRobotReady(this.robot);
	}

    public void UpdateSyncedProgramLine(string programLine)
    {
        //if (this.syncModel != null)
        //{
        //    this.syncModel.syncProgramLine.Value = programLine;
        //}
    }


    public void SetParent()
    {
        transform.SetParent(UNetAnchorManager.Instance.transform, true);
    }

    public void ResetCounter()
    {
        if (this.syncModel != null)
        {
            this.syncModel.syncCounter.Value = int.MaxValue;
        }
    }

    public void SetCounter(int counter)
    {
        if (this.syncModel != null)
        {
            this.syncModel.syncCounter.Value = counter;
        }
    }

    public int GetCounter()
    {
        if (this.syncModel != null)
        {
            return this.syncModel.syncCounter.Value;
        }
        return int.MaxValue;
    }

    public bool IsCounterValid()
    {
        return this.syncModel != null && this.syncModel.syncCounter.Value != int.MaxValue ? true : false;
    }

    public void SetRunningState(bool runningState)
    {
        if (this.syncModel != null)
        {
            this.syncModel.isSystemRunning.Value = runningState;
        }
    }

    public bool IsSystemRunning()
    {
        return this.syncModel != null && this.syncModel.isSystemRunning.Value;
    }


    public void SetDacncingState(bool dancingstate)
    {
        if (this.syncModel != null)
        {
            this.syncModel.isSystemDancing.Value = dancingstate;
        }
    }


    public bool IsSystemDancing()
    {
        return this.syncModel != null && this.syncModel.isSystemDancing.Value;
    }
}
