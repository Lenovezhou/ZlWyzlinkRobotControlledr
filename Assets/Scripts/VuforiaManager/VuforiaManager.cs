using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if NETFX_CORE
using UnityEngine.XR.WSA.Input;
#endif

public class VuforiaManager : Singleton<VuforiaManager> {

    //校准参数
    private Vector3 limitpos = new Vector3(0.11706f, -0.10598f, 0.0828f);//(0.5485f, 0.3293f, -0.1031f);
    private Vector3 limitangle = new Vector3(87.839f,120.803f,-119.804f);
    public Transform offsetpos;

#if NETFX_CORE
    private GestureRecognizer recognizer;
#endif
    public Action callback;
    public Transform[] vuforia;
    public Transform imageTarget;
    private TextMesh remindText;
    private MeshRenderer trakerrenderer;
    public TextMesh debuggermesh;

    private List<Transform> movetargets = new List<Transform>();
    private List<Vector3> originpos = new List<Vector3>();
    //private Dictionary<string, Transform> movetargets = new Dictionary<string, Transform>();
    //private Dictionary<string, Vector3> originpos = new Dictionary<string, Vector3>();
    //private List<string> allparts = new List<string>();

    public void Traking(bool isactive,Action callback = null)
    {
        ControllVuforia(isactive);
        this.callback = callback;
    }

    public void RegisterSawpobj(Transform t)
    {
        movetargets.Add(t);
        originpos.Add(t.position);
        //allparts.Add(t.name);
    }

    /// <summary>
    /// 打开或者关闭vuforia
    /// </summary>
    /// <param name="isactive">打开或关闭</param>
    void ControllVuforia(bool isactive)
    {
        for (int i = 0; i < vuforia.Length; i++)
        {
            GameObject go = vuforia[i].gameObject;

            Camera[] cs = go.GetComponentsInChildren<Camera>();
            for (int j = 0; j < cs.Length; j++)
            {
                cs[i].enabled = isactive;
            }

            go.SetActive(isactive);        }
    }

    private void DestroyVuforia()
    {
        for (int i = 0; i < vuforia.Length; i++)
        {
            GameObject go = vuforia[i].gameObject;
            Destroy(go);
        }
    }


    void MoveToNewPos(Transform direction)
    {
        for (int i = 0; i < movetargets.Count; i++)
        {
            if (movetargets[i].GetComponent<SyncedRobotController>())
            {
                movetargets[i].position = Vector3.zero;

                movetargets[i].forward = -direction.up;
                Vector3 transfer = direction.position
                    + (movetargets[i].forward * 0.045f)//0.1122713f 
                    + (movetargets[i].up * -0.0915447182f) 
                    + (movetargets[i].right * -0.1f);//-0.06614f

                movetargets[i].position += transfer;
            }
        }
    }

    // Use this for initialization
    void Awake()
    {
        Traking(false);
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        Camera.main.GetComponent<CameraRotation>().enabled = true;
        Camera.main.GetComponent<CameraTouchRotator>().enabled = true;
#endif

        remindText = imageTarget.Find("GizmoPointer/RemindText").GetComponent<TextMesh>();
        debuggermesh = GameObject.Find("FPSDisplay/FPSText").GetComponent<TextMesh>();

        trakerrenderer = offsetpos.GetComponent<MeshRenderer>();//remindText.GetComponent<MeshRenderer>();
#if NETFX_CORE

        // 创建手势识别对象
        recognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
        // 设置手势识别的类型
        recognizer.SetRecognizableGestures(UnityEngine.XR.WSA.Input.GestureSettings.Tap | UnityEngine.XR.WSA.Input.GestureSettings.Hold | UnityEngine.XR.WSA.Input.GestureSettings.DoubleTap);
        // 添加手势识别的事件
        recognizer.TappedEvent += Recognizer_TappedEvent;
        // 开启手势识别
        recognizer.StartCapturingGestures();

#endif


    }

#if NETFX_CORE
    private void Recognizer_TappedEvent(UnityEngine.XR.WSA.Input.InteractionSourceKind source, int tapCount, Ray headRay)
    {
        if (imageTarget.gameObject.activeSelf && trakerrenderer.enabled )
        {
            MoveToNewPos(imageTarget);
            if (callback != null)
            {
                callback();
             //   DestroyVuforia();
            }
        }
    }
#endif
    void Update ()
    {
		
	}
}
