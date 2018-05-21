using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public class RemoteRobotController : MonoBehaviour, IRobotControl
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    private TcpClient clientSocket = new TcpClient();
    private NetworkStream stream = null;
    public bool isConnected = false;
    public bool syncOn = false;
    public int NumberOfJoints = 6;          // TODO: Need to get the parameters from actual robot arm or robot arm config

    private string newline = "\r\n";

    public bool isSyncOn()
    {
        return this.syncOn;
    }

    public void ConnectToRemoteRobot(string ipAddress, string port, Action<bool, string> statusCallback)
    {
        try
        {
            Debug.Log("Connecting to server " + ipAddress + ":" + port);
            Debug.Assert(isConnected == false);
            Debug.Assert(syncOn == false);
            IPAddress address = IPAddress.Parse(ipAddress);
            int portNum = Int32.Parse(port);
            clientSocket.BeginConnect(address, portNum, (IAsyncResult socketResult) => {
                if (socketResult.IsCompleted)
                {
                    if (clientSocket.Client.Connected)
                    {
                        isConnected = true;
                        this.stream = clientSocket.GetStream();
                        MainThreadDispatcher.QueueTask(() =>
                        {
                            SetSyncOn(true);
                            statusCallback(true, "Status:\nConnected");
                        });
                        Debug.Log("Connected.");
                    }
                    else
                    {
                        Debug.Log("Connection to server failed.");
                        MainThreadDispatcher.QueueTask(() =>
                        {
                            statusCallback(false, "Status:\nConnection to\nserver failed.");
                        });
                    }
                    // Start connecting
                    // Retrieving number of joints
                }
                else
                {
                    Debug.Log("Not able to connect");
                    MainThreadDispatcher.QueueTask(() =>
                    {
                        statusCallback(false, "Status:\nNot able\nto connect");
                    });
                }
                clientSocket.EndConnect(socketResult);
            }, null);
        }
        catch (Exception ex)
        {
            Debug.Log("Failed to connect to " + ipAddress + ex.Message);
            MainThreadDispatcher.QueueTask(() =>
            {
                statusCallback(false, "Failed to\nconnect to\n" + ipAddress + "\n" + ex.Message);
            });
        }
    }

    private void OnDestroy()
    {
        if (this.syncOn)
        {
            this.syncOn = false;
            this.stream.BeginWrite("MOTO_OFF" + newline);
        }
        if (this.isConnected)
        {
            this.isConnected = false;
            this.clientSocket.Close();
        }
    }

    internal void SetSyncOn(bool syncOnValue)
    {
        if (this.syncOn && !syncOnValue)
        {
            SendMotorOnOff(false);
            this.syncOn = false;
        }
        else if (!this.syncOn && syncOnValue)
        {
            this.syncOn = true;
            SendMotorOnOff(true);
        }
        else
        {
            Debug.LogWarning("Invalid status setting: SyncOn was " + this.syncOn);
        }
    }

    public bool IsConnected()
    {
        return isConnected;
    }

    public void GetJointAngle(int jointIndex, Action<float> callback)
    {
        Debug.Assert(false);        // TODO: Should be removed
        Debug.Assert(isConnected);
        Debug.Assert(jointIndex < this.NumberOfJoints);
        this.stream.BeginWrite("GetAngle: " + jointIndex);
        this.stream.BeginRead((result) => { callback(float.Parse(result)); });
    }

    public void GetNumberOfJoints(Action<int> callback)
    {
        Debug.Assert(false);        // TODO: Should be removed
        if (!isConnected)
        {
            throw new InvalidOperationException("The remote robot is not connected");
        }

        this.stream.BeginWrite("JointCount");
        this.stream.BeginRead((result) => { callback(Int32.Parse(result)); });
    }

    // if sync is not on, return empty coroutine instead of the send coroutine
    private Coroutine StartSend(string data, Action<string> callback = null)
    {
        if (!syncOn)
        {
            return StartCoroutine(Extensions.EmptyCoroutine());
        }

        if (callback == null)
        {
            callback = (string str) => { };
        }
        return StartCoroutine(SendDataToRemote_CR(data, callback));
    }

    private IEnumerator SendDataToRemote_CR(string data, Action<string> callback)
    {
        Debug.Assert(isConnected);
        //Debug.Log("Sending data: " + data);
        this.stream.BeginWrite(data + newline);

        bool received = false;

        this.stream.BeginRead((result) => {
            //Debug.Log("Received data: " + result);

            if (callback != null)
            {
                if (string.Compare(result, "END_FLAG") == 0)
                {
                    Debug.Log("Got END_FLAT@1. Finished.");
                    callback(result);
                    received = true;
                }
                else
                {
                    this.stream.BeginRead((result2) => {
                        if (string.Compare(result, "END_FLAG") == 0)
                        {
                            Debug.Log("Got END_FLAT@2. Finished.");
                            callback(result);
                            received = true;
                        }
                        else
                        {
                            Debug.Log("Received no END_FLAG for second read");
                            received = false;
                        }
                    });
                }
            }
        });

        while (received != true)
        {
            yield return -1;
        }
        Debug.Log("Confirmed. Existing the call");
    }
    
    ///////////// Start Send Coroutine functions //////////////////////////
    public Coroutine SetMaxAngleSpeeds(float[] newMaxSpeeds)
    {
        string data = ConvertToRemoteFormatSetMaxAngleSpeed(newMaxSpeeds);
        return StartSend(data);
    }
    public Coroutine MoveJointBySpeed(int jointIndex, float relativeAngle, float speed)
    {
        string data = ConvertToRemoteFormatSingleJoint(jointIndex, relativeAngle, NumberOfJoints);
        return StartSend(data);
    }
    public Coroutine MoveAxis(float[] absoluteAngles, float speed)
    {
        //Debug.Log("StartMoveAxis");
        string data = ConvertToRemoteFormatMoveAxis(true, absoluteAngles, this.NumberOfJoints);
        return StartSend(data);
    }
    public Coroutine Home()
    {
        return StartSend("HOME");
    }
    public Coroutine SendIOCommand(bool inout, bool onoff = false)
    {
        string data = "IO_";
        if (inout == true)
        {
            data += "IN, 4";
        }
        else
        {
            data += "OUT, 4, ";
            if (onoff == true)
            {
                data += "TRUE";
            }
            else
            {
                data += "FALSE";
            }
        }
        return StartSend(data);
    }
    public Coroutine SendMotorOnOff(bool state)
    {
        string data = "MOTO_ON";
        if (state == false)
        {
            data = "MOTO_OFF";
        }

        return StartSend(data);
    }
    public Coroutine GetAxisAngle(Action<float[]> callback)
    {
        string data = "GetAxisAngle";
        return StartSend(data, (result) => {
            string[] tokens = result.Split();
            float[] angles = new float[this.NumberOfJoints];
            if(tokens[0] == "GetAxisPos,")
            {
                int expectedTokens = 1 + this.NumberOfJoints;
                if (tokens.Length != expectedTokens)
                {
                    Debug.Log("Received wrong number of axis angles: " + tokens.Length + ". Expected: " + expectedTokens);
                }
                else
                {
                    for (int i = 0; i < this.NumberOfJoints; i++)
                    {
                        // parse each token except for the last char in each (which should be a ",")
                        angles[i] = float.Parse(tokens[i + 1].Substring(0, tokens[i + 1].Length - 1));
                        Debug.Log("Got angles " + angles[0] + ", " + angles[1] + ", " + angles[2] + ", " + angles[3] + ", " + angles[4] + ", " + angles[5]);
                    }
                }
            }
            else
            {
                Debug.Log("Expected 'GetAxisPos,' but instead received: " + tokens[0]);
            }
            callback(angles);
        } );
    }

    ///////////// Convert format functions //////////////////////////
    private string ConvertToRemoteFormatSingleJoint(int jointIndex, float relativeAngle, int totalJoints)
    {
        return "a" + jointIndex + ", " + relativeAngle + ",,,,," + newline;
    }
    private string ConvertToRemoteFormatSetMaxAngleSpeed(float[] newMaxSpeeds)
    {
        string result = "setMaxAngleSpeed";
        for (int i = 0; i < this.NumberOfJoints; i++)
        {
            result += ", " + newMaxSpeeds[i];
        }
        return result + newline;
    }
    private string ConvertToRemoteFormatMoveAxis(bool absOrRel, float[] relativeAngles, int totalJoints)
    {
        string result = "MoveAxis";
        if(absOrRel == true)
        {
            result += "Abs";
        }
        else
        {
            result += "Rel";
        }
        for (int i = 0; i < totalJoints; i++)
        {
            result += ", " + -relativeAngles[i];
        }
        return result + newline;
    }

