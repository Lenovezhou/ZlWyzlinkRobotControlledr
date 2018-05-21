using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// The joint rotation follows the right hand rotation
public class RobotSkeleton : MonoBehaviour {
    public bool debugVisual = false;

    private const int maxRounds = 30;
    private const float hitAccuracy = 0.001f;      // TODO: This should be scaled by the root transform
    private const int stableCountMax = 5;

    private Vector3 IKTargetForDebug = Vector3.zero;
    private Vector3 IKDirectionForDebug = Vector3.zero;
    private Vector3 IKLeftDebug = Vector3.zero;
    private Vector3 IKRightDebug = Vector3.zero;

    private SkeletonTransform root; // joint skeleton transforms
    private Vector3 rootPosition;
    private int JointCount;

    public void InitializeSkeleton(RobotControllerSimple robotController)
    {
        var joints = robotController.GetJoints();
        this.JointCount = joints.Length;
        this.rootPosition = this.transform.InverseTransformPoint(joints[0].transform.position);
        this.root = GenerateSkeleton(joints, robotController.EndEffectorGrabPoint);
    }

    public SkeletonTransform GenerateSkeleton(RobotArmJoint[] joints, Transform endEffectorGrabPoint)
    {
        SkeletonTransform parent = null;
        SkeletonTransform root = null;
        int i;
        for (i = 0; i < joints.Length - 1; i++)
        {
            var skeletonTransform = new SkeletonTransform(this, parent, i, joints[i], joints[i + 1].transform.localPosition);
            parent = skeletonTransform;

            if (root == null)
            {
                root = skeletonTransform;
            }
        }
        var endEffector = new SkeletonTransform(this, parent, i, joints[i], endEffectorGrabPoint.localPosition);
        return root;
    }

    internal Vector3 CalcEndEffectorPosition(float[] anglesBuffer)
    {
        var t = this.root;
        foreach (var angle in anglesBuffer)
        {
            t.SetAngle(angle);
            t = t.GetChild();
        }
        return GetEndEffectorPosition() + this.rootPosition;
    }

    public float[] SetRelativeAngle(int index, float angle)
    {
        var t = this.root;
        while (t != null)
        {
            if (index == 0)
            {
                t.Rotate(angle);
                break;
            }
            index--;
            t = t.GetChild();
        }
        return GetAngles();
    }

    // TODO: Use indexed arrays with linked list
    public void SetAngles(float[] angles)
    {
        var t = this.root;
        int i = 0;
        while (t != null)
        {
            t.RotateTo(angles[i]);
            t = t.GetChild();
            i++;
        }
        Debug.Assert(i == angles.Length);
    }

    public void ResetSkeleton()
    {
        var t = this.root;
        while (t != null)
        {
            t.RotateTo(0);
            t = t.GetChild();
        }
    }

    public Vector3 GetEndEffectorPosition()
    {
        // TODO: We could reference the end effector directly
        Vector3 endEffectorPosition = Vector3.zero;
        var t = this.root;
        while (t != null)
        {
            endEffectorPosition = t.EndPosition;
            t = t.GetChild();
        }
        return endEffectorPosition;
    }

    public RotationPosition GetEndEffectorRotationPosition()
    {
        Vector3 endEffectorPosition = Vector3.zero;
        var t = this.root;
        while (t != null && t.GetChild() != null)
        {
            t = t.GetChild();
        }
        Debug.Assert(t != null);
        return new RotationPosition() {
            Rotation = this.transform.rotation * t.EndRotation,
            Position = this.transform.TransformPoint(t.EndPosition + rootPosition)
        };
    }

    public float[] GetAngles()
    {
        var angles = new float[this.JointCount];
        var node = this.root;
        for (int i = 0; i < this.JointCount; i++)
        {
            angles[i] = node.GetAngle();
            node = node.GetChild();
        }
        return angles;
    }

