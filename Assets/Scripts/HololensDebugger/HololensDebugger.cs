using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HololensDebugger : Singleton<HololensDebugger> {

    private TextMesh debugger;


    void Find () {
        debugger = transform.Find("FPSText").GetComponent<TextMesh>();	
	}

    public void WriteInHololensScene(string message)
    {
        if (null == debugger)
        {
            Find();
        }
        debugger.text = message + "\r\n";
    }

    public void SaveLog(string message)
    {
        if (null == debugger)
        {
            Find();
        }
        debugger.text += message + "\r\n";
    }


}
