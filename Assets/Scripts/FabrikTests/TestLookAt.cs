using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLookAt : MonoBehaviour {

    public GameObject target;
    Quaternion startRot;

	// Use this for initialization
	void Start ()
    {
        LookTowardOnAxis(-transform.up, transform.forward, target.transform.position);
        startRot = transform.rotation;
        //LookTowardOnAxis(transform.right, Vector3.up, target.transform.position);
    }
	
	// Update is called once per frame
	void Update () {
        transform.rotation = startRot;
        LookTowardOnAxis(Vector3.up, Vector3.right, target.transform.position);
    }

    void LookTowardOnAxis(Vector3 rotateThis, Vector3 aroundThis, Vector3 towardThis)
    {
        // this vector is needed to decide whether to rotate with a positive or negative angle
        Vector3 crossed = Vector3.Cross(rotateThis, aroundThis);

        // this points toward the target and will be projected
        Vector3 projected = towardThis - transform.position;

        // now it is projected into the plane defined by the normal vector aroundThis
        projected = Vector3.ProjectOnPlane(projected, aroundThis);

        // if this is zero, angle is zero and we don't need to rotate
        float dotProduct = Vector3.Dot(crossed, projected);

        // now we just need to get the angle between two vectors which are now in the same plane
        float angle = 0;
        if (dotProduct > 0)
        {
            angle = -Vector3.Angle(rotateThis, projected);
        }
        else if (dotProduct < 0)
        {
            angle = Vector3.Angle(rotateThis, projected);
        }

        // apply the rotation in worldspace because all our parameters are in worldspace
        transform.Rotate(aroundThis, angle, Space.World);
    }
}
