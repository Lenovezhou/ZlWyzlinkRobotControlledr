using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldPositionTracker : Singleton<WorldPositionTracker>
{
    private RobotsMain robotsMain;
    private void Start()
    {
        robotsMain = GameObject.FindObjectOfType<RobotsMain>();
    }

    private void Update()
    {
        robotsMain.transform.position = this.transform.position;
        robotsMain.transform.rotation = this.transform.rotation;
    }
}
