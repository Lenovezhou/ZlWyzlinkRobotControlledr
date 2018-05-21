using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UNETSyncParent : NetworkBehaviour {

    [SyncVar]
    public Transform parent;

    private void Awake()
    {
        transform.parent = parent;
    }

}
