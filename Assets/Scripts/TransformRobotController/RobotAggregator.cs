using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[RequireComponent(typeof(RobotControllerSimple))]
[RequireComponent(typeof(IRobotControl))]
public class RobotAggregator : MonoBehaviour
{
    private RobotControllerSimple localRobot;
    private IRobotControl remoteRobot;

    [SerializeField]
    UnityEngine.UI.Text RobotProgramText;

    private SyncedRobotController syncedRobot;

    private void Start()
    {
        localRobot = GetComponent<RobotControllerSimple>();
        Debug.Assert(localRobot != null);
        remoteRobot = GetComponent<IRobotControl>();
        Debug.Assert(remoteRobot != null);
    }

    public void SetSyncedRobotController(SyncedRobotController syncedRobotController)
    {
        this.syncedRobot = syncedRobotController;
    }

    public void GetJointAngle(int jointIndex, Action<float> callback)
    {
        if (remoteRobot.IsConnected())
        {
            remoteRobot.GetJointAngle(jointIndex, callback);
        }
    }

    public void GetNumberOfJoints(Action<int> callback)
    {
        throw new NotImplementedException();
    }

    public void SetMaxSpeeds(float[] newMaxSpeeds)
    {
        for (int i = 0; i < localRobot.GetJoints().Length; i++)
        {
            localRobot.GetJoints()[i].SetCurrentMaxSpeed(newMaxSpeeds[i]);
        }
    }

    ///////////// Aggregator functions
    public IEnumerator SetMaxAngleSpeeds_CR(float[] newMaxSpeeds)
    {
        Coroutine c1;
        if (remoteRobot.IsConnected())
        {
            c1 = remoteRobot.SetMaxAngleSpeeds(newMaxSpeeds);
        }
        else
        {
            c1 = StartCoroutine(Extensions.EmptyCoroutine());
        }
        this.SetMaxSpeeds(newMaxSpeeds);
        yield return c1;
    }
    public IEnumerator MoveJointBySpeed_CR(int jointID, float relativeAngle, float speed)
    {
        Coroutine c1;
        if (remoteRobot.IsConnected())
        {
            c1 = remoteRobot.MoveJointBySpeed(jointID, relativeAngle, speed);
        }
        else
        {
            c1 = StartCoroutine(Extensions.EmptyCoroutine());
        }
        var c2 = localRobot.GetJoints()[jointID].MoveAngleWithSpeed(false, relativeAngle, speed); // relative angle
        yield return c1;
        yield return c2;
    }

    public void GetSynchronizedSpeeds(float[] speedResults, float[] angles, float speed)
    {
        // need to find how long it will take the slowest joint movement
        float longest = 0;
        for (int i = 0; i < localRobot.GetJoints().Length; i++)
        {
            float angleDist = Mathf.Abs(localRobot.GetJoints()[i].GetAngle() - angles[i]);
            float angularSpeed = localRobot.GetJoints()[i].GetCurrentMaxSpeed() * speed;
            float time = angleDist / angularSpeed;

            if (time > longest)
            {
                longest = time;
            }
        }

        // now we need to find all other angle speeds so that they take the same amount of time as longest
        for (int i = 0; i < localRobot.GetJoints().Length; i++)
        {
            float angleDist = Mathf.Abs(localRobot.GetJoints()[i].GetAngle() - angles[i]);
            float angularSpeed = localRobot.GetJoints()[i].GetCurrentMaxSpeed() * speed;
            float time = angleDist / angularSpeed;

            speedResults[i] = time / longest;
        }
    }

    public IEnumerator MoveAxis_CR(bool absOrRel, float[] angles, float speed)
    {
        Debug.Assert(localRobot != null);

        Coroutine[] crs = new Coroutine[angles.Length + 1];
        float[] speeds = new float[localRobot.GetJoints().Length];
        // try to find speeds that will make joint movements end together
        GetSynchronizedSpeeds(speeds, angles, speed);
        // to remove the synchronized joint movement: comment out above function and just pass 'speed' below
        for (int i = 0; i < localRobot.GetJoints().Length; i++)
        {
            crs[i] = localRobot.GetJoints()[i].MoveAngleWithSpeed(absOrRel, angles[i], speeds[i]);
        }

        if (remoteRobot.IsConnected())
        {
            crs[crs.Length - 1] = remoteRobot.MoveAxis(angles, speed);
        }
        else
        {
            crs[crs.Length - 1] = StartCoroutine(Extensions.EmptyCoroutine());
        }
        for (int i = 0; i < crs.Length; i++)
        {
            yield return crs[i];
        }
    }
    public IEnumerator Home_CR()
    {
        Coroutine c1;
        if (remoteRobot.IsConnected())
        {
            c1 = remoteRobot.Home();
        }
        else
        {
            c1 = StartCoroutine(Extensions.EmptyCoroutine());
        }
        yield return MoveAxis_CR(true, new float[] { 0, -90, 0, -90, 0, 0 }, 1.0f);
        yield return c1;
    }

    public IEnumerator MotorOnOff_CR(bool onoff)
    {
        Coroutine c1;
        if (remoteRobot.IsConnected())
        {
            c1 = remoteRobot.SendMotorOnOff(onoff);
        }
        else
        {
            c1 = StartCoroutine(Extensions.EmptyCoroutine());
        }
        yield return c1;
    }
    public IEnumerator IO_InOut_CR(bool inout, bool onoff)
    {
        Coroutine c1;
        if (remoteRobot.IsConnected())
        {
            SetClawColor(onoff);
            c1 = remoteRobot.SendIOCommand(inout, onoff);
        }
        else
        {
            c1 = StartCoroutine(Extensions.EmptyCoroutine());
        }
        yield return c1;
    }

