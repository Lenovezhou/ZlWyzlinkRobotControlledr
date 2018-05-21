using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class ImageTargetTracker : MonoBehaviour
{
    private ImageTargetBehaviour imageTarget;
    private Text textOutput;
    private Animator animator;
    private Queue<Vector3> positionQueue = new Queue<Vector3>();
    private int queueSize = 30;
    public Vector3 lockedPosition;
    private Vector3 totalSum;

    private Vector3 MSEToTalSum;

    public float errorForNewSample;
    public float errorForAvg;
    public float angleBetween;
    public int count;

    private ImageTargetTracker neighber0;
    private ImageTargetTracker neighber1;

    private bool isplayedeffect = false;

    private Action lockedcallback;
    // Use this for initialization
    void Start ()
    {
		if (imageTarget == null)
        {
            Debug.LogError("The Image Target is required for initialization");
        }
        this.textOutput = transform.Find("TextOutput").GetComponent<Text>();
        this.animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () {
        PlayLockedEffect();

        if (imageTarget != null)
        {
            if (imageTarget.CurrentStatus == TrackableBehaviour.Status.TRACKED || imageTarget.CurrentStatus == TrackableBehaviour.Status.DETECTED)
            {
                AppendPositionQueue(imageTarget.transform.position);
            }
            this.textOutput.text = "" + this.errorForNewSample.ToString("0.000") + " / " + this.errorForAvg.ToString("0.000") + " / " + this.angleBetween.ToString("0.000");
        }
	}

    private void AppendPositionQueue(Vector3 position)
    {
        if (positionQueue.Count < queueSize)
        {
            positionQueue.Enqueue(position);
            totalSum += position;
            animator.SetInteger("Status", 1);
            count++;
        }
        else
        {
            animator.SetInteger("Status", 2);
            this.errorForNewSample = (this.lockedPosition - position).sqrMagnitude * 1000f;
            // TODO: Filter out bias once we have the threshold
            //if (this.errorForNewSample < mse)
            {
                positionQueue.Enqueue(position);
                totalSum += position;
                totalSum -= positionQueue.Dequeue();
            }
            var newAvg = totalSum / positionQueue.Count;
            this.errorForAvg = (newAvg - this.lockedPosition).sqrMagnitude * 1000f;
            this.lockedPosition = newAvg;
            if (neighber0 != null && neighber1 != null && neighber0.isLocked() && neighber1.isLocked())
            {
                this.angleBetween = Vector3.Angle(neighber0.lockedPosition - this.lockedPosition, neighber1.lockedPosition - this.lockedPosition);
            }
            else
            {
                this.angleBetween = 0;
            }
        }
    }

    internal void SetAngleNumbers(ImageTargetTracker imageTargetTracker1, ImageTargetTracker imageTargetTracker2)
    {
        this.neighber0 = imageTargetTracker1;
        this.neighber1 = imageTargetTracker2;
    }

    public bool isLocked()
    {
        return this.positionQueue.Count >= queueSize;
    }


    public void ClearMemery()
    {
        transform.Find("StateIndicator").GetComponent<UnityEngine.UI.Image>().color = new Color(0,0.51f,0.45f,0.35f);
        positionQueue.Clear();
        isplayedeffect = false;
        count = 0;
        totalSum = Vector3.zero;
        lockedPosition = Vector3.zero;
        animator.SetInteger("Status", 1);
    }

    void PlayLockedEffect()
    {
        if (isLocked() && !isplayedeffect)
        {
            isplayedeffect = true;
            if (null != lockedcallback)
            {
                lockedcallback();
            }
        }
    }

    internal void SetTarget(ImageTargetBehaviour imageTargetBehaviour,Action callback = null)
    {
        this.imageTarget = imageTargetBehaviour;
        lockedcallback = callback;
    }
}
