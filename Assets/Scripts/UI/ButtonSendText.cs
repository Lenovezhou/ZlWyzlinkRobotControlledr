using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSendText : MonoBehaviour
{
    public UnityEngine.UI.Text textToSend;
    public ProgramEditor pe;
        
    public void SendText()
    {
        pe.SwitchProgram(textToSend.text);
    }

    public void SetText(string s)
    {
        textToSend.text = s;
    }

    public void SetEditor(ProgramEditor pr)
    {
        pe = pr;
    }
}
