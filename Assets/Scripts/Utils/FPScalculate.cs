using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPScalculate : MonoBehaviour
{
	void Update ()
    {
        this.GetComponent<Text>().text = ((int)(1f / Time.deltaTime)).ToString();
	}
}
