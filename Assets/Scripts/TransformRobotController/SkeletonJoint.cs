using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonJoint : MonoBehaviour
{
    //public Vector3 ParentPivot;
    public float length;
    public Vector3 targetPoint;

    private void Start()
    {
        Debug.Assert(this.transform.parent != null);
        //this.ParentPivot = this.transform.parent.transform
        length = Vector3.Distance(this.transform.localPosition, Vector3.zero);
        targetPoint = this.transform.position;
    }

    public void Approach(){}

    private void LateUpdate()
    {
        if (this.transform.parent != null)
        {
            Debug.DrawLine(this.transform.position, this.transform.parent.position, Color.blue);
        }
    }
}
