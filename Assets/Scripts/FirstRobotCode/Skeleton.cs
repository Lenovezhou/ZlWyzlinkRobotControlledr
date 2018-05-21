using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Collections.Generic;

public class Skeleton : MonoBehaviour {

    public GameObject actuatorNode;
    public GameObject endNode;

    public GameObject trackingPoint;

    public AudioClip clickSound;

    private GameObject skeleton;
    private Transform endEffector;
    private List<GameObject> trackingPoints = new List<GameObject>();

    private bool syncAngles = false;

    private AudioSource audioSource;

    // Use this for initialization
    void Start () {
        StartCoroutine(BuildSkeletonStructure());
	}

    private IEnumerator BuildSkeletonStructure()
    {
        yield return new WaitForEndOfFrame();

        var endEffector = FindObjectOfType<EndEffector>();
        if (endEffector == null)
        {
            Debug.Log("Failed to find the EndEffector in the scene.");
            yield break;
        }
        this.skeleton = BuildSkeletonStructure(endEffector.gameObject, null);
        this.audioSource = this.endEffector.GetComponent<AudioSource>();
    }

    private GameObject BuildSkeletonStructure(GameObject node, GameObject root)
    {
        if (node == null)
        {
            Debug.Log("Invalid node null");
            return null;
        }

        HingeJoint joint = node.GetComponent<HingeJoint>();
        if (joint != null)
        {
            var parentNode = joint.connectedBody;
            var parentJoint = parentNode.GetComponent<HingeJoint>();
            if (parentJoint != null)
            {
                var newRoot = Instantiate(this.actuatorNode);
                root.transform.SetParent(newRoot.transform);
                root.transform.localPosition = Quaternion.FromToRotation(parentNode.transform.TransformDirection(parentJoint.axis), Vector3.up)
                    * (node.transform.TransformPoint(joint.anchor) - parentNode.transform.TransformPoint(parentJoint.anchor));
                root.transform.localRotation = Quaternion.FromToRotation(parentNode.transform.TransformDirection(parentJoint.axis), node.transform.TransformDirection(joint.axis));

                return BuildSkeletonStructure(parentNode.gameObject, newRoot);
            }
            else
            {
                var newRoot = Instantiate(this.actuatorNode);
                root.transform.SetParent(newRoot.transform);
                root.transform.localPosition = Quaternion.FromToRotation(node.transform.TransformDirection(joint.axis), Vector3.forward) 
                    * node.transform.TransformVector(joint.connectedAnchor);
                newRoot.transform.position = parentNode.transform.position;
                newRoot.transform.GetChild(0).localScale *= 2;
                return newRoot;
            }
        }
        else if (root == null)
        {
            var parentNode = node.transform.parent;
            var parentJoint = parentNode.GetComponent<HingeJoint>();
            Debug.Assert(parentNode.GetComponent<HingeJoint>() != null, "Assume the parent is connected");
            root = Instantiate(this.endNode);
            var newRoot = Instantiate(this.actuatorNode);
            root.transform.SetParent(newRoot.transform);
            root.transform.localPosition = Quaternion.FromToRotation(parentNode.transform.TransformDirection(parentJoint.axis), Vector3.up) 
                * (node.transform.position - parentNode.transform.TransformPoint(parentJoint.anchor));

            this.endEffector = root.transform;
            return BuildSkeletonStructure(parentNode.gameObject, newRoot);
        }
        Debug.Assert(false, "Unexpected path");
        return null;

    }

    // Update is called once per frame
    void Update () {
        if (this.syncAngles)
        {
            SyncAngles();
        }
	}

    void OnGUI()
    {
        //syncAngles = GUI.Toggle(new Rect(130, 10, 60, 30), syncAngles, "Sync");
    }

    private void SyncAngles()
    {
        var controller = GetComponent<RobotController>();
        var angles = controller.GetAngles();
        ApplyAngles(this.skeleton, angles);
    }

    private void ApplyAngles(GameObject skeletonRoot, float[] angles)
    {
        if (skeletonRoot != null && skeletonRoot.transform.childCount > 1)
        {
            SkeletonRotator skeletonRotator = skeletonRoot.transform.FindChildOfType<SkeletonRotator>();
            foreach (var angle in angles)
            {
                if (skeletonRotator == null)
                {
                    break;
                }
                skeletonRotator.SetAngle(angle);

                skeletonRotator = skeletonRotator.FindChildOfType<SkeletonRotator>();
            }
        }
    }

    public IEnumerator RunCurves(float timeScale, int sampleRate, params AnimationCurve[] curves)
    {
        yield return new WaitForSeconds(1.0f);

        if (this.trackingPoints.Count != sampleRate)
        {
            this.trackingPoints.Clear();
            for (int i = 0; i < sampleRate; i++)
            {
                this.trackingPoints.Add(Instantiate(this.trackingPoint));
            }
        }

        float timePoint = 0;
        for (int i = 0; i < sampleRate; i++)
        {
            float[] angles = new float[curves.Length];
            for (int j = 0; j < curves.Length; j++)
            {
                angles[j] = curves[j].Evaluate(timePoint / timeScale) * 180;
            }
            ApplyAngles(this.skeleton, angles);

            timePoint += timeScale / sampleRate;
            this.audioSource.PlayOneShot(clickSound);
            yield return new WaitForSeconds(0.1f);

            this.trackingPoints[i].transform.position = this.endEffector.position;
            this.trackingPoints[i].transform.rotation = this.endEffector.rotation;
        }
    }
}
