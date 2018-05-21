using UnityEngine;
using System.Collections;
using System;

public class RobotController : Singleton<RobotController> {

    private float[] jointForces = new float[7];

    public HingeJoint[] joints = new HingeJoint[7];

    public float force = 10000;

    public AnimationCurve[] testCurves = new AnimationCurve[7];

    private const float width = 800;
    private const float height = 500;
    private DebugOverlay debugOverlay = new DebugOverlay(width, height, false);
    private Color[] colors = new Color[] { Color.blue, Color.red, Color.yellow, Color.cyan, Color.green };

    // Use this for initialization
    void Start () {
        EnablePhysics(false);
        this.debugOverlay.DrawCurve(0, this.testCurves[0], Color.blue);
        this.debugOverlay.DrawCurve(1, this.testCurves[1], Color.red);
        this.debugOverlay.DrawCurve(2, this.testCurves[2], Color.yellow);
        this.debugOverlay.DrawCurve(3, this.testCurves[3], Color.cyan);
        this.debugOverlay.DrawCurve(4, this.testCurves[4], Color.green);
    }

    public void StartAction()
    {
        EnablePhysics(true);

        StartCoroutine(RunCurves(10.0f, 100, testCurves));
    }

    public void StartAction1()
    {
        EnablePhysics(true);

        StartCoroutine(this.RunCurvesWithVelocity(10.0f, 100, testCurves));
    }

    public void StartTrack()
    {
        StartCoroutine(this.GetComponent<Skeleton>().RunCurves(10.0f, 100, testCurves));
    }

    public void EnablePhysics(bool enabled)
    {
        this.transform.FindAllDescendentOfType<Rigidbody>((Rigidbody rigidbody) => rigidbody.isKinematic = !enabled);
    }
	
    void FixedUpdate()
    {
        //for (int i = 0; i < joints.Length; i++)
        //{
        //    SetMotorForce(joints[i], jointForces[i]);
        //}
    }

    void SetMotorForce(HingeJoint joint, float jointForce)
    {
        var motor = joint.motor;
        motor.force = this.force;
        motor.targetVelocity = jointForce;
        joint.motor = motor;
    }

    void OnGUI()
    {
        //jointForces[0] = GUI.HorizontalSlider(new Rect(10, 10, 100, 40), jointForces[0], -50, 50);
        //jointForces[1] = GUI.HorizontalSlider(new Rect(10, 50, 100, 40), jointForces[1], -50, 50);
        //jointForces[2] = GUI.HorizontalSlider(new Rect(10, 90, 100, 40), jointForces[2], -50, 50);
        //jointForces[3] = GUI.HorizontalSlider(new Rect(10, 130, 100, 40), jointForces[3], -50, 50);
        //jointForces[4] = GUI.HorizontalSlider(new Rect(10, 170, 100, 40), jointForces[4], -50, 50);
        //jointForces[5] = GUI.HorizontalSlider(new Rect(10, 210, 100, 40), jointForces[5], -50, 50);
        //jointForces[6] = -jointForces[5];


        //if (GUI.Button(new Rect(130, 10, 100, 30), "Set angle"))
        //{
        //    this.SetActuatorAnglesMovement(45, 30, -160, 10, 0, 0, 0);
        //}
        //if (GUI.Button(new Rect(10, 10, 100, 30), "Reset angle"))
        //{
        //    this.SetActuatorAnglesMovement(0, 0, 0, 0, 0, 0, 0);
        //}


        //if (GUI.Button(new Rect(140, 10, 90, 30), "Start Action"))
        //{
        //    StartAction();
        //}
        //if (GUI.Button(new Rect(240, 10, 90, 30), "Start Action 1"))
        //{
        //    StartAction1();
        //}
        //if (GUI.Button(new Rect(340, 10, 90, 30), "Start Track"))
        //{
        //    StartTrack();
        //}

        //this.debugOverlay.DrawTexture();
        
    }

    public void SetActuatorAnglesMovement(params float[] angles)
    {
        //Debug.Log("Setting angles" + angles[0]);
        for (int i = 0; i < angles.Length; i++)
        {
            StartCoroutine(this.joints[i].MoveToAngle(angles[i]));
            //this.joints[i].ForceAngle(angles[i]);
        }
    }

    internal float[] GetAngles()
    {
        float[] angles = new float[this.joints.Length];      // Number of angles
        for (int i = 0; i < this.joints.Length; i++)
        {
            angles[i] = this.joints[i].angle;
        }
        return angles;
    }

    public IEnumerator RunCurves(float timeScale, int sampleRate, params AnimationCurve[] curves)
    {
        yield return new WaitForSeconds(1);

        this.GetComponent<AudioSource>().Play();

        var startTime = Time.time;
        for (int i = 0; i < sampleRate; i++)
        {
            var timePoint = Time.time - startTime;
            float[] angles = new float[this.joints.Length];
            for (int j = 0; j < curves.Length; j++)
            {
                angles[j] = curves[j].Evaluate(timePoint / timeScale) * 180;
            }
            SetActuatorAnglesMovement(angles);

            UpdateChart(timeScale, timePoint);

            yield return new WaitForSeconds(timeScale / sampleRate);
        }
        this.GetComponent<AudioSource>().Stop();

        EnablePhysics(false);
    }

    public IEnumerator RunCurvesWithVelocity(float timeScale, int sampleRate, params AnimationCurve[] curves)
    {
        yield return new WaitForSeconds(0.1f);

        this.GetComponent<AudioSource>().Play();
        var startTime = Time.time;
        for (int i = 0; i < sampleRate; i++)
        {
            var timePoint = Time.time - startTime;
            float[] angles = new float[this.joints.Length];
            for (int j = 0; j < curves.Length; j++)
            {
                var velocity = curves[j].EvaluateTangent(timePoint / timeScale) * 0.2f;
                var expectedAngle = curves[j].Evaluate(timePoint / timeScale) * 180;
                var angle = joints[j].angle;
                velocity = velocity + velocity * VelocityErrorAdjustment(expectedAngle, angle);
                this.joints[j].SetSpeed(velocity);
            }

            UpdateChart(timeScale, timePoint);

            yield return new WaitForSeconds(timeScale / sampleRate);
        }
        this.GetComponent<AudioSource>().Stop();
        EnablePhysics(false);
    }

    private float VelocityErrorAdjustment(float expectedAngle, float angle)
    {
        return Mathf.Atan((angle - expectedAngle) / 5) * 0.4f;
    }

    private void UpdateChart(float timeScale, float timePoint)
    {
        var capturedAngles = GetAngles();
        for (int k = 0; k < 5; k++)
        {
            this.debugOverlay.DrawPoint(k, timePoint / timeScale, capturedAngles[k] / 90, colors[k]);
        }
        this.debugOverlay.Apply();
    }
}
