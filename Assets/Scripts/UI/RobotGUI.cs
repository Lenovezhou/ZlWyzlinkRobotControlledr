using HoloToolkit.Sharing;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Windows.Speech;
using System.Linq;
using SpectatorView;

public class RobotGUI : MonoBehaviour
{
    private SyncedRobotController syncedRobot;
    private RobotControllerSimple robot;
    private TransformSynchronizer robotsync;
    private RobotMainAnchorControll robotanchor;
    private RobotAggregator robotAggregator;
    [SerializeField]
    Button AddGizmosButton;
    [SerializeField]
    Button PlayStopButton;
    [SerializeField]
    Button HomeButton;
    [SerializeField]
    Button DancingPathButton;
    [SerializeField]
    Button BottlePathButton;
    [SerializeField]
    Button Switch_Vuforia;
    [SerializeField]
    Text stateText;
    [SerializeField]
    Sprite StopIcon;
    [SerializeField]
    Sprite PlayIcon;

    [SerializeField]
    Transform path;
    Transform pathdance;
    Transform pathbottle;

    //unity
    [SerializeField]
    Button savepath;
    GameObject LoadingPanel;

    private string previousLine;

    private bool isdancing = false;
    private bool isgetbottle = false;

    private ArmPath armpath = ArmPath.None;
    private ArmPath lastchoise = ArmPath.None;


    /*舞蹈自动执行*/
    private Run run;
    private KeywordRecognizer keywordRecognizer;

    private void Awake()
    {
        LoadingPanel = GameObject.Find("LoadingDialog");
        if (LoadingPanel)
            LoadingPanel.gameObject.SetActive(false);
    }

    private void Start()
    {
        robotanchor = FindObjectOfType<RobotMainAnchorControll>();
        robot = this.transform.parent.FindChildOfType<RobotControllerSimple>();
        this.robotAggregator = this.robot.transform.parent.GetComponentInChildren<RobotAggregator>();
        robotsync = this.transform.parent.GetComponent<TransformSynchronizer>();
        this.syncedRobot = this.transform.parent.GetComponent<SyncedRobotController>();
        pathdance = path.Find("PathDancing").transform;
        pathbottle = path.Find("PathGetBottle").transform;
        AddGizmosButton.onClick.AddListener(CreateGizmo);
        PlayStopButton.onClick.AddListener(IKMoveToTargets);
        HomeButton.onClick.AddListener(Home);
        Switch_Vuforia.onClick.AddListener(SwitchVuforia);
        DancingPathButton.onClick.AddListener(()=> {StartCoroutine( GetPath(ArmPath.Dancing)); });
        BottlePathButton.onClick.AddListener(()=> { StartCoroutine( GetPath(ArmPath.GetBottle)); });
        savepath.onClick.AddListener(SavePath);
        InitializeSpeechRecognigzer();

#if !UNITY_EDITOR
        savepath.gameObject.SetActive(false);
#endif

    }

    public void CreateGizmo()
    {
        Transform parent = WayPointManager.Instance.transform;

        var temptarget = new Vector3(-0.5f + parent.childCount * -0.03f, 0.5f, parent.childCount * 0.1f - 0.2f);
        var targetpos = parent.TransformPoint(temptarget);
        PlayerController.Instance.CmdSpawnGizmos(parent.forward,AddGizmosButton.transform.position, targetpos, GizmoController.GizmoPostAction.None,false);
        //WayPointManager.Instance.CreateWayPoint(AddGizmosButton.transform.position,transform.parent);
    }
    public void IKMoveToTargets()
    {
        if (!robot.IsRunning)
        {
            PlayerController.Instance.CmdSetProgramLine ("IKMoveToTarget");
            PlayBG(lastchoise);
        }
    }
    public void Home()
    {
        armpath = ArmPath.Home;
        PlayerController.Instance.CmdSetProgramLine("HOME");

        //TODO ------>>>> robot.Stop();


        //if (!robot.IsRunning)
        //{
        //    StopAllCoroutines();
        //    robot.RunProgramLine("HOME");
        //    PlayBG(ArmPath.Home);
        //    OperationButtonInteractive(true);
        //}
        //else if(isdancing)
        //{
        //    robot.Stop();
        //    PlayBG(ArmPath.Home);
        //    StopAllCoroutines();
        //    OperationButtonInteractive(false,HomeButton);
        //}
    }


