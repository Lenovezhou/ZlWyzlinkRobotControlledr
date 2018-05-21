using HoloToolkit.Sharing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using Vuforia;
using SpectatorView;

public class LoadingDialog : MonoBehaviour
{
    private SharingStage sharingStage;
    private Text messageText;
    private IPPanelController ipPanelController;
    private MarkerTracker markerTrackerPanel;
    private Animator messagePanelAnimator;
    private bool _isinitalized = false;

    private void OnEnable()
    {
        if(_isinitalized)
            StartCoroutine(StartLoadingProcess());
    }
    private void Start()
    {
        messagePanelAnimator = transform.Find("MessagePanel").GetComponent<Animator>();
        messageText = transform.Find("MessagePanel/Message").GetComponent<Text>();
        this.ipPanelController = transform.Find("IPPanel").GetComponent<IPPanelController>();
        this.ipPanelController.gameObject.SetActive(false);
        this.markerTrackerPanel = transform.Find("AlignmentPanel").GetComponent<MarkerTracker>();
        this.markerTrackerPanel.gameObject.SetActive(false);
        Debug.Assert(messageText != null);


        NetworkDiscoveryWithAnchors discovery = NetworkDiscoveryWithAnchors.Instance;
        if (discovery == null)
        {
            LogMessageToPanel("Failed to load discovery");
        }
        else {
            StartCoroutine(StartLoadingProcess());
        }
        _isinitalized = true;
    }


    private IEnumerator StartLoadingProcess()
    {
        if (IsHoloLens())
        {
            OpenPanel();
            var waitForSeconds = new WaitForSeconds(0.3f);

            while (!NetworkDiscoveryWithAnchors.Instance.Connected)
            {
                Debug.Log("NetworkDiscoveryWithAnchors.Instance.NotConnected");
                yield return waitForSeconds;
            }
            Debug.Log("NetworkDiscoveryWithAnchors.Instance.Connected");

            #region Connect Server with SharingStage

            //yield return waitForSeconds;
            //yield return waitForSeconds;
            //string ipAddress = GetAutoConnectIPAddress();
            //LogMessageToPanel("Connecting to Server on " + ipAddress);

            //yield return -1;

            //// Collect the IP address and make the connection
            //while (!this.sharingStage.IsConnected)
            //{
            //    if (ipAddress == null)
            //    {
            //        this.ipPanelController.gameObject.SetActive(true);
            //        this.ipPanelController.OpenPanel();
            //        while (ipAddress == null)
            //        {
            //            ipAddress = this.ipPanelController.GetIPAddress();
            //            yield return waitForSeconds;
            //        }
            //    }
            //    Debug.Log("Received dialog IP address: " + ipAddress);
            //    this.sharingStage.ServerAddress = ipAddress;
            //    this.sharingStage.ConnectToServer(ipAddress, 20602);
            //    this.sharingStage.enabled = true;
            //    ipAddress = null;
            //    // Wait for 4 seconds for connection timeout
            //    for (int i = 0; i < 8 && !this.sharingStage.IsConnected; i++)
            //    {
            //        yield return waitForSeconds;
            //    }
            //}
            //LogMessageToPanel("Connected to the server");
            //this.ipPanelController.ClosePanel();
            //this.ipPanelController.gameObject.SetActive(false);
            //yield return -1;

            #endregion

            if (RobotsMain.Instance.ForceMarkerInitialization)
            {

                //if (this.sharingStage.SessionUsersTracker.CurrentUsers.Count != 0)
                {
                    // Wait for the network initialization and remote spawning
                    LogMessageToPanel("Waiting for networking initialization");
                    yield return new WaitForSeconds(2.0f);
                }

                // TODO: Should consider the use case of multi-hololens. What if the other hololens joined
                // while we position the robot?
                LogMessageToPanel("Locating robot positions");
                var robot = GameObject.FindObjectOfType<SyncedRobotController>();

                StartCoroutine(ShowRobot(null != robot));
            }
            else
            {
                LogMessageToPanel("Seting up robot");
                Run.After(1, () => RobotsMain.Instance.SetupRobotScene());
            }
        }
        else
        {
            //LogMessageToPanel("Connecting to Server on " + this.sharingStage.ServerAddress);
            //this.sharingStage.enabled = true;
        }
        Camera.main.GetComponent<VuforiaBehaviour>().enabled = false;

        ClosePanel();
    }


