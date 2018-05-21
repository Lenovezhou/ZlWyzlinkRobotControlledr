using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProgramEditor : MonoBehaviour
{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
    private LoadRobotProgram ProgramLoader;
    [SerializeField]
    private InputField EditableTextBox;
    [SerializeField]
    private InputField IPaddressInput;
    [SerializeField]
    private InputField PortInput;
    [SerializeField]
    private RectTransform ScrollContent;
    [SerializeField]
    private Text ErrorMessageText;
    
    private void Start()
    {
        ProgramLoader = GetComponent<LoadRobotProgram>();
        Debug.Assert(ProgramLoader != null);
        UpdateEditableProgramText();
    }

    public void UpdateEditableProgramText()
    {
        EditableTextBox.text = ProgramLoader.GetProgramTextRaw();
    }

    public void SaveFile()
    {
        ProgramLoader.SaveFile(EditableTextBox.text);
    }

    public void RunProgram()
    {
        SaveFile();
        ProgramLoader.RunProgram();
    }

    public void Stop()
    {
        ProgramLoader.StopProgram();
    }

    public void NewProgram()
    {
        Stop();
        ProgramLoader.NewProgram();
    }

    public void SwitchProgram(string programName)
    {
        ProgramLoader.SetAndLoadProgram(programName);
        UpdateEditableProgramText();
    }
#else
    public void SwitchProgram(string programName)
    {
    }
#endif
}
