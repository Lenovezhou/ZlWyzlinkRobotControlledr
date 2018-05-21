using UnityEngine;

public struct RotationPosition
{
    public Vector3 Position;
    public Quaternion Rotation;

    public RotationPosition(Quaternion rotation, Vector3 position)
    {
        Rotation = rotation;
        Position = position;
    }

    static public RotationPosition GetRotationPosition(Transform transform)
    {
        return new RotationPosition() { Rotation = transform.rotation, Position = transform.position };
    }
}