    /*编辑器状态下的保存按钮*/
    private void SavePath()
    {
        Transform waypointmanager = WayPointManager.Instance.transform;
        int count = waypointmanager.childCount;
        Transform robot = transform.parent;

        Transform parent = pathdance;

        for (int i = 0; i < count; i++)
        {
            Transform w = waypointmanager.GetChild(i);
            Vector3 pos = w.transform.position;//robot.TransformPoint(w.position);
            GameObject go = null;
            if (parent.childCount > i)
            {
                go = parent.GetChild(i).gameObject;
            }
            else {
                go = new GameObject("pathpoint" + i);
                go.transform.SetParent(parent);
            }
            go.transform.position = pos;

            PathPoint pp = go.GetComponent<PathPoint>();

            if (null == pp)
            {
                pp = go.AddComponent<PathPoint>();
            }
            pp.Forward = -go.transform.localEulerAngles;
            pp.GetGizmopostaction = w.GetComponent<GizmoController>().GetPoatAction;
            
        }

    }

    /*获取已经保存的路径*/
    private IEnumerator GetPath(ArmPath ap, Action callback = null)
    {
        if (!robot.IsRunning)
        {
            Transform gizmoparent = WayPointManager.Instance.transform;
            OperationButtonInteractive(false);
            yield return (WayPointManager.Instance.RemoveAllWayPoint());
            Transform parent = null;
            Vector3 flystartpos = Vector3.zero;
            armpath = ap;
            lastchoise = armpath;
            isdancing = (ap == ArmPath.Dancing);
            switch (ap)
            {
                case ArmPath.Dancing:
                    syncedRobot.SetDacncingState (true);
                    parent = pathdance;
                    flystartpos = DancingPathButton.transform.position;
                    /*10s后自动开始dancing*/
                    this.run = Run.After(10, () => 
                    {
                        PlayStopButton.onClick.Invoke();
                    });
                    break;
                case ArmPath.GetBottle:
                    syncedRobot.SetDacncingState(false);
                    parent = pathbottle;
                    flystartpos = BottlePathButton.transform.position;
                    if (null != this.run)
                    {
                        Run.DeletRun(run);
                    }
                    break;
                default:
                    break;
            }

            int length = parent.childCount;

            List<float> dancedurations = ReadTxtHelper.Instance.danceDurations;

            for (int i = 0; i < length; i++)
            {
                Transform child = parent.GetChild(i);
                PathPoint pp = child.GetComponent<PathPoint>();
                Vector3 position = child.position;
                PlayOneShout();
                Vector3 forward = transform.parent.forward;
                if (ap == ArmPath.Dancing)
                {
                    forward = pp.Forward;
                    if (dancedurations.Count > i)
                    {
                        pp.Duration = dancedurations[i];
                    }
                }

                PlayerController.Instance.CmdSpawnGizmos(gizmoparent.forward, flystartpos, position, pp.GetGizmopostaction, ap == ArmPath.Dancing);
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(1f);
            OperationButtonInteractive(true);
        }
    }

    /*点击定位按钮调用*/
    public void SwitchVuforia()
    {
        robotanchor.PullRobotAtHere();
    }

    /*实例预存路径过程中防止误操作操作*/
    private void OperationButtonInteractive(bool ison,Button excepted = null)
    {
        Button[] buttons = GetComponentsInChildren<Button>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i] != excepted)
            {
                buttons[i].interactable = ison;
            }
        }
        Toggle[] toggles = GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggles.Length; i++)
        {
            toggles[i].interactable = ison;
        }
    }

    private void Update()
    {
        string text = robot.IsRunning ? "Running: " : "Done!";
        text += robot.RunningProgramLine;
        if (string.Compare(previousLine, text) != 0)
        {
            this.stateText.text = previousLine + "\n" + text;
            previousLine = text;
        }
    }

    void PlayBG(ArmPath ap)
    {
        //TODO ----->>>>   robot.EnableMeshRenderer
        switch (ap)
        {
            case ArmPath.Home:
                //robot.EnableMeshRenderer(true);
                break;
            case ArmPath.Dancing:
                //robot.EnableMeshRenderer(issolomode);
                break;
            case ArmPath.GetBottle:
                //robot.EnableMeshRenderer(issolomode);
                break;
        }
        PlayerController.Instance.SyncPlaySound(isdancing);
    }

    void PlayOneShout()
    {
        Sound.Instance.PlayerOneShout("buttonUi",transform.position);
    }

    private void InitializeSpeechRecognigzer()
    {
        Dictionary<string, Action> keywords = new Dictionary<string, Action>();
        keywords.Add("Start", () => {
            PlayStopButton.onClick.Invoke();
        });
        keywords.Add("Reset",() => {
            HomeButton.onClick.Invoke();
        });
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += (PhraseRecognizedEventArgs args) => {
            Action act;
            if (keywords.TryGetValue(args.text, out act))
            {
                act();
            }
        };
        keywordRecognizer.Start();
    }


}
