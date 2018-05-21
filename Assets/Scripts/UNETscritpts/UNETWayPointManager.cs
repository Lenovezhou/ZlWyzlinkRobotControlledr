using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UNETWayPointManager : NetworkBehaviour
{
    private static UNETWayPointManager _Instance;
    public static UNETWayPointManager Instance
    {
        get
        {
            if (_Instance == null)
            {
                _Instance = GameObject.FindObjectOfType<UNETWayPointManager>();
            }
            return _Instance;
        }
    }

}
