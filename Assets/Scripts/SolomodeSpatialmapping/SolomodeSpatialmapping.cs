using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if NETFX_CORE
using UnityEngine.XR.WSA.Input;
#endif

public class SolomodeSpatialmapping : Singleton<SolomodeSpatialmapping>, IInputClickHandler
{
    public Transform cursor;
    public void OnInputClicked(InputClickedEventData eventData)
    {
#if NETFX_CORE
        RobotsMain.Instance.issolomode = true;
        SpectatorView.NetworkDiscoveryWithAnchors.Instance.StartHosting("SpectatorViewSession");
        StartObserve();
        //Camera.main.GetComponent<Vuforia.VuforiaBehaviour>().enabled = false;
#endif
    }


    private void Update()
    {
        #region ToLenovozhou Test
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (SpectatorView.NetworkDiscoveryWithAnchors.Instance.running)
            {
                RobotsMain.Instance.issolomode = true;
                SpectatorView.NetworkDiscoveryWithAnchors.Instance.StartHosting("SpectatorViewSession");

                Run.After(1, () => {
                    SpectatorView.PlayerController.Instance.CmdSetupRobotScene(Camera.main.transform.forward, Camera.main.transform.forward);
                });
            }
         
        }
        #endregion
    }


#if NETFX_CORE
    private GestureRecognizer gestureRecognizer;


    public void StartObserve()
    {
        SpatialMappingManager.Instance.StartObserver();
        RigisterGesture();
    }

    public void StopObserve()
    {
        SpatialMappingManager.Instance.DrawVisualMeshes = false;
        SpatialMappingManager.Instance.StopObserver();
        gestureRecognizer.TappedEvent -= GestureRecognizer_TappedEvent;
    }


    void RigisterGesture()
    {
        //  创建GestureRecognizer实例  
        gestureRecognizer = new GestureRecognizer();
        //  注册指定的手势类型,本例指定单击及双击手势类型  
        gestureRecognizer.SetRecognizableGestures(GestureSettings.Tap
            | GestureSettings.DoubleTap);
        //  订阅手势事件  
        gestureRecognizer.TappedEvent += GestureRecognizer_TappedEvent;

        //  开始手势识别  
        gestureRecognizer.StartCapturingGestures();
    }

    private void GestureRecognizer_TappedEvent(InteractionSourceKind source, int tapCount, Ray headRay)
    {
        StopObserve();
        var tempderection = Camera.main.transform.right;
        var derection = new Vector3(tempderection.x,0, tempderection.z);
        SpectatorView.PlayerController.Instance.CmdSetupRobotScene(cursor.position, derection);
    }
#endif
}
