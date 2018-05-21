using UnityEngine;
using System.Collections;

public class TestSceneController : MonoBehaviour {
	
	// Update is called once per frame
	void Update () {
	    if (Input.GetKeyDown(KeyCode.A))
        {
            OperationPanel.Instance.StartRenderingCurves(RobotController.Instance.testCurves);
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            RobotController.Instance.StartTrack();
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            RobotController.Instance.StartAction1();
        }

    }

}