#else
    public int NumberOfJoints = 6;      // TODO: Hard code them here
    public bool IsConnected()
    {
        return false;
    }
    public bool isSyncOn()
    {
        return false;
    }
    public void SetSyncOn(bool syncOn)
    {
        Debug.LogError("Invalid call if not on PC platform");
    }
    public void ConnectToRemoteRobot(string ipAddress, string port, Action<bool, string> statusCallback)
    {
        statusCallback(false, "Not enabled");
    }
    public Coroutine MoveJointBySpeed(int jointIndex, float relativeAngle, float speed)
    {
        Debug.LogError("Invalid call if not on PC platform");
        return null;
    }
    public Coroutine MoveAllJointsBySpeed(float[] relativeAngles, float speed)
    {
        Debug.LogError("Invalid call if not on PC platform");
        return null;
    }
    public Coroutine MoveAxis(float[] relativeAngles, float speed)
    {
        Debug.LogError("Invalid call if not on PC platform");
        return null;
    }
    public Coroutine SetMaxAngleSpeeds(float[] newMaxSpeeds)
    {
        Debug.LogError("Invalid call if not on PC platform");
        return null;
    }
    public Coroutine GetAxisAngle(Action<float[]> callback)
    {
        Debug.LogError("Invalid call if not on PC platform");
        return null;
    }
    public Coroutine Home()
    {
        Debug.LogError("Invalid call if not on PC platform");
        return null;
    }
    public Coroutine SendMotorOnOff(bool state)
    {
        Debug.LogError("Invalid call if not on PC platform");
        return null;
    }
    public void GetJointAngle(int jointIndex, Action<float> callback)
    {
        Debug.LogError("Invalid call if not on PC platform");
    }
    public Coroutine SendIOCommand(bool inout, bool onoff = false)
    {
        Debug.LogError("Invalid call if not on PC platform");
        return null;
    }
#endif
}
