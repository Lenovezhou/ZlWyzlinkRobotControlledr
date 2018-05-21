using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FabrikTest1 : MonoBehaviour {

    public int segCount = 4;
    public GameObject segmentPrefab;
    public GameObject jointPrefab;
    public GameObject pointMarkerPrefab;
    public GameObject endMarker;
    GameObject[] segs;
    GameObject[] joints;

    bool running = false;
    int mouseClicks = 0;
    Vector3 startPosition;
    Vector3 endPosition;

    // Use this for initialization
    void Start()
    {
        segs = new GameObject[segCount];
        joints = new GameObject[segCount + 1];
        //endMarker = Instantiate(pointMarkerPrefab, this.transform);
        startPosition = transform.localPosition;
        endPosition = startPosition + new Vector3(1f, 1f, 0);
        //endMarker.transform.localPosition = endPosition;

        for (int i = 0; i < segCount; i++)
        {
            segs[i] = Instantiate(segmentPrefab, this.transform);
            segs[i].transform.localPosition = new Vector3(0, i, 0);
            segs[i].transform.LookAt(segs[i].transform.position + segs[i].transform.up * (i + 1));
            segs[i].gameObject.SetActive(true);
            joints[i] = Instantiate(jointPrefab, this.transform);
            joints[i].transform.localPosition = new Vector3(0, i, 0);
            joints[i].gameObject.SetActive(true);
        }
        joints[segCount] = Instantiate(jointPrefab, this.transform);
        joints[segCount].transform.localPosition = new Vector3(0, segCount, 0);
        joints[segCount].gameObject.SetActive(true);
    }
	
	void Update ()
    {
        endPosition = endMarker.transform.position;

        for (int i = 0; i < 50; i++)
        {
            FabrikUpdate(startPosition, endPosition, i);
        }

        //if (Input.GetMouseButtonDown(0) && !running)
        {
            //StartCoroutine(FabrikUpdate(startPosition, endPosition, mouseClicks));

            mouseClicks++;
        }
    }

    //IEnumerator FabrikUpdate(Vector3 startPos, Vector3 targetPos, int clicks)
    void FabrikUpdate(Vector3 startPos, Vector3 targetPos, int clicks)
    {
        running = true;

        if (clicks % 2 == 0)
        {
            for (int i = segCount - 1; i >= 0; i--)
            {
                Vector3 temp = segs[i].transform.localPosition;

                // move segment
                if (i == segCount - 1)
                    segs[i].transform.localPosition = targetPos;
                else
                    segs[i].transform.localPosition = segs[i + 1].transform.localPosition + segs[i + 1].transform.forward;// joints[i + 1].transform.position;

                // rotate segment toward previous position
                if(i != segCount - 1)
                    segs[i].transform.LookAt(transform.position + temp);

                // move joint to segment end
                //joints[i + 1].transform.position = segs[i].transform.position + segs[i].transform.forward;
                
            }
        }
        else
        {
            for (int i = 0; i < segCount; i++)
            {
                Vector3 temp = segs[i].transform.localPosition;

                // move segment
                if (i == 0)
                    segs[i].transform.localPosition = startPos;
                else
                    segs[i].transform.localPosition = segs[i - 1].transform.localPosition + segs[i - 1].transform.forward;// joints[i + 1].transform.position;

                // rotate segment toward previous position
                if (i != segCount - 1)
                    segs[i].transform.LookAt(transform.position + temp);

                // move joint to segment end
                //joints[i + 1].transform.position = segs[i].transform.position + segs[i].transform.forward;
                
            }
        }

        running = false;
    }

    void SetPositions(int count, Vector3[] jointPositions)
    {
        if(count == segCount)
        {
            for (int i = 0; i < count; i++)
            {

            }
        }
    }
}