    public float[] CalculateIK(Vector3 targetPosition, Quaternion targetRotation)
    {
        var target = this.transform.InverseTransformPoint(targetPosition);
        var rotation = Quaternion.Inverse(this.transform.rotation) * targetRotation;
        this.IKTargetForDebug = target;
        this.IKDirectionForDebug = rotation * (Vector3.forward * 0.2f / this.transform.localScale.magnitude);
        this.IKLeftDebug = target + rotation * (Vector3.left * 0.05f / this.transform.localScale.magnitude);
        this.IKRightDebug = target + rotation * (Vector3.right * 0.05f / this.transform.localScale.magnitude);

        var list = GetTransformList();
        ApplyAngles(list, target - this.rootPosition, rotation);
        return GetAngles();
    }

    private bool ApplyAngles(List<SkeletonTransform> list, Vector3 target, Quaternion targetRotation)
    {
        bool reachedTarget = false;
        int nextJointIndex;
        var wristTarget = GetWristTarget(list, target, targetRotation, out nextJointIndex);

        //var wristTarget = target - targetRotation * Vector3.forward * (list[5].Length + list[4].Length);
        DrawDebugLine(wristTarget, target, Color.blue);

        if (!TryApplyAngles(list, target, targetRotation, wristTarget, nextJointIndex))
        {
            //TryApplyAngles(list, target, targetRotation, wristTarget, 3);
        }

        return reachedTarget;
    }

    private Vector3 GetWristTarget(List<SkeletonTransform> list, Vector3 target, Quaternion targetRotation, out int nextJointIndex)
    {
        var length = list.Count;
        nextJointIndex = length - 3;
        return list[length - 2].ReachOutTo(list[length - 1].ReachOutTo(target, targetRotation), targetRotation);
    }

    private bool TryApplyAngles(List<SkeletonTransform> list, Vector3 target, Quaternion targetRotation, Vector3 wristTarget, int ikJointStart)
    {
        bool reachedTarget = false;
        int roundCounter = 0;
        int index = ikJointStart;
        if (list[0].UpdateAngle(wristTarget - list[0].Position))
        {
            Vector3 target3;
            if (list[index].isWristPoint)
            {
                target3 = target;
            }
            else
            {
                // Move forward to the next joint to start IK
                target3 = wristTarget;
                wristTarget = list[index].ReachOutTo(wristTarget, list[1].Position, targetRotation);
                index--;
            }
            reachedTarget = FitSkeleton(list, wristTarget, index, ref roundCounter);

            // Update wrist angles (Joints length-3, length-2, length-1)
            if (list[ikJointStart].UpdateAngle(target3 - list[ikJointStart].Position))
            {
                if (list[ikJointStart + 1].UpdateAngle(target - list[ikJointStart + 1].Position))
                {
                    if (list[ikJointStart + 2].UpdateAngle(targetRotation * Vector3.up))
                    {
                        reachedTarget = reachedTarget && true;
                    }
                }
            }
        }
        Debug.Log("Finished IK @round " + roundCounter + " Delta:" + Vector3.Distance(GetEndEffectorPosition(), target));
        return reachedTarget;
    }

    private bool FitSkeleton(List<SkeletonTransform> list, Vector3 wristTarget, int ikJointStart, ref int roundCounter)
    {
        // Reaching to wrist point
        bool reachedTarget = false;
        int stableCounter = 0;
        var target0 = Vector3.positiveInfinity;
        while (roundCounter < maxRounds && Vector3.Distance(wristTarget, target0) > hitAccuracy && stableCounter < stableCountMax)
        {
            var tempTarget = FitJointAngles(list, ikJointStart, wristTarget, out reachedTarget);
            if (Vector3.Distance(target0, tempTarget) < hitAccuracy * 0.1f)
            {
                stableCounter++;
            }
            else
            {
                stableCounter = 0;
            }
            target0 = tempTarget;
            roundCounter++;
        }
        return reachedTarget && Vector3.Distance(wristTarget, target0) <= hitAccuracy;
    }

