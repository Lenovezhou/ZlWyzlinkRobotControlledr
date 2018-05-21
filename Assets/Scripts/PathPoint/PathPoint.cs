using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathPoint : MonoBehaviour
{
    [SerializeField]
    bool ClawOpen;
    [SerializeField]
    bool ClawClose;
    [SerializeField]
    bool RotateLeft;
    [SerializeField]
    bool RotateRight;
    [SerializeField]
    float duration;
    [SerializeField]
    float speed;
    [SerializeField]
    Vector3 forward;
    private GizmoController.GizmoPostAction gizmopostaction;
    public GizmoController.GizmoPostAction GetGizmopostaction
    {
        get
        {
            return ResetPostAction();
        }
        set
        {
            gizmopostaction = value;
            RemindChoise(value);
        }
    }

    public float Duration
    {
        get
        {
            return duration;
        }

        set
        {
            duration = value;
        }
    }

    public float Speed
    {
        get
        {
            return speed;
        }

        set
        {
            speed = value;
        }
    }

    public Vector3 Forward
    {
        get { return forward; }
        set { forward = value; }
    }

    private void RemindChoise(GizmoController.GizmoPostAction gp)
    {
        ClawOpen = false;
        ClawClose = false;
        RotateLeft = false;
        RotateRight = false;
        switch (gp)
        {
            case GizmoController.GizmoPostAction.None:
                break;
            case GizmoController.GizmoPostAction.ClawOpen:
                ClawOpen = true;
                break;
            case GizmoController.GizmoPostAction.ClawClose:
                ClawClose = true;
                break;
            case GizmoController.GizmoPostAction.RotateLeft:
                RotateLeft = true;
                break;
            case GizmoController.GizmoPostAction.RotateRight:
                RotateRight = true;
                break;
            default:
                break;
        }
    }


    private GizmoController.GizmoPostAction ResetPostAction()
    {
        GizmoController.GizmoPostAction ggaction = GizmoController.GizmoPostAction.None;
        if (ClawOpen)
        {
            ggaction = GizmoController.GizmoPostAction.ClawOpen;
        }
        if (ClawClose)
        {
            ggaction = GizmoController.GizmoPostAction.ClawClose;
        }
        if (RotateLeft)
        {
            ggaction = GizmoController.GizmoPostAction.RotateLeft;
        }
        if (RotateRight)
        {
            ggaction = GizmoController.GizmoPostAction.RotateRight;
        }
        return ggaction;
    }

}

public enum ArmPath { None,Home,Dancing,GetBottle}


