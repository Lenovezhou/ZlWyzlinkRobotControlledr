using UnityEngine;
using System.Collections;
using System;

public class UnityMain : MonoBehaviour {

    public GameObject RobotArmPrefab;
    public GameObject OperationPanelPrefab;

	// Use this for initialization
	void Start () {
        //StartCoroutine(PrintSomething());
	}

    private IEnumerator PrintSomething()
    {
        Debug.Log("Test log");
        yield return new WaitForSeconds(3.0f);
        Debug.Log("Test log");
        yield return new WaitForSeconds(3.0f);
        Debug.Log("Test log");
        yield return new WaitForSeconds(3.0f);
        Debug.Log("Test log");
        yield return new WaitForSeconds(3.0f);
        Debug.Log("Test log");
        yield return new WaitForSeconds(3.0f);
        Debug.Log("Test log");
    }

    public void StartTheScene()
    {
        Debug.Log("The scene is started.");
        StartCoroutine(SetupTheScene());
    }

    public IEnumerator SetupTheScene()
    {
        yield return new WaitForSeconds(2.0f);
        var panel = Instantiate(OperationPanelPrefab);
        panel.transform.position = CalculateInitPosition(2f, -0.5f);

        yield return new WaitForSeconds(3.0f);
        var robotArm = Instantiate(RobotArmPrefab);
        robotArm.transform.position = CalculateInitPosition(5.0f, -1.8f);
    }

    private Vector3 CalculateInitPosition(float distanceForward, float up)
    {
        var forwardVector = Camera.main.transform.forward;
        forwardVector.y = 0;
        forwardVector = forwardVector.normalized * distanceForward;
        return Camera.main.transform.position + forwardVector + Vector3.up * up;
    }

    // Update is called once per frame
    void Update () {
	
	}
}
