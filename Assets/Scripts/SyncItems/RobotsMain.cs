using HoloToolkit.Sharing;
using HoloToolkit.Sharing.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Windows.Speech;
using Vuforia;
#if NETFX_CORE
using UnityEngine.XR.WSA.Input;
#endif

public class RobotsMain : Singleton<RobotsMain> {

    private SharingStage sharingStage;

    private KeywordRecognizer keywordRecognizer;
    private PrefabSpawnManager spawnManager;
    public bool RunAsHoloLensMode = false;
    public bool ForceMarkerInitialization = false;
    public string HololensAutoConnectIPAddress;

    [HideInInspector]
    public bool issolomode = false;

    public delegate void RobotReady(RobotControllerSimple robot);
    public event RobotReady OnRobotReady;

    private void Awake()
    {
        // Turn off the main console UI
        if (IsHoloLens())
        {
            GameObject.FindGameObjectWithTag("ConsoleUI").SetActive(false);
        }
        else {
            Camera.main.transform.SetParent(GameObject.Find("CameraRig").transform);
            Camera.main.transform.localPosition = Vector3.zero;
            // Camera.main.transform.localPosition = new Vector3(-0.524f, 0.41f, -1.328f);
        }
    }

    // Use this for initialization
    void Start ()
    {
#if !UNITY_EDITOR
        InitializeSpeechRecognigzer();
#endif
    }

    //private IEnumerator SetupConnections()
    //{
    //    //this.sharingStage.enabled = true;
    //    this.sharingStage.SharingManagerConnected += (sender, e) =>
    //    {
    //        var robot = GameObject.FindObjectOfType<SyncedRobotController>();
    //        if (IsHoloLens() && robot != null)
    //        {
    //            Destroy(robot.gameObject);
    //            WayPointManager.Instance.RemoveAllWayPoint();
    //        }
    //        //if (IsHoloLens())
    //        //{
    //        //    StartCoroutine(CreateHeadTracker());
    //        //}
    //        //else
    //        //{
    //        //    Run.After(1, () =>
    //        //    {
    //        //        if (WorldPositionTracker.Instance == null)
    //        //        {
    //        //            var syncWorldPosition = new SyncWorldPosition();
    //        //            spawnManager.Spawn(syncWorldPosition, Vector3.zero, Quaternion.identity, GameObject.FindGameObjectWithTag("WorldPosition"), "SyncedWorldPosition", false);
    //        //        }
    //        //    });
    //        //}
    //        //Run.After(1, () => SetupRobotScene());
    //    };
    //    yield return -1;
    //}



    public void SetupRobotScene(PositionRotation pr)
    {
        Camera.main.GetComponent<VuforiaBehaviour>().enabled = false;
        var robot = GameObject.FindObjectOfType<SyncedRobotController>();
        if (robot == null)
        {
            UnityEngine.Debug.Log("Building the Synced Robot");
#if NETFX_CORE
            gameObject.AddComponent<UnityEngine.XR.WSA.WorldAnchor>();
#endif
            var syncObject = new SyncRobot();
            var forwardrotation = Quaternion.Euler(pr.Forward);
            spawnManager.Spawn(syncObject, pr.Position, forwardrotation, this.gameObject, "SyncedRobot", false);
        }
    }


    public void SetupRobotScene()
    {
        // Create the robot here, only on Console (Non-HoloLens)
        // TODO: We just create a new one if there isn't one existed.
        Camera.main.GetComponent<VuforiaBehaviour>().enabled = false;
        var robot = GameObject.FindObjectOfType<SyncedRobotController>();
        if (robot == null)
        {
            UnityEngine.Debug.Log("Building the Synced Robot");
            var syncObject = new SyncRobot();
            Vector3 sponpos = Vector3.zero;
            spawnManager.Spawn(syncObject, sponpos, Quaternion.identity, this.gameObject, "SyncedRobot", false);
            ///暂时不生成桌子
            // Create predefined control points
            //var v1 = new Vector3(-0.726f, 0.161f, 0);
            //var v2 = new Vector3(-0.726f, 0.161f, 0.14f);
            //var v3 = new Vector3(-0.726f, 0.161f, -0.14f);
            //ItemManager.Instance.CreateItem(ItemManager.ItemType.bottle, v1);
            //ItemManager.Instance.CreateItem(ItemManager.ItemType.bottle, v2);
            //ItemManager.Instance.CreateItem(ItemManager.ItemType.bottle, v3);

            //var v4 = new Vector3(-0.726f, 0.161f, 0);
            //ItemManager.Instance.CreateItem(ItemManager.ItemType.table, v4);
        }
     }

