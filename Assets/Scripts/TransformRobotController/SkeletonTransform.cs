using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonTransform
{
    private float angle;
    private readonly int index;
    private readonly float length;

    // Whether there is an offset that the axis is point away from operational plane
    private readonly float pointingOffset;
    private readonly Quaternion linkRotation;
    private readonly Vector3 normal;
    private readonly Vector3 pointing;
    private readonly SkeletonTransform parent;
    private readonly RobotSkeleton skeleton;
    private readonly float minAngle;
    private readonly float maxAngle;
    private readonly bool isSymmetrical;
    public readonly bool isWristPoint;
    private SkeletonTransform child;

    // For the straight angle, we will use two joints as one joint since they
    // could not bend. compoundLength and compoundDiffAngle are the combine data
    // for a joint to its sub straight joint
    private float compoundLength = 0;
    private float compoundDiffAngle = 0;

    private const float spinJointAngleThreshold = 1;

    public SkeletonTransform(RobotSkeleton skeleton, SkeletonTransform parent, int i, RobotArmJoint joint, Vector3 jointNextPosition)
    {
        this.skeleton = skeleton;
        this.parent = parent;
        this.index = i;

        // In the case that the joint is not started from 0, we need to compensate based on the current angle
        this.angle = joint.GetAngle();
        this.normal = InverseTransformDirection(Quaternion.Inverse(skeleton.transform.rotation) * joint.transform.up);               // Up will be the normal/axis
        var linkVector = Quaternion.Inverse(skeleton.transform.rotation) * joint.transform.TransformDirection(jointNextPosition);
        this.length = linkVector.magnitude;
        this.linkRotation = Quaternion.FromToRotation(Vector3.forward, Quaternion.AngleAxis(-joint.GetAngle(), this.normal) * InverseTransformDirection(linkVector));
        this.pointing = Quaternion.AngleAxis(-joint.GetAngle(), this.normal) * InverseTransformDirection(Quaternion.Inverse(skeleton.transform.rotation) * joint.transform.forward);        // Forward will be the pointing
        this.pointingOffset = joint.pointingOffset;

        this.minAngle = joint.MinAngle;
        this.maxAngle = joint.MaxAngle;
        this.isSymmetrical = joint.is180Symmetrical;
        this.isWristPoint = joint.isWristPoint;

        if (parent != null)
        {
            this.parent.SetChild(this);
        }

        this.compoundLength = this.length;
        this.compoundDiffAngle = 0.0f;
    }

    private Vector3 ChildLocalPosition { get { return GetChildLocalPosition(); } }
    public Vector3 EndPosition { get { return this.TransformPoint(GetChildLocalPosition()); } }
    public Quaternion EndRotation { get { return this.TransformRotation(Quaternion.AngleAxis(this.angle, this.normal)); } }
    public Vector3 Position { get { return this.TransformPoint(Vector3.zero); } }
    public Vector3 Normal { get { return TransformDirection(this.normal); } }
    public float Length { get { return this.length; } }

    public bool UpdateAngle(Vector3 targetVector)
    {
        bool reachedTarget;
        this.angle = Clamp(CalcAngle(InverseTransformDirection(targetVector)), this.minAngle, this.maxAngle, out reachedTarget);
        Debug.Assert(!float.IsNaN(this.angle));
        return reachedTarget;
    }

    private Vector3 GetChildLocalPosition()
    {
        var linkOriginVector = this.linkRotation * (Vector3.forward * length);
        var linkVector = Quaternion.AngleAxis(this.angle, this.normal) * linkOriginVector;
        return linkVector;
    }

    private float CalcAngle(Vector3 localEndVector)
    {
        Debug.Assert(!localEndVector.IsNaN());
        float angle;
        Vector3 rotationAxis;
        var linkOriginVector = this.pointing;
        DebugAngles(localEndVector, linkOriginVector);
        var mappedTargetVector = Vector3.ProjectOnPlane(localEndVector, this.normal);
        var rotation = Quaternion.FromToRotation(Vector3.ProjectOnPlane(linkOriginVector, this.normal), mappedTargetVector);
        rotation.ToAngleAxis(out angle, out rotationAxis);
        var targetAngle = Vector3.Angle(rotationAxis, this.normal) < 5.0f ? angle : -angle;
        if (this.pointingOffset != 0f)
        {
            targetAngle -= Mathf.Rad2Deg * Mathf.Asin(this.pointingOffset / mappedTargetVector.magnitude);
        }
        // Clamp the angle between -90 ~ +90 (for symmetrical) or -180 ~ +180
        var newAngle = isSymmetrical ? Mathf.Repeat(targetAngle + 90, 180) - 90 : Mathf.Repeat(targetAngle + 180, 360) - 180;
        Debug.Assert(!float.IsNaN(newAngle));
        return newAngle;
    }

    private void DebugAngles(Vector3 localEndVector, Vector3 linkOriginVector)
    {
        float lengthFactor = 0.08f;
        this.skeleton.DrawDebugRay(this.Position, TransformDirection(linkOriginVector) * lengthFactor, Color.cyan);
        this.skeleton.DrawDebugRay(this.Position, TransformDirection(localEndVector) * lengthFactor, Color.blue);
        this.skeleton.DrawDebugLine(this.Position + TransformDirection(linkOriginVector).normalized * 0.01f, this.Position + TransformDirection(localEndVector).normalized * 0.01f, Color.cyan);
    }

    public Vector3 GetLinkOriginVector()
    {
        return TransformDirection(this.linkRotation * (Vector3.forward * length));
    }

    public float GetAngle()
    {
        Debug.Assert(!float.IsNaN(this.angle));
        return this.angle;
    }

    public void SetAngle(float angle)
    {
        Debug.Assert(!float.IsNaN(this.angle));
        Debug.Assert(!float.IsNaN(angle));
        this.angle = angle;
    }

    public void SetChild(SkeletonTransform child)
    {
        this.child = child;
        if (child.IsSpinJoint())
        {
            var endVector = child.EndPosition - this.Position;
            this.compoundLength = endVector.magnitude;
            this.compoundDiffAngle = CalcAngle(InverseTransformDirection(endVector));
        }
    }

    public SkeletonTransform GetChild()
    {
        return this.child;
    }

    public void Rotate(float angle)
    {
        Debug.Assert(!float.IsNaN(this.angle));
        Debug.Assert(!float.IsNaN(angle));
        this.angle += angle;
        this.angle = Mathf.Repeat(this.angle + 180, 360) - 180;
    }

    public void RotateTo(float angle)
    {
        Debug.Assert(!float.IsNaN(angle));
        Debug.Assert(!float.IsNaN(this.angle));
        this.angle = angle;
    }

    private bool IsSpinJoint()
    {
        var endVector = this.linkRotation * (Vector3.forward * length);
        var angle = Vector3.Angle(this.normal, endVector);
        return angle < spinJointAngleThreshold || Mathf.Abs(angle - 180) < spinJointAngleThreshold;
    }

    public Vector3 ReachOutTo(Vector3 target, Quaternion rotation)
    {
        return target - (rotation * Vector3.forward).normalized * this.length;
    }

    public Vector3 ReachOutTo(Vector3 target, Vector3 baseVector, Quaternion targetRotation)
    {
        var v0 = baseVector - target;
        var planeNormal = Vector3.Cross(v0, Vector3.up);
        var v1 = Vector3.Cross(planeNormal, targetRotation * (IsSpinJoint() ? Vector3.left : Vector3.forward));
        return v1.normalized * this.length + target;
    }

    /// <summary>
    /// Set out to the target for IK algorithm
    /// </summary>
    public Vector3 ReachOutTo(Vector3 target)
    {
        if (IsSpinJoint())
        {
            return target;
        }
        var targetVector = target - Position;
        return Position + targetVector.normalized * (targetVector.magnitude - this.compoundLength);
    }

    /// <summary>
    /// Retract back to the previous joint but still pointing to the target. Update the angle of the skeleton
    /// </summary>
    public Vector3 RetractBackTo(Vector3 target, Vector3 backTo, out bool reachedTarget)
    {
        var targetVector = target - backTo;
        var newAngle = CalcAngle(InverseTransformDirection(targetVector)) - this.compoundDiffAngle;
        this.angle = Clamp(newAngle, this.minAngle, this.maxAngle, out reachedTarget);
        Debug.Assert(!float.IsNaN(this.angle));
        return TransformPoint(ChildLocalPosition);
    }

    /// <summary>
    /// Transform a vector to the world coordinates
    /// </summary>
    public Vector3 TransformPoint(Vector3 position)
    {
        if (this.parent != null)
        {
            var newPosition = this.parent.TransformPoint(Quaternion.AngleAxis(this.parent.angle, this.parent.normal) * position + this.parent.ChildLocalPosition);
            Debug.Assert(!newPosition.IsNaN());
            return newPosition;
        }
        Debug.Assert(!position.IsNaN());
        return position;
    }

    /// <summary>
    /// Transform a vector from the world coordinates to local coordinates
    /// </summary>
    public Vector3 InverseTransformPoint(Vector3 position)
    {
        if (this.parent != null)
        {
            return Quaternion.AngleAxis(-this.parent.angle, this.parent.normal) * this.parent.InverseTransformPoint(position) - this.parent.ChildLocalPosition;
        }
        return position;
    }

    /// <summary>
    /// Transform a direction to the world coordinates
    /// </summary>
    public Vector3 TransformDirection(Vector3 direction)
    {
        if (this.parent != null)
        {
            return this.parent.TransformDirection(Quaternion.AngleAxis(this.parent.angle, this.parent.normal) * direction);
        }
        return direction;
    }

    /// <summary>
    /// Transform a direction from world coordinates to local coordinates
    /// </summary>
    public Vector3 InverseTransformDirection(Vector3 direction)
    {
        if (this.parent != null)
        {
            return Quaternion.AngleAxis(-this.parent.angle, this.parent.normal) * this.parent.InverseTransformDirection(direction);
        }
        return direction;
    }

    /// <summary>
    /// Transform a quaternion to the world coordinates
    /// </summary>
    public Quaternion TransformRotation(Quaternion localRotation)
    {
        if (this.parent != null)
        {
            return this.parent.TransformRotation(
                Quaternion.AngleAxis(this.parent.angle, this.parent.normal) 
                * localRotation);
        }
        return localRotation;
    }

    /// <summary>
    /// Transform a quaternion from world coordinates to local coordinates
    /// </summary>
    public Quaternion InverseTransformRotation(Quaternion rotation)
    {
        if (this.parent != null)
        {
            return Quaternion.AngleAxis(-this.parent.angle, this.parent.normal) 
                * this.parent.InverseTransformRotation(rotation);
        }
        return rotation;
    }

    private float Clamp(float value, float minValue, float maxValue, out bool reached)
    {
        reached = false;
        if (value < minValue)
        {
            return minAngle;
        }
        else if (value > maxValue)
        {
            return maxAngle;
        }
        reached = true;
        return value;
    }

    public string GetDebugStr()
    {
        //var vec = Vector3.left;
        //Debug.DrawRay(this.Position, vec, Color.gray, 0f);
        //Debug.DrawRay(this.Position, TransformDirection(InverseTransformDirection(vec)), Color.white, 0f);

        return "" + this.index + (IsSpinJoint() ? "*" : "")
            + ": [" + this.linkRotation.eulerAngles.ToString() + "]"
            + " L" + length.ToString("0.000") + " @" + angle.ToString("0.0000")
            //+ " : " + CalcAngle(ChildLocalPosition).ToString("0.0000")
            //+ " - " + this.axis + " : " + this.GetAxis().ToString("0.000")
            //+ " ^" + this.compoundLength.ToString("0.00") + ", " + this.compoundDiffAngle.ToString("0.00")
            ;
    }
}
