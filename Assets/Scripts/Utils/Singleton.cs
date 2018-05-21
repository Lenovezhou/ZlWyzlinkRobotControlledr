using UnityEngine;
using System.Collections;

public class Singleton<T> : MonoBehaviour where T: Object {

    private static T _singleton;

    public static T Instance
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = GameObject.FindObjectOfType<T>();
            }
            return _singleton;
        }
    }

}