    private IEnumerator CreateHeadTracker()
    {
        yield return new WaitForSeconds(2);
        UnityEngine.Debug.Log("Spawned head tracker");
        var syncObject = new SyncHeadPosition();
        spawnManager.Spawn(syncObject, Vector3.zero, Quaternion.identity, this.gameObject, "HeadTracker", true);
        while (true)
        {
            // Tracking the head positions
            syncObject.Transform.Position.Value = Camera.main.transform.position;
            syncObject.Transform.Rotation.Value = Camera.main.transform.rotation;
            yield return new WaitForEndOfFrame();
        }
    }

    private void InitializeGestureRecognizer()
    {
#if NETFX_CORE
        UnityEngine.XR.WSA.Input.GestureRecognizer gestureRecognizer = new UnityEngine.XR.WSA.Input.GestureRecognizer();
        gestureRecognizer.SetRecognizableGestures(UnityEngine.XR.WSA.Input.GestureSettings.Tap);
        gestureRecognizer.TappedEvent += (InteractionSourceKind source, int tapCount, Ray headRay) => {
            UnityEngine.Debug.Log("Tap detected");
            var selectedObject = EventSystem.current.currentSelectedGameObject;
            if (selectedObject != null)
            {
                var pointerEventData = new PointerEventData(EventSystem.current);
                ExecuteEvents.Execute(selectedObject, pointerEventData, ExecuteEvents.pointerClickHandler);
            }
        };
        gestureRecognizer.StartCapturingGestures();
#endif
    }

    private void InitializeSpeechRecognigzer()
    {
        Dictionary<string, Action> keywords = new Dictionary<string, Action>();
        keywords.Add("Create Sphere", () => {
            UnityEngine.Debug.Log("Creating sphere");
            spawnManager.Spawn(new SyncTestCube(), UnityEngine.Random.onUnitSphere * 2, UnityEngine.Random.rotation, this.gameObject, "TestCube", true);
        });
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += (PhraseRecognizedEventArgs args) => {
            Action act;
            if (keywords.TryGetValue(args.text, out act))
            {
                act();
            }
            else
            {
                UnityEngine.Debug.Log("Failed to find the keyword: " + args.text);
            }
        };
    }

    public void UpdateRobotReady(RobotControllerSimple robot)
    {
        // Notify others that the robot is ready
        if(null != OnRobotReady)
            this.OnRobotReady(robot);
    }

    public bool IsHoloLens()
    {
        return RunAsHoloLensMode || 
            (SystemInfo.deviceType == DeviceType.Desktop && 
            SystemInfo.deviceModel.StartsWith("HoloLens"));
    }

#if !NETFX_CORE
    private void LaunchSharingService()
    {
        string filePathName = @"External\HoloToolkit\Sharing\Server\SharingService.exe";

        if (!filePathName.StartsWith(@"\"))
        {
            filePathName = @"\" + filePathName;
        }
        string searchDir = "";
        if (string.IsNullOrEmpty(searchDir))
        {
            searchDir = Application.dataPath.Replace("/", @"\");
        }
        string appPath = searchDir + filePathName;

        string args = @"-local";
        Process pr = new Process();
        pr.StartInfo.FileName = appPath;
        pr.StartInfo.WorkingDirectory = searchDir;
        pr.StartInfo.Arguments = args;

        pr.Start();
    }
#endif
}