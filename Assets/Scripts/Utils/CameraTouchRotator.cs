using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraTouchRotator : MonoBehaviour
{
    private CameraRotation cameraRotationController;
    private float checkingDelayCounter;
    private Vector2 accumulatedMove;
    private int previousTouchCount;

    public enum TouchState
    {
        NoTouch,
        MovingX,
        MovingY,
        OrientationChecking,
        Zooming,
    };

    public TouchState touchState = TouchState.NoTouch;

    private void Start()
    {
        this.cameraRotationController = GetComponent<CameraRotation>();
    }

    private void Update()
    {
        if (touchState == TouchState.NoTouch && previousTouchCount == 0 && Input.touchCount > 0)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (Input.touchCount == 1)
                {
                    this.touchState = TouchState.OrientationChecking;
                    this.checkingDelayCounter = 100;
                    this.accumulatedMove = Vector2.zero;
                }
                else if (Input.touchCount == 2)
                {
                    this.touchState = TouchState.Zooming;
                }
            }
        }
        else if (this.touchState == TouchState.OrientationChecking)
        {
            if (Input.touchCount == 1)
            {
                this.checkingDelayCounter--;
                var t0 = Input.GetTouch(0);
                var absX = Mathf.Abs(t0.deltaPosition.x);
                var absY = Mathf.Abs(t0.deltaPosition.y);
                if (checkingDelayCounter <= 0 || absX > 1f || absY > 1f)
                {
                    this.touchState = (absX >= absY) ? TouchState.MovingX : TouchState.MovingY;
                    Debug.Log("We got " + t0.deltaPosition.ToString() + "@" + this.checkingDelayCounter);
                }
            }
            else if (Input.touchCount == 2)
            {
                this.touchState = TouchState.Zooming;
            }
            else
            {
                this.touchState = TouchState.NoTouch;
            }
        }
        else if (this.touchState == TouchState.Zooming)
        {
            if (Input.touchCount == 2)
            {
                var t0 = Input.GetTouch(0);
                var t1 = Input.GetTouch(1);

                var d0 = Vector2.Distance(t0.position, t1.position);
                var d1 = Vector2.Distance(t0.position - t0.deltaPosition, t1.position - t1.deltaPosition);

                var zoom = d0 - d1;
                cameraRotationController.SetZoom(zoom / 2);
            }
            else
            {
                // End zooming
                cameraRotationController.SetZoom(0);
                this.touchState = TouchState.NoTouch;
            }
        }
        else if (this.touchState == TouchState.MovingX)
        {
            if (Input.touchCount == 1)
            {
                var t0 = Input.GetTouch(0);
                cameraRotationController.SetDragOffset(new Vector2(t0.deltaPosition.x, 0));
            }
            else
            {
                cameraRotationController.SetDragOffset(Vector2.zero);
                this.touchState = TouchState.NoTouch;
            }
        }
        else if (this.touchState == TouchState.MovingY)
        {
            if (Input.touchCount == 1)
            {
                var t0 = Input.GetTouch(0);
                cameraRotationController.SetDragOffset(new Vector2(0, t0.deltaPosition.y));
            }
            else
            {
                cameraRotationController.SetDragOffset(Vector2.zero);
                this.touchState = TouchState.NoTouch;
            }
        }
        this.previousTouchCount = Input.touchCount;
    }
}