    private Vector3 FitJointAngles(List<SkeletonTransform> list, int index, Vector3 target, out bool reachedTarget)
    {
        if (index == 0)
        {
            // Actually we don't move joint 0 here
            reachedTarget = true;
            return list[index + 1].Position;
        }
        var t0 = list[index].ReachOutTo(target);
        DrawDebugLine(t0, target, Color.grey);
        var t1 = FitJointAngles(list, index - 1, t0, out reachedTarget);
        var t2 = list[index].RetractBackTo(target, t1, out reachedTarget);
        DrawDebugLine(t1, t2, Color.yellow);
        return t2;
    }

    internal IEnumerable<RotationPosition> GetPath(RotationPosition from, RotationPosition to, float resolution)
    {
        var endAngles = this.CalculateIK(to.Position, to.Rotation);
        var startAngles = this.CalculateIK(from.Position, from.Rotation);
        if (float.IsNaN(startAngles[0]))
        {
            startAngles = new float[] { 0, -90, 0, -90, 0, 0 };
        }
        for (float i = 0; i < 1; i += resolution)
        {
            this.SetAngles(Lerp(startAngles, endAngles, i));
            yield return this.GetEndEffectorRotationPosition();
        }
        this.SetAngles(endAngles);
        yield return this.GetEndEffectorRotationPosition();
    }

    float[] Lerp(float[] from, float[] to, float t)
    {
        Debug.Assert(from != null && to != null && from.Length == to.Length);
        float[] result = new float[from.Length];
        for (int i = 0; i < from.Length; i++)
        {
            result[i] = Mathf.LerpAngle(from[i], to[i], t);
        }
        return result;
    }

    private List<SkeletonTransform> GetTransformList()
    {
        var t = this.root;
        var list = new List<SkeletonTransform>();
        while (t != null)
        {
            list.Add(t);
            t = t.GetChild();
        }

        return list;
    }

    private void Update()
    {
        if (!debugVisual)
        {
            return;
        }
        RenderSkeleton();
    }

    public void RenderSkeleton()
    {
        var t = this.root;
        while (t != null)
        {
            LineDrawer.DrawSkeleton(this.transform, t, this.rootPosition, Color.red, 0.0f);

            t = t.GetChild();
        }
        LineDrawer.DrawCross(this.transform.TransformPoint(this.GetEndEffectorPosition() + this.rootPosition), Color.black, 0);
        LineDrawer.DrawCross(this.transform.TransformPoint(this.IKTargetForDebug), Color.gray, 0);

        // Draw target
        Debug.DrawRay(this.transform.TransformPoint(this.IKTargetForDebug), this.transform.TransformVector(IKDirectionForDebug), Color.gray, 0);
        Debug.DrawLine(this.transform.TransformPoint(this.IKLeftDebug), this.transform.TransformPoint(this.IKRightDebug), Color.gray, 0);
    }

    public string GetDebugInfo(int index)
    {
        var t = this.root;
        while (t != null)
        {
            if (index == 0)
            {
                return t.GetDebugStr();
            }
            index--;
            t = t.GetChild();
        }
        return "Joint not found";
    }

    public void DrawDebugLine(Vector3 t0, Vector3 target, Color color, float duration = 2.0f)
    {
        if (this.debugVisual)
        {
            Debug.DrawLine(this.transform.TransformPoint(t0 + this.rootPosition), this.transform.TransformPoint(target + this.rootPosition), color, 2.0f);
        }
    }

    public void DrawDebugRay(Vector3 origin, Vector3 direction, Color color, float duration = 2.0f)
    {
        if (this.debugVisual)
        {
            Debug.DrawRay(this.transform.TransformPoint(origin + this.rootPosition), this.transform.TransformVector(direction), color, 2.0f);
        }
    }

    private void DrawDebugCross(Vector3 position, Color color, float duration)
    {
        if (this.debugVisual)
        {
            LineDrawer.DrawCross(this.transform.TransformPoint(position + this.rootPosition), color, duration);
        }
    }
}
