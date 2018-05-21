using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class LoadRobotProgram : MonoBehaviour
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    RobotControllerSimple Robot;
    RobotAggregator robotAggregator;

    string defaultProgramString = "HOME\r\nwait 1\r\nGetAxisAngle\r\nwait 1\r\nMOTO_ON\r\nwait 1\r\nMOTO_OFF\r\nwait 1\r\nIO_OUT true\r\nwait 1\r\nIO_OUT false\r\nwait 1\r\nIO_IN\r\nwait 1\r\nMoveAxisRel 45, 0, 0, 0, 0, 0\r\nwait 1\r\nMoveAxisRel 45, 0, 0, 0, 0, 0\r\nwait 1\r\nHOME\r\nwait 1\r\nMoveAxisAbs 45, 0, 0, 0, 0, 0\r\nwait 1\r\nMoveAxisAbs 45, 0, 0, 0, 0, 0\r\nwait 1\r\nHOME\r\n\r\nSetMaxAngleSpeed 0 30\r\nSetMaxAngleSpeed 1 30\r\nSetMaxAngleSpeed 2 30\r\nSetMaxAngleSpeed 3 30\r\nSetMaxAngleSpeed 4 30\r\nSetMaxAngleSpeed 5 30\r\n\r\nmoveJoint 1 30 1\r\nmoveJoint 2 60 1\r\nmoveJoint 0 60 1\r\nwait 1\r\nrepeat 2 3\r\nmoveJoint 2 -30 1\r\nmoveJoint 2 30 2\r\n";

    string currentProgramName;
    private string ProgramTextRaw;
    private string[] LoadedProgramLines;
    private int commandCount = 0;
    private int programJointCount = 0;
    private string defaultProgramName = "DefaultProgram.txt";

    [SerializeField]
    UnityEngine.UI.Text AvailableProgramsText;

    [SerializeField]
    UnityEngine.UI.Button ProgramButtonPrefab;
    [SerializeField]
    GameObject EditorPanel;
    private ProgramEditor programEditor;

    private string DocumentsPath;
    private List<string> ProgramFilesInFolder = new List<string>();
    private List<UnityEngine.UI.Button> ProgramButtons = new List<UnityEngine.UI.Button>();

    private Coroutine programRunningCoroutine;
    private bool cancelExecution = false;
    public bool isRunning { get; private set; }

    private void Start()
    {
        this.isRunning = false;
        RobotsMain.Instance.OnRobotReady += RobotMain_OnRobotReady;
    }

    private void RobotMain_OnRobotReady(RobotControllerSimple robot)
    {
        RobotsMain.Instance.OnRobotReady -= RobotMain_OnRobotReady;

        this.Robot = robot;
        this.robotAggregator = this.Robot.transform.parent.GetComponentInChildren<RobotAggregator>();
        Debug.Assert(this.robotAggregator != null);
        this.programEditor = GetComponent<ProgramEditor>();
        Debug.Assert(this.programEditor != null);

        DocumentsPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "/RobotController/";
        UpdateProgramButtons();
        SetAndLoadProgram(ProgramFilesInFolder[0]);
    }

    private void UpdateProgramButtons()
    {
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(DocumentsPath);

        if (dir.Exists == false)
        {
            System.IO.Directory.CreateDirectory(DocumentsPath);

            System.IO.File.WriteAllText(DocumentsPath + defaultProgramName, defaultProgramString);
            ProgramFilesInFolder.Add(defaultProgramName);

            UnityEngine.UI.Button button = Instantiate(ProgramButtonPrefab, EditorPanel.transform);
            button.GetComponentInChildren<UnityEngine.UI.Text>().text = defaultProgramName;
            button.GetComponent<ButtonSendText>().SetText(defaultProgramName);
            button.GetComponent<ButtonSendText>().SetEditor(programEditor);
            ProgramButtons.Add(button);
        }
        else
        {
            System.IO.FileInfo[] info = dir.GetFiles("*.*");

            if (info.GetLength(0) == 0)
            {
                System.IO.File.WriteAllText(DocumentsPath + defaultProgramName, defaultProgramString);
                ProgramFilesInFolder.Add(defaultProgramName);

                UnityEngine.UI.Button button = Instantiate(ProgramButtonPrefab, EditorPanel.transform);
                button.GetComponentInChildren<UnityEngine.UI.Text>().text = defaultProgramName;
                button.GetComponent<ButtonSendText>().SetText(defaultProgramName);
                button.GetComponent<ButtonSendText>().SetEditor(programEditor);
                ProgramButtons.Add(button);
            }
            else
            {
                for (int i = 0; i < info.GetLength(0); i++)
                {
                    if (ProgramFilesInFolder.Contains(info[i].Name) == false)
                    {
                        ProgramFilesInFolder.Add(info[i].Name);

                        UnityEngine.UI.Button button = Instantiate(ProgramButtonPrefab, EditorPanel.transform);
                        button.GetComponentInChildren<UnityEngine.UI.Text>().text = info[i].Name;
                        button.GetComponent<ButtonSendText>().SetText(info[i].Name);
                        button.GetComponent<ButtonSendText>().SetEditor(programEditor);
                        ProgramButtons.Add(button);
                    }
                }
            }
        }
    }

    public void NewProgram()
    {
        System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(DocumentsPath);

        if (dir.Exists == false)
        {
            System.IO.Directory.CreateDirectory(DocumentsPath);
        }

        string newProgramString = "NewProgram";
        string tryName = newProgramString + ".txt";
        int countTries = 0;
        while (true)
        {
            countTries++;

            if (System.IO.File.Exists(DocumentsPath + tryName))
            {
                tryName = newProgramString + countTries.ToString() + ".txt";
            }
            else
            {
                break;
            }
        }

        System.IO.File.WriteAllText(DocumentsPath + tryName, defaultProgramString);
        UpdateProgramButtons();
        SetAndLoadProgram(tryName);
    }

    public void RunProgram()
    {
        this.programRunningCoroutine = StartCoroutine(RunLoadedProgram());
    }

    public Coroutine StopProgram()
    {
        this.cancelExecution = true;
        return StartCoroutine(WaitUntilFinish());
    }

    private IEnumerator WaitUntilFinish()
    {
        while (this.isRunning)
        {
            yield return new WaitForSeconds(0.3f);
        }
    }

    public int GetCommandCount()
    {
        return this.commandCount;
    }
    public int GetJointCount()
    {
        return this.programJointCount;
    }
    public string GetProgramTextRaw()
    {
        return this.ProgramTextRaw;
    }
    
    public void SaveFile(string s)
    {
        System.IO.File.WriteAllText(DocumentsPath + currentProgramName, s);
    }

    public void SetAndLoadProgram(string filename)
    {
        SetCurrentProgramText(filename);
        LoadProgramFromFile();
    }

    private void SetCurrentProgramText(string filename)
    {
        currentProgramName = filename;
    }

    private void LoadProgramFromFile()
    {
        LoadedProgramLines = System.IO.File.ReadAllLines(DocumentsPath + currentProgramName);
        ProgramTextRaw = System.IO.File.ReadAllText(DocumentsPath + currentProgramName);
    }

    private IEnumerator RunLoadedProgram()
    {
        Debug.Assert(LoadedProgramLines.Length > 0);
        Debug.Assert(currentProgramName.Length > 0);

        this.isRunning = true;
        this.cancelExecution = false;
        
        for (int i = 0; i < LoadedProgramLines.Length && !cancelExecution; i++)
        {
            string[] tokens = LoadedProgramLines[i].Split();

            switch (tokens[0])
            {
                case "HOME":
                case "GetAxisAngle":
                case "MovePos":
                case "MOTO_ON":
                case "MOTO_OFF":
                ////////////////////
                case "IO_IN":
                case "IO_OUT":
                ////////////////////
                case "SetMaxAngleSpeed":
                case "SetMaxSpeed":
                ////////////////////
                case "MoveAbs":
                case "MoveRel":
                ////////////////////
                case "MoveAxisAbs":
                case "MoveAxisRel":
                    var result = this.robotAggregator.RunProgramLine(LoadedProgramLines[i]);
                    yield return result;
                    if (!result.IsSucceeded)
                    {
                        Debug.Log("Failed running program line " + LoadedProgramLines[i] + "\n" + result.ErrorMessage);
                    }
                    break;
                ////////////////////


                    // old commands
                case "moveJoint":
                    Debug.Log("Need new implementation!!!");
                    break;
                case "repeat":
                    Debug.Log("Need new implementation!!!");
                    break;
                case "wait":
                    Debug.Log("Need new implementation!!!");
                    break;
                case "move":
                    Debug.Log("Need new implementation!!!");
                    break;

                case "positions":
                    Debug.Log("Need new implementation!!!");
                    break;
                    //while (true) // break when we can't find a point to read
                    //{
                    //    i++; // go to next line
                    //    tokens = lines[i].Split(); // get the tokens
                    //    const int leadingTokens = 2; // 'p0' and '='
                    //    int checkJoints = tokens.Length - leadingTokens;

                    //    if (checkJoints == Robot.GetNumberOfJoints())
                    //    {
                    //        Robot.AddPosition(ref tokens);
                    //        programJointCount = checkJoints;
                    //    }
                    //    else
                    //    {
                    //        break; // not a point definition
                    //    }
                    //}
                    //break;

                default:
                    if (!string.IsNullOrEmpty(tokens[0]))
                    {
                        Debug.Log("[Command Unrecognized] " + tokens[0] + "\n");
                    }
                    break;
            }
        }
        this.isRunning = false;
    }
#endif    
}
