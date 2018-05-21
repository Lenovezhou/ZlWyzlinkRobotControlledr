using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Extensions{

    private static float matchingThreshold = 0.4f;
    private static float maxForce = 10000;

    public static IEnumerator MoveToAngle(this HingeJoint joint, float angle)
    {
        while (Mathf.Abs(joint.angle - angle) > matchingThreshold)
        {
            joint.motor = new JointMotor() { force = maxForce, freeSpin = false, targetVelocity = MoveTowardsWith(joint, angle) };
            yield return new WaitForFixedUpdate();
        }
    }

    public static void SetSpeed(this HingeJoint joint, float speed)
    {
        joint.motor = new JointMotor() { force = maxForce, freeSpin = false, targetVelocity = speed };
    }

    public static void ForceAngle(this HingeJoint joint, float angle)
    {
        joint.useLimits = true;
        joint.limits = new JointLimits() { max = angle, min = angle - 0.1f };
    }

    public static void LookAwayFrom(Transform t, Vector3 target)
    {
        Vector3 towardTarget = target - t.position;
        t.LookAt(t.position - towardTarget);
    }

    public static float MoveTowardsWith(HingeJoint joint, float target)
    {
        if (joint.useLimits)
        {
            var moveWithLimits = Mathf.MoveTowards(joint.angle, target, 30);
            return moveWithLimits - joint.angle;
        }
        var move = Mathf.MoveTowardsAngle(joint.angle, target, 30);
        return move - joint.angle;
    }

    public static IEnumerator EmptyCoroutine()
    {
        yield return 1;
    }

    public static T FindChildOfType<T>(this Component component)
    {
        foreach (Transform child in component.transform)
        {
            var subComponent = child.GetComponent<T>();
            if (subComponent != null)
            {
                return subComponent;
            }
        }
        return default(T);
    }

    public static IEnumerable<T> FindChildrenOfType<T>(this Component component)
    {
        foreach (Transform child in component.transform)
        {
            var subComponent = child.GetComponent<T>();
            if (subComponent != null)
            {
                yield return subComponent;
            }
        }
    }

    public static T FindDescendentOfType<T>(this Component component)
    {
        Stack<Transform> items = new Stack<Transform>();
        foreach (Transform child in component.transform)
        {
            items.Push(child);
        }
        while (items.Count > 0)
        {
            var comp = items.Pop();
            var subComponent = comp.GetComponent<T>();
            if (subComponent != null)
            {
                return subComponent;
            }
            else
            {
                foreach (Transform child in comp.transform)
                {
                    items.Push(child);
                }
            }
        }
        return default(T);
    }

    public static void FindAllDescendentOfType<T>(this Component component, System.Action<T> action) where T : Component
    {
        foreach (Transform child in component.transform)
        {
            var comp = child.GetComponent<T>();
            if (comp != null)
            {
                Debug.Log("Processed " + comp.gameObject.name);
                action(comp);
            }
            child.FindAllDescendentOfType<T>(action);
        }
    }

    public static Color SetAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }
}
