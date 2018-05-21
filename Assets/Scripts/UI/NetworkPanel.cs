using HoloToolkit.Sharing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class NetworkPanel : MonoBehaviour
{
    private RemoteRobotController remoteRobot;
    private RobotControllerSimple robotController;
    private Transform usersPanel;
    private Transform userTemplate;

    private Button syncButton;
    private InputField ipInputField;
    private InputField portInputField;
    private Text statusText;
    private Color statusStartColor;

    // User notifications
    private SessionUsersTracker usersTracker;
    private static User localUser = null;
    private Dictionary<int, GameObject> userDict = new Dictionary<int, GameObject>();
    private Transform usersPanelContainer;

    public void Start()
    {
        this.syncButton = this.transform.Find("Panel/NetworkingGroup/ConnectButton").GetComponent<Button>();
        this.ipInputField = this.transform.Find("Panel/NetworkingGroup/IPaddressInput").GetComponent<InputField>();
        this.portInputField = this.transform.Find("Panel/NetworkingGroup/PortInput").GetComponent<InputField>();
        this.statusText = this.transform.Find("Panel/NetworkingGroup/Status/Text").GetComponent<Text>();
        this.usersPanel = this.transform.Find("Panel/NetworkingGroup/UsersPanel");
        this.usersPanelContainer = usersPanel.Find("UserListContainer");
        this.userTemplate = this.usersPanel.Find("UserIcon");
        Debug.Assert(this.syncButton != null && this.ipInputField != null && this.portInputField != null && this.statusText != null);
        Debug.Assert(this.usersPanel != null && this.userTemplate != null);
        statusStartColor = statusText.color;
        
        RobotsMain.Instance.OnRobotReady += RobotsMain_OnRobotReady;
        // SharingStage should be valid at this point, but we may not be connected.
        //if (SharingStage.Instance.IsConnected)
        //{
        //    Connected(null, null);
        //}
        //else
        //{
        //    SharingStage.Instance.SharingManagerConnected += Connected;
        //}
    }

    private void RobotsMain_OnRobotReady(RobotControllerSimple robot)
    {
        RobotsMain.Instance.OnRobotReady -= RobotsMain_OnRobotReady;
        this.robotController = robot;
        this.remoteRobot = robot.GetComponent<RemoteRobotController>();
        Debug.Assert(this.robotController != null && this.remoteRobot != null);

        // Enable Sync button
        this.syncButton.interactable = true;
        this.syncButton.onClick.AddListener(() =>
        {
            this.OnSyncButton();
        });
        syncButton.onClick.Invoke();
    }

    public void OnSyncButton()
    {
        var syncOn = remoteRobot.isSyncOn();
        if (syncOn)
        {
            // Turn off the motor
            remoteRobot.SetSyncOn(false);
            syncButton.GetComponent<Image>().color = Color.white;
        }
        else
        {
            if (remoteRobot.IsConnected())
            {
                syncButton.GetComponent<Image>().color = Color.green;
                remoteRobot.SetSyncOn(true);

                Debug.Log("Syncing virtual robot position to real robot position.");
                remoteRobot.GetAxisAngle((angles) => {
                    MainThreadDispatcher.QueueTask(() =>
                    {
                        for (int i = 0; i < remoteRobot.NumberOfJoints; i++)
                        {
                            robotController.GetJoints()[i].MoveAngleWithSpeed(true, angles[i], 1); // sync start position
                        }
                    });
                });
            }
            else
            {
                syncButton.GetComponent<Image>().color = Color.gray;
            }
        }

        if (!syncOn && !remoteRobot.IsConnected())
        {
            remoteRobot.ConnectToRemoteRobot(ipInputField.text, portInputField.text, (succeeded, message) => {
                Debug.Log("Connection succeeded = " + succeeded);
                statusText.text = message;
                statusText.color = succeeded ? Color.green : Color.red;
                syncButton.GetComponent<Image>().color = succeeded ? Color.green : Color.white;
            });
        }
    }

    private void Connected(object sender, System.EventArgs e)
    {
        SharingStage.Instance.SharingManagerConnected -= Connected;

        // Remove the warning text
        this.usersPanel.Find("WarningText").gameObject.SetActive(false);

        localUser = SharingStage.Instance.Manager.GetLocalUser();
        usersTracker = SharingStage.Instance.SessionUsersTracker;
        string users = string.Empty;

        for (int i = 0; i < usersTracker.CurrentUsers.Count; i++)
        {
            users += "\n" + usersTracker.CurrentUsers[i].GetName();
            //AddUser(usersTracker.CurrentUsers[i].GetName());
        }

        Debug.LogFormat("User Notifications: {0} users in room.{1}", usersTracker.CurrentUsers.Count, users);

        

        usersTracker.UserJoined += NotifyUserJoined;
        usersTracker.UserLeft += NotifyUserLeft;
    }

    private void NotifyUserJoined(User user)
    {
        if (user.IsValid() && localUser.GetID() != user.GetID())
        {
            Debug.LogFormat("User Notifications: User {0} has joined the room.", user.GetName() + "(" + localUser.GetID() + ")");
            AddUser(user);
        }
    }

    private void NotifyUserLeft(User user)
    {
        if (user.IsValid() && localUser.GetID() != user.GetID())
        {
            Debug.LogFormat("User Notifications: User {0} has left the room.", user.GetName() + "(" + localUser.GetID() + ")");
            RemoveUser(user);
        }
    }

    private void AddUser(User user)
    {
        var userIcon = Instantiate(this.userTemplate, this.usersPanelContainer);
        userIcon.Find("Text").GetComponent<Text>().text = user.GetName();
        userIcon.gameObject.SetActive(true);
        userDict.Add(user.GetID(), userIcon.gameObject);
    }

    private void RemoveUser(User user)
    {
        GameObject gameObject;
        if (userDict.TryGetValue(user.GetID(), out gameObject))
        {
            Destroy(gameObject);
            userDict.Remove(user.GetID());
        }
        else
        {
            Debug.LogError("Failed to remove user with ID=" + user.GetID());
        }
    }

    private void OnDestroy()
    {
        if (usersTracker != null)
        {
            usersTracker.UserJoined -= NotifyUserJoined;
            usersTracker.UserLeft -= NotifyUserLeft;
        }
        usersTracker = null;
    }
}
