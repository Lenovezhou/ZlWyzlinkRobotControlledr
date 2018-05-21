using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowMyPos : MonoBehaviour {

    private Vector3 previouspos;

    private TextMesh selftextmesh;

    private string newline = "\r\n";

    private Transform robot;
    private Transform robotmain;

    void Start ()
    {
        selftextmesh = GetComponent<TextMesh>();
        robot = transform.parent;
        robotmain = transform.parent.parent;
    }
	
	void Update () {
        if (transform.parent.position != previouspos)
        {
            previouspos = robot.position;
            selftextmesh.text = "robot_position-->>"+ previouspos.x.ToString() + newline + previouspos.y.ToString() + newline + previouspos.z.ToString();
            selftextmesh.text += newline+ "Localpos::"+robot.localPosition.ToString();
            selftextmesh.text +=  newline+robotmain.name + "_position -- >>" + robotmain.position.x.ToString() + newline + robotmain.position.y.ToString() + newline + robotmain.position.z.ToString();

        }
	}
}
