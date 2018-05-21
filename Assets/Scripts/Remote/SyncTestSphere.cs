using HoloToolkit.Sharing.Spawning;
using HoloToolkit.Sharing.SyncModel;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SyncDataClass]
public class SyncTestSphere : SyncSpawnedObject {
    [SyncData] public SyncColor color;

    public SyncTestSphere()
    {
        //color.ObjectChanged += (SyncObject obj) =>
        //{
        //    // 
        //};
    }
}