    public void SetClawColor(bool onoff)
    {
        MeshRenderer[] models = localRobot.ClawModel.GetComponentsInChildren<MeshRenderer>();
        for (int i = 0; i < models.Length; i++)
        {
            if (models[i].material.color != null)
            {
                if (onoff == false) // closed
                {
                    models[i].material.color = Color.red;
                }
                else
                {
                    models[i].material.color = Color.white;
                }
            }
        }
    }

    public IEnumerator GetAxisAngle_CR(Action<float[]> callback)
    {
        Coroutine c1;
        if (remoteRobot.IsConnected())
        {
            c1 = remoteRobot.GetAxisAngle(callback);
        }
        else
        {
            c1 = StartCoroutine(Extensions.EmptyCoroutine());
        }
        yield return c1;
    }

    public void MoveJointBySpeed(int jointIndex, float targetAngle, float speed)
    {
        throw new NotImplementedException();
    }
    
    public ProgramResult RunProgramLine(string programLine)
    {
        if (localRobot.IsRunning)
        {
            return new ProgramResult(true, false, "Program is busy");
        }
        localRobot.IsRunning = true;
        var programResult = new ProgramResult();
        localRobot.RunningProgramLine = programLine;
        StartCoroutine(RunProgramLine(programLine, programResult));
        return programResult;
    }

    // TODO: This function doesn't alow any exceptions. Need try catch and better structures
    public IEnumerator RunProgramLine(string programLine, ProgramResult programResult)
    {
        string[] tokens = programLine.Split();
        if (!string.IsNullOrEmpty(tokens[0]))
        {
            switch (tokens[0])
            {
                case "HOME":
                    LogExecutionLine(tokens[0]);
                    yield return StartCoroutine(Home_CR());
                    break;
                case "GetAxisAngle":
                    LogExecutionLine(tokens[0]);
                    yield return StartCoroutine(GetAxisAngle_CR((anglesReturned) => { Debug.Log("Remote axis angles: " + anglesReturned); }));
                    break;
                case "MOTO_ON":
                    LogExecutionLine(tokens[0]);
                    yield return StartCoroutine(MotorOnOff_CR(true));
                    break;
                case "MOTO_OFF":
                    LogExecutionLine(tokens[0]);
                    yield return StartCoroutine(MotorOnOff_CR(false));
                    break;
                ////////////////////
                case "IO_IN":
                    LogExecutionLine(tokens[0]);
                    yield return StartCoroutine(IO_InOut_CR(true, false));
                    break;
                case "IO_OUT":
                    LogExecutionLine(tokens[0]);
                    yield return StartCoroutine(IO_InOut_CR(false, bool.Parse(tokens[1])));
                    break;
                ////////////////////
                case "SetMaxAngleSpeed":
                    yield return StartCoroutine(SetMaxAngleSpeeds_CR(GetAxisParams(GetParamsSetMaxAngle(tokens), 2)));
                    break;
                case "SetMaxSpeed":
                    LogExecutionLine("[command not implemented]");
                    break;
                ////////////////////
                case "MoveAbs":
                    LogExecutionLine("[command not implemented]");
                    break;
                case "MoveRel":
                    LogExecutionLine("[command not implemented]");
                    break;
                ////////////////////
                case "MoveAxisAbs":
                    LogExecutionLine(tokens[0]);
                    yield return StartCoroutine(MoveAxis_CR(true, GetAxisParams(tokens, 1), 1));
                    break;
                case "MoveAxisRel":
                    LogExecutionLine(tokens[0]);
                    yield return StartCoroutine(MoveAxis_CR(false, GetAxisParams(tokens, 1), 1));
                    break;
                case "IKMoveToTarget":
                    LogExecutionLine(tokens[0]);
                    yield return localRobot.IKMoveToTargetArray();
                    break;
                default:
                    LogExecutionLine("Invalid Command:" + tokens[0]);
                    break;
            }
        }
        programResult.isDone = true;
        programResult.IsSucceeded = true;
        //
        localRobot.IsRunning = false;
        localRobot.RunningProgramLine = "";
        if (syncedRobot != null)
        {
            syncedRobot.UpdateSyncedProgramLine("");
        }
    }

    public float[] GetAxisParams(string[] tokens, int preceedingTokens)
    {
        var axisParams = new float[localRobot.RobotJoints.Length];
        StringBuilder sb = new StringBuilder();
        sb.Append(tokens[0]);
        Debug.Assert(tokens.Length == localRobot.RobotJoints.Length + preceedingTokens);
        for (int j = 1; j <= localRobot.RobotJoints.Length; j++)
        {
            axisParams[j - 1] = float.Parse(tokens[j]);
            sb.Append(" ").Append(tokens[j]);
        }
        LogExecutionLine(sb.ToString());
        return axisParams;
    }

    public string[] GetParamsSetMaxAngle(string[] tokens)
    {
        if (tokens.Length == 3)
        {
            var axisParams = new string[localRobot.GetNumberOfJoints() + 2];
            axisParams[0] = tokens[0];
            axisParams[1] = tokens[1];
            axisParams[2] = tokens[2];
            for (int j = 3; j < axisParams.Length; j++)
            {
                axisParams[j] = "0";
            }
            return axisParams;
        }
        else
        {
            return tokens;
        }
    }

    public void LogExecutionLine(string message = "")
    {
        if (RobotProgramText != null)
        {
            RobotProgramText.text += (message + "\n");
        }
    }
}

class Coroutine<T>
{
    public T getResult;
    public Coroutine coroutine;
}