    private IEnumerator ShowRobot(bool isalredyspawn)
    {
        var waitForSeconds = new WaitForSeconds(0.3f);
        // Start Vuforia SDK to detect the targets
        LogMessageToPanel("Please look at the Robot Arm ...");
        this.markerTrackerPanel.gameObject.SetActive(true);
        this.markerTrackerPanel.gameObject.GetComponent<MarkerTracker>().StartTraking();
        yield return -1;

        while (!markerTrackerPanel.FinishedDetection())
        {
            yield return waitForSeconds;
        }

        this.markerTrackerPanel.gameObject.SetActive(false);
        // Move the center to the position
        var result = markerTrackerPanel.GetTrackingPositionRotation();

        Vector3 offsetPosition = ReadTxtHelper.Instance.OffsetPosition;
        Vector3 offsetRotation = ReadTxtHelper.Instance.OffsetRotation;


        //RobotsMain没有同步位置的功能，所以将得到的数据传递给生成函数
        RobotsMain.Instance.transform.position = result.Position;
        Quaternion qua = Quaternion.Euler(result.Forward);
        //RobotsMain.Instance.transform.rotation = qua;
        RobotsMain.Instance.transform.right = -result.Forward;

        //矫正位置
        RobotsMain.Instance.transform.Rotate(Vector3.up, offsetRotation.y);

        Vector3 robotforward = RobotsMain.Instance.transform.forward.normalized;
        Vector3 robotup = RobotsMain.Instance.transform.up.normalized;
        Vector3 robotright = RobotsMain.Instance.transform.right.normalized;

        Vector3 offset = robotforward * offsetPosition.z + robotup * offsetPosition.y + robotright * offsetPosition.x;

        RobotsMain.Instance.transform.position += offset;

        // TODO: Add WorldAnchor At RobotMainAnchorControll
        GameObject _anchor = new GameObject();
        _anchor.transform.SetParent(null);
        _anchor.transform.position = RobotsMain.Instance.transform.position;
        _anchor.transform.localScale = Vector3.one * 0.1f;
        _anchor.AddComponent<RobotMainAnchorControll>();

        if (!isalredyspawn)
        {
            Vector3 pos = RobotsMain.Instance.transform.position;
            Vector3 forward = RobotsMain.Instance.transform.forward;
            // Ready to setup the scene
            //RobotsMain.Instance.SetupRobotScene();
            PlayerController.Instance.CmdSetupRobotScene(pos, forward);
        }
        Run.After(2, () => gameObject.SetActive(false));
    }

    private void OpenPanel()
    {
        messagePanelAnimator.SetBool("Open", true);
    }

    private void ClosePanel()
    {
        messagePanelAnimator.SetBool("Open", false);
    }

    private string GetAutoConnectIPAddress()
    {
        IPAddress address;
        string ipAddress = RobotsMain.Instance.HololensAutoConnectIPAddress;
        if (!string.IsNullOrEmpty(ipAddress) && IPAddress.TryParse(ipAddress, out address))
        {
            return ipAddress;
        }
        return null;
    }

    private void SharingStage_SharingManagerConnected(object sender, System.EventArgs e)
    {
        //LogMessageToPanel("The server is connected.");
    }

    private bool IsHoloLens()
    {
        return RobotsMain.Instance.IsHoloLens();
    }

    private void LogMessageToPanel(string message)
    {
        this.messageText.text = message;
    }
}
