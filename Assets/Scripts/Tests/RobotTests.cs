using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotTests : MonoBehaviour
{
    public RobotControllerSimple robot;

	// Use this for initialization
	void Start () {
        if (robot == null)
        {
            Debug.LogError("Failed to find the robot");
            return;
        }
        foreach (Transform t in this.transform)
        {
            t.gameObject.SetActive(false);
        }
        StartCoroutine(TestLoop());
	}

    private IEnumerator TestLoop()
    {
        yield return new WaitForSeconds(2);

        foreach (Transform target in this.transform)
        {
            Debug.Log("Testing");
            // TODO: Need to transform to robot space
            target.gameObject.SetActive(true);
            RenderPath(robot, robot.GetEndEffectorRotationPosition(), 
                RotationPosition.GetRotationPosition(target.gameObject.transform));
            yield return robot.IKMoveToTarget(target.position, target.rotation);
            yield return new WaitForSeconds(1);
            target.gameObject.SetActive(false);
        }
    }

    private void RenderPath(RobotControllerSimple robot, RotationPosition from, RotationPosition to)
    {
        var list = robot.GetPath(from, to);
        float duration = 8.0f;
        for (int i = 0; i < list.Length - 1; i++)
        {
            Debug.DrawLine(list[i].Position, list[i + 1].Position, Color.green, duration);
            Debug.DrawRay(list[i].Position, list[i].Rotation * Vector3.forward * 0.04f, Color.gray, duration);
            Debug.DrawLine(list[i].Position + list[i].Rotation * Vector3.left * 0.01f, list[i].Position + list[i].Rotation * Vector3.right * 0.01f, Color.gray, duration);
        }
    }
}
