using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMyPos : MonoBehaviour {

    private TextMesh selfmesh;
    private Vector3 lastpos;
	// Use this for initialization
	void Start () {
        selfmesh = GetComponent<TextMesh>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (lastpos != transform.position)
        {
            Transform parent = transform.parent;
            selfmesh.text = parent.name + parent.position.ToString();
            lastpos = transform.position;
        }	
	}
}
