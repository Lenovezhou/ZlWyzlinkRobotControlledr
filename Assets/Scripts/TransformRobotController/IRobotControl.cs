using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRobotControl{
    bool IsConnected();
    Coroutine Home();
    Coroutine SendMotorOnOff(bool onoff);
    Coroutine SendIOCommand(bool inout, bool onoff);
    Coroutine GetAxisAngle(Action<float[]> callback);
    void GetJointAngle(int joint, Action<float> callback);
    Coroutine SetMaxAngleSpeeds(float[] newMaxSpeeds);
    Coroutine MoveJointBySpeed(int jointIndex, float targetAngle, float speed);
    Coroutine MoveAxis(float[] angles, float speed);
}
