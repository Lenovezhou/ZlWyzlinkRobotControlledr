using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraRotation : MonoBehaviour, IDragHandler {
    private Transform cameraTransform;

    private void Start()
    {
        this.cameraTransform = transform;//this.transform.Find("Main Camera");
        targetZ = this.cameraTransform.position.z;
        Debug.Assert(this.cameraTransform != null);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Consider to add touch screen control
        throw new NotImplementedException();
    }

    private float targetAngleY = 25;
    private float targetAngleX = 0;
    private float targetZ;
    private float velocityX = 0;
    private float velocityY = 0;
    private float velocityZ = 0;
    public float smoothTime = 0.5f;
    public float speed = 60.0f;
    public float zoomSpeed = 10.0f;

    private Vector2 dragOffset;
    private float zoom;

	// Update is called once per frame
	void Update () {
        var h = Input.GetAxis("Horizontal") + this.dragOffset.x;
        var v = -Input.GetAxis("Vertical") + this.zoom;
        var d = Input.GetAxis("Depth") - this.dragOffset.y;

        targetAngleY += h * speed * Time.deltaTime;
        targetAngleX = Mathf.Clamp(targetAngleX + d * speed * Time.deltaTime, -10, 45);

        var angleY = Mathf.SmoothDampAngle(this.transform.rotation.eulerAngles.y, targetAngleY, ref velocityY, smoothTime);
        var angleX = Mathf.SmoothDampAngle(this.transform.rotation.eulerAngles.x, targetAngleX, ref velocityX, smoothTime);
        this.transform.localRotation = Quaternion.Euler(angleX, angleY, 0);

        targetZ = Mathf.Clamp(targetZ + v * zoomSpeed * Time.deltaTime, -30, -0.5f);
        var position = cameraTransform.localPosition;
        position.z = Mathf.SmoothDamp(position.z, targetZ, ref velocityZ, smoothTime);
        cameraTransform.transform.localPosition = position;
	}

    public void SetDragOffset(Vector2 delta)
    {
        this.dragOffset = delta;
    }

    public void SetZoom(float zoom)
    {
        this.zoom = zoom;
    }
}
