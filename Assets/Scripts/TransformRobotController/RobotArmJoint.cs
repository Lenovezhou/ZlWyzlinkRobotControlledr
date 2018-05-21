using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotArmJoint : MonoBehaviour
{
    private Vector3 rotationAxisLocal = Vector3.up;

    [SerializeField]
    private float minAngle;
    [SerializeField]
    private float maxAngle, maxSpeed, currentMaxSpeed = 30.0f;
    [SerializeField]
    public float pointingOffset = 0.0f;
    [SerializeField]
    [Tooltip("Check if this joint is sysmmetric on forward and backward")]
    public bool is180Symmetrical = false;
    [SerializeField]
    [Tooltip("Check if this joint controls the wrist point, where could be pointed without angle constrains")]
    public bool isWristPoint = false;

    [Header("Information indicator")]
    [SerializeField]
    private IndicatorUIBase indicatorPrefab;
    [SerializeField]
    private float indicatorScale = 1.0f;
    [SerializeField]
    private float indicatorOffset = 0.12f;
    private IndicatorUIBase indicator;


    // TODO: simulate the increase to target speed and decrease to stopping point
    //[SerializeField]
    //private float acceleration; // in degrees per second

    public InterpolationMethods.InterpolationMethod interpolationMethod =
        InterpolationMethods.InterpolationMethod.Trigonometric;

    [Tooltip("If the target angle it is moving toward is this far away, it is set to it.")]
    [SerializeField]
    public float angleEpsilon = 0.1f;

    [Header("For viewing:")]
    [SerializeField]
    private float currentAngle = 0;
    [SerializeField]
    private float targetAngle = 0;
    private float initialAngle = 0;
    private Quaternion initialRotation;

    private RobotControllerSimple controller;

    
    private void Awake()
    {
        this.controller = GetComponentInParent<RobotControllerSimple>();
        this.initialAngle = currentAngle;
        initialRotation = this.transform.localRotation;
    }

    private void Start()
    {
        if (indicatorPrefab != null)
        {
            this.indicator = Instantiate<IndicatorUIBase>(indicatorPrefab, this.transform);
            this.indicator.transform.localScale *= indicatorScale;
            this.indicator.transform.localPosition = Vector3.up * indicatorOffset;
        }
    }

    public void SetPhysicalMaxSpeedLimit(float max)
    {
        maxSpeed = max;
        currentMaxSpeed = 30;
    }

    public void SetCurrentMaxSpeed(float newMax)
    {
        if(newMax < maxSpeed)
        {
            currentMaxSpeed = newMax;
        }
        else if(newMax < 0)
        {
            currentMaxSpeed = 0;
        }
        else
        {
            currentMaxSpeed = maxSpeed;
        }
    }

    public float GetCurrentMaxSpeed()
    {
        return currentMaxSpeed;
    }

    public float MaxAngle
    {
        get
        {
            return maxAngle;
        }

        set
        {
            maxAngle = value;
        }
    }

    public float MinAngle
    {
        get
        {
            return minAngle;
        }

        set
        {
            minAngle = value;
        }
    }
    
    public float GetAngle()
    {
        return currentAngle;
    }

    /// <summary>
    /// Instantly sets the current angle and connecting segments to this angle.
    /// </summary>
    /// <param name="angle"></param>
    public void RotateToAngle(float angle, float powerConsumption = 0.0f)
    {
        this.transform.localRotation = initialRotation * Quaternion.AngleAxis(angle - initialAngle, rotationAxisLocal);
        currentAngle = angle;
        if (this.indicator != null)
        {
            this.indicator.UpdateValues(angle, powerConsumption);
        }
    }

    private void CheckForMaxMinAngle()
    {
        //if (currentAngle > MaxAngle - angleEpsilon)
        //{
        //    SetModelColor(Color.red);
        //}
        //else if (currentAngle < MinAngle + angleEpsilon)
        //{
        //    SetModelColor(Color.yellow);
        //}
        //else
        //{
        //    SetModelColor(modelStartingColor);
        //}
    }
    
    public Coroutine MoveAngleWithSpeed(bool absOrRel, float angle, float speed)
    {
        float targetAngle = currentAngle - angle;
        if (absOrRel == true)
        {
            targetAngle = angle;
        }

        if (targetAngle > MaxAngle)
        {
            targetAngle = MaxAngle;
        }
        else if (targetAngle < MinAngle)
        {
            targetAngle = MinAngle;
//            SetModelColor(Color.yellow);
        }

        return StartCoroutine(CR_MoveToAngleWithSpeed(targetAngle, speed));
    }

    public void StopCoroutines()
    {
        this.StopAllCoroutines();
    }

    // TODO: Need to optimize this function to consider the wrapping case
    // And consider the MinMax limitations better. Consider better implementation for trapezoidal move profiles
    IEnumerator CR_MoveToAngleWithSpeed(float _targetAngle, float speedProportion)
    {
        targetAngle = _targetAngle;
        float startAngle = GetAngle();
        float angleDistToTarget = _targetAngle - startAngle;
        float direction = _targetAngle > startAngle ? 1 : -1;
        float timeElapsed = 0;
        float speedThisMovement = currentMaxSpeed * speedProportion;

        while (true)
        {
            CheckForMaxMinAngle();

            if (direction == 1)
            {
                if (GetAngle() + angleEpsilon > _targetAngle)
                {
                    RotateToAngle(_targetAngle);
                    break;
                }
            }
            else
            {
                if (GetAngle() - angleEpsilon < _targetAngle)
                {
                    RotateToAngle(_targetAngle);
                    break;
                }
            }

            timeElapsed += Time.deltaTime;

            float nextAngle = startAngle + direction * timeElapsed * speedThisMovement;

            RotateToAngle(nextAngle);

            yield return new WaitForEndOfFrame();
        }
    }

    /// <summary>
    /// Returns the interpolated value between 0 and 1, given the linear value.
    /// </summary>
    /// <param name="normalizedValue"></param>
    /// <returns></returns>
    private float GetInterpolation(float normalizedValue)
    {
        if (interpolationMethod == InterpolationMethods.InterpolationMethod.Linear)
        {
            return normalizedValue;
        }
        else if (interpolationMethod == InterpolationMethods.InterpolationMethod.Trigonometric)
        {
            //float zeroToOne = timeNormalized;
            //float zeroToPi = zeroToOne * Mathf.PI;
            //float minusOneToOne = Mathf.Cos(zeroToPi);
            //float backToZeroToOne = 1f - (minusOneToOne + 1f) / 2f;
            //return backToZeroToOne;
            return 1f - (Mathf.Cos(normalizedValue * Mathf.PI) + 1f) / 2f;
        }
        else
        {
            return 1;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (this.controller != null && this.controller.displayJointLimits)
        {
            DrawAngleLimits(this.transform.position, this.transform.rotation, this.rotationAxisLocal, minAngle, maxAngle);
        }
    }

    private void DrawAngleLimits(Vector3 position, Quaternion rotation, Vector3 axis, float minAngle, float maxAngle)
    {
        float length = 0.03f;
        float radius = 0.1f;
        float radius2 = 0.11f;
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(position, position + rotation * Vector3.forward * length);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(position, position + rotation * Vector3.up * Mathf.Max(radius, radius2));
        Gizmos.DrawLine(position, position + rotation * Quaternion.AngleAxis(minAngle, axis) * Vector3.up * radius);
        Gizmos.DrawLine(position, position + rotation * Quaternion.AngleAxis(maxAngle, axis) * Vector3.up * radius2);
#if UNITY_EDITOR
        UnityEditor.Handles.color = Color.yellow;
        UnityEditor.Handles.DrawWireArc(position, rotation * axis, rotation * Vector3.up * radius, minAngle, radius);
        UnityEditor.Handles.DrawWireArc(position, rotation * axis, rotation * Vector3.up * radius, maxAngle, radius2);
#endif
    }
}
