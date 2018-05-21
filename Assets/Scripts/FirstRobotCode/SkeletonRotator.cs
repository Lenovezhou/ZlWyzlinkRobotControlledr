using UnityEngine;
using System.Collections;
using System;

public class SkeletonRotator : MonoBehaviour {
    private Quaternion initialRotation;

	// Use this for initialization
	void Start () {
        this.initialRotation = this.transform.localRotation;
	}

    internal void SetAngle(float angle)
    {
        this.transform.localRotation = this.initialRotation * Quaternion.Euler(Vector3.up * angle);
    }
}
