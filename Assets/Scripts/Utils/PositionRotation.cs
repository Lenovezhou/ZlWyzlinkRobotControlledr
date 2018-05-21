using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PositionRotation
{
    public Vector3 Position { get; private set; }
    public Quaternion Rotation { get; private set; }
    public Vector3 Forward { get; private set; }
    public GizmoController gizmocontroller;

    public PositionRotation(Vector3 position, Quaternion rotation, GizmoController gc = null)
    {
        this.Forward = Vector3.zero;
        this.Position = position;
        this.Rotation = rotation;
        this.gizmocontroller = gc;
    }

    public PositionRotation(Vector3 postion, Vector3 forward, GizmoController gc = null)
    {
        this.Forward = forward;
        this.Position = postion;
        this.Rotation = Quaternion.identity;
        this.gizmocontroller = gc;
    }

}
