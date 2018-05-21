
using UnityEngine;
using System.Text;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Net;
using Vuforia;
using HoloToolkit.Unity.SpatialMapping;


public class IPPanelController : MonoBehaviour
{
    private Animator panelAnimator;
    private string ipAddress;
    private Text ipAddressTextInput;
    private Text errorMessage;
    private Canvas selfcanvas;
	// Use this for initialization
	void Start ()
    {
        this.selfcanvas = GetComponent<Canvas>();
        this.panelAnimator = GetComponent<Animator>();
        this.ipAddressTextInput = transform.Find("IPAddressInput/Text").GetComponent<Text>();
        this.errorMessage = transform.Find("ErrorMessageBox").GetComponent<Text>();
        Run.After(3, () => { this.selfcanvas.enabled = true;OpenPanel(); });
    }

    public void OnOk()
    {
        var inputText = transform.Find("IPAddressInput/Text").GetComponent<Text>().text;
        if (String.IsNullOrEmpty(inputText))
        {
            DisplayErrorMessage("Please fill the IP Address");
        }
        else
        {
            IPAddress ipAddressInstance;
            if (IPAddress.TryParse(inputText, out ipAddressInstance))
            {
                this.ipAddress = inputText;
                Debug.Log("IP input valid: " + inputText);
            }
            else
            {
                DisplayErrorMessage("The IP address is incorrect");
            }
        }
        PlayEffect("buttonUi");
    }

    public void OnSoloMode()
    {
        RobotsMain.Instance.issolomode = true;
        GameObject.Find("LoadingDialog").GetComponent<Canvas>().enabled = false;
        HoloToolkit.Sharing.SharingStage.Instance.ConnectSolo();
#if !NETFX_CORE
        RobotsMain.Instance.SetupRobotScene();
#else
        SolomodeSpatialmapping.Instance.StartObserve();
#endif
    }

    private void DisplayErrorMessage(string errorMessage)
    {
        if (null == this.errorMessage)
        {
            this.errorMessage = transform.Find("ErrorMessageBox").GetComponent<Text>();
        }
        this.errorMessage.text = errorMessage;
    }

    internal void OpenPanel()
    {
        this.ipAddress = null;
        if (!this.panelAnimator )
        {
            this.panelAnimator = GetComponent<Animator>();
        }
        this.panelAnimator.SetBool("Open", true);
        PlayEffect("openInterface02");
#if NETFX_CORE
        AddString(ReadTxtHelper.Instance.Ip);
        OnOk();
#endif
    }

    internal string GetIPAddress()
    {
        return ipAddress;
    }

    internal void ClosePanel()
    {
        if (this.panelAnimator != null)
        {
            this.panelAnimator.SetBool("Open", false);
            PlayEffect("closeInterface02");
        }
    }

    // Callback from buttons
    public void AddString(string s)
    {
        transform.Find("IPAddressInput").GetComponent<InputField>().text = s;
    }

    // Callback from buttons
    public void Backspace()
    {
        if (ipAddressTextInput.text.Length > 0)
        {
            ipAddressTextInput.text = ipAddressTextInput.text.Substring(0, ipAddressTextInput.text.Length - 1);
        }
    }

    void PlayEffect(string path)
    {
        Sound.Instance.PlayerEffect(path);
    }

}
