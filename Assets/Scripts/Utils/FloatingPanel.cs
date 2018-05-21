using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPanel : MonoBehaviour
{
    [SerializeField]
    private float coneAngle = 15f, distanceMin = 2f, distanceMax = 4f, movementScale = 0.005f;

    Transform controlling; // reference to the obj this will be moving
    Vector3 currentPosition;
    [SerializeField]
    private bool keepInCone = true, keepInDistanceRange = true, keepFacingCamera = true;
    Vector3 coneVelocity = Vector3.zero;
    Vector3 distVelocity = Vector3.zero;

    private void Start()
    {
        controlling = this.gameObject.transform;
        currentPosition = controlling.transform.position;
    }

    private void Update()
    {
        if (keepFacingCamera)
        {
            Extensions.LookAwayFrom(controlling.transform, Camera.main.transform.position);
        }
        if (keepInCone)
        {
            UpdateKeepInCone();
        }
        if (keepInDistanceRange)
        {
            UpdateKeepInDistanceRange();
        }
        currentPosition += coneVelocity;
        currentPosition += distVelocity;
        controlling.position = currentPosition;
    }
    
    public void KeepInViewCone(bool on, float angle)
    {
        Debug.Assert(angle >= 0);

        keepInCone = on;
        coneAngle = angle;
    }

    public void KeepInDistanceRange(bool on, float min, float max)
    {
        keepInDistanceRange = on;
        distanceMax = max;
        distanceMin = min;
    }

    private void UpdateKeepInDistanceRange()
    {
        Vector3 camToPanel = currentPosition - Camera.main.transform.position;
        
        if (camToPanel.magnitude > distanceMax || camToPanel.magnitude < distanceMin)
        {
            if(camToPanel.magnitude > distanceMax)
            {
                coneVelocity = - camToPanel.normalized * (camToPanel.magnitude - distanceMax);
            }
            else
            {
                coneVelocity = camToPanel.normalized * (distanceMin - camToPanel.magnitude);
            }
            coneVelocity *= movementScale;
        }
    }

    private void UpdateKeepInCone()
    {
        Vector3 camToPanel = currentPosition - Camera.main.transform.position;
        Vector3 target = Camera.main.transform.position + Camera.main.transform.forward * ((distanceMax + distanceMin) / 2f);

        if (Vector3.Angle(camToPanel, Camera.main.transform.forward) > coneAngle)
        {
            distVelocity = target - currentPosition;
            distVelocity *= movementScale;
        }
        else
        {
            if (distVelocity.magnitude > 0.005f)
            {
                distVelocity *= 0.9f;
            }
            else
            {
                distVelocity = Vector3.zero;
            }
        }


    }
    
}
