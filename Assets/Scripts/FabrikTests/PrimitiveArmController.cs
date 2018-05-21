using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// version 1 - getting something working

public class PrimitiveArmController : MonoBehaviour
{
    public GameObject seg1, seg2, joint1, joint2;
    float segmentLength = 1;

    // specify a position for the arm to point at
    Vector3 target = new Vector3(1, 1, 0);
    Vector3 lastGoodPosition;

    // Use this for initialization
    void Start ()
    {
        // mark the end point with a cylinder
        joint2.transform.position = target;
        lastGoodPosition = joint2.transform.position;
    }
	
	// Update is called once per frame
	void Update ()
    {
        // make sure we don't break the arm
        if(joint2.transform.position.magnitude > 2)
        {
            joint2.transform.position = lastGoodPosition;
        }

        // joint1 marks the elbow
        // when we find this, the rest is easy
        Vector3 midpoint = joint2.transform.position * 0.5f;
        float a = Mathf.Sqrt(1f - midpoint.magnitude * midpoint.magnitude);

        Vector3 toElbow = midpoint.normalized;
        // rotate
        float temp = toElbow.x;
        toElbow.x = -toElbow.y;
        toElbow.y = temp;

        joint1.transform.position = midpoint + toElbow * a;

        // arm segment 1 will start at the origin and point to the first joint
        seg1.transform.position = new Vector3(0, 0, 0);
        seg1.transform.LookAt(joint1.transform.position);

        // arm segment 2 goes from the elbow to the target
        seg2.transform.position = joint1.transform.position;
        seg2.transform.LookAt(joint2.transform.position);

        lastGoodPosition = joint2.transform.position;
    }
}
