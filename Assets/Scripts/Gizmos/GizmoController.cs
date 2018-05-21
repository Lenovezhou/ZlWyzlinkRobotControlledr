using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using HoloToolkit.Unity.InputModule;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;

public class GizmoController : NetworkBehaviour,
    IDragHandler, IBeginDragHandler,
    IPointerClickHandler,
    IPointerEnterHandler, IPointerExitHandler,
    IManipulationHandler
{
    public enum AxisConstrain
    {
        None,
        X,
        Y,
        Z,
    };
    public enum GizmoPostAction
    {
        None = 0,
        ClawOpen,
        ClawClose,
        RotateLeft,
        RotateRight,
    };

    [SerializeField]
    private AxisConstrain axisConstrain;
    [SerializeField]
    private float draggingSpeed = 2.0f;

    public int Index { get; private set; }

    #region Unet同步变量
    [SyncVar(hook = "UpdateOrderText")]
    public int syncIndex;

    [SyncVar(hook = "SyncActionSprite")]
    private GizmoPostAction postAction;
    /// <summary>
    /// The position relative to the shared world anchor.
    /// </summary>
    [SyncVar(hook = "OnPositionChange")]
    private Vector3 localPosition;
    [SyncVar(hook ="OnRotationChange")]
    private Quaternion localRotation;
    #endregion


    /*持续时间*/
    private float duration;

    /*机械臂移动到该点速度*/
    public float speed;


    private Material originalMat;
    private Vector3 startPosition;
    private TextMesh indexText;
    private SpriteRenderer postActionSprite;
    private GizmoMenuController menuController;

   

    private MeshRenderer[] selfrenderers;
    private Collider[] selfcollider;

    /*是否为舞蹈点*/
    [SyncVar]
    private bool isDancing = false;
    public bool IsDancing
    {
        get
        {
            return isDancing;
        }
        set
        {
            isDancing = value;
        }
    }

    //方便外界获取
    public GizmoPostAction GetPoatAction
    {
        get{ return postAction; }
        set { postAction = value; }
    }

    public float Duration
    {
        get
        {
            return duration;
        }

        set
        {
            duration = value;
        }
    }

    public Sprite rotateLeft;
    public Sprite rotateRight;
    public Sprite clawOpen;
    public Sprite clawClose;

    private Color originalColor;
    private Color dimmedColor = Color.gray;
    private bool isInSafeRange = false;

    // Use this for initialization
    void Start()
    {
        selfcollider = GetComponentsInChildren<Collider>();
        selfrenderers = GetComponentsInChildren<MeshRenderer>();
        HideAxisInHololens();
        this.originalMat = GetComponent<MeshRenderer>().material;
        CheckChild();
        Debug.Assert(indexText != null && postActionSprite != null);
        this.menuController = this.transform.Find("GizmoMenu").GetComponent<GizmoMenuController>();
        Debug.Assert(this.menuController != null);
        this.originalColor = GetComponent<MeshRenderer>().material.color;
        SetParent();


        //this.audioSource.PlayOneShot(creationAudio);
    }

    void SetParent()
    {
        Transform parent = WayPointManager.Instance.transform;
        transform.SetParent(parent);
        CheckChild();
        UpdateOrderText(transform.GetSiblingIndex() + 1);
    }


    void CheckChild()
    {
        if(!indexText || !postActionSprite)
            this.indexText = this.transform.Find("IndexText").GetComponent<TextMesh>();
            this.postActionSprite = this.transform.Find("PostActionSprite").GetComponent<SpriteRenderer>();
    }

    #region Unet hook 回调数据同步
    public void UpdateOrderText(int index)
    {
        CheckChild();
        this.syncIndex = index;
        indexText.text = index.ToString();
        var accessor = this.GetComponent<DefaultSyncModelAccessor>();
        if (accessor != null)
        {
            var model = (SyncGizmo)accessor.SyncModel;
            model.index.Value = index;
        }
    }
    void OnPositionChange(Vector3 update)
    {
        transform.position = update;
    }
    void OnRotationChange(Quaternion update)
    {
        transform.rotation = update;
    }
    [Command]
    public void CmdSendSyncTransform(Vector3 pos, Quaternion rot)
    {
        this.localPosition = pos;
        this.localRotation = rot;
    }
    #endregion
    public void OnDrag(PointerEventData eventData)
    {
        var distance = Vector3.Dot(this.transform.position - Camera.main.transform.position, Camera.main.transform.forward);
        var point = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, distance));
        //this.transform.position = point;
        //Vector3 localpos = transform.parent.TransformPoint(point);
        SpectatorView.PlayerController.Instance.SendSharedTransform(gameObject, point, transform.rotation);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(this.gameObject);
        originalMat.SetFloat("_Outline", 0.0005f);
        PlayEffect("Focus");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
        originalMat.SetFloat("_Outline", 0f);
    }

    private Vector3 ApplyAxisConstrains(AxisConstrain constrain, Vector3 target, Vector3 newPosition)
    {
        if (constrain == AxisConstrain.None)
        {
            return newPosition;
        }
        else if (constrain == AxisConstrain.X)
        {
            return new Vector3(newPosition.x, target.y, target.z);
        }
        else if (constrain == AxisConstrain.Y)
        {
            return new Vector3(target.x, newPosition.y, target.z);
        }
        else if (constrain == AxisConstrain.Z)
        {
            return new Vector3(target.x, target.y, newPosition.z);
        }
        throw new InvalidOperationException("Invalid constrains");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        var distance = Vector3.Dot(this.transform.position - Camera.main.transform.position, Camera.main.transform.forward);
        var point = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, distance));
        startPosition = this.transform.position - point;
    }

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
        Debug.Log("Manipulation started");
        this.startPosition = this.transform.position;
        InputManager.Instance.OverrideFocusedObject = this.gameObject;
    }
    
    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        Debug.Log("Manipulation updated: " + this.transform.position.ToString() + " " + startPosition.ToString() + "^" + eventData.CumulativeDelta.ToString() + "@" + this.GetHashCode());
        Vector3 point = startPosition + eventData.CumulativeDelta * draggingSpeed;
        //Vector3 localpos = transform.parent.TransformPoint(point);
        SpectatorView.PlayerController.Instance.SendSharedTransform(gameObject, point, transform.rotation);
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
        Debug.Log("Manipulation completed");
        InputManager.Instance.OverrideFocusedObject = null;
    }

    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
        Debug.Log("Manipulation canceled");
        InputManager.Instance.OverrideFocusedObject = null;
    }

    private Vector3 ApplyAxisConstrainsRelative(AxisConstrain constrain, Vector3 delta)
    {
        if (constrain == AxisConstrain.None)
        {
            return delta;
        }
        else if (constrain == AxisConstrain.X)
        {
            return new Vector3(delta.x, 0, 0);
        }
        else if (constrain == AxisConstrain.Y)
        {
            return new Vector3(0, delta.y, 0);
        }
        else if (constrain == AxisConstrain.Z)
        {
            return new Vector3(0, 0, delta.z);
        }
        throw new InvalidOperationException("Invalid constrains");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Right click to delete
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RemoveGizmo();
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // Toggle the menu
            Debug.Log("Toggle menu");
            //this.audioSource.PlayOneShot(menuOpenAudio);
            this.menuController.ToggleMenu(this.postAction);
        }
    }

    public void RemoveGizmo()
    {
        if (WayPointManager.Instance != null)
        {
            WayPointManager.Instance.RemoveWayPoint(this);
        }
    }

    public void MoveForwardInOrder()
    {
        if (WayPointManager.Instance != null)
        {
            WayPointManager.Instance.MoveForwardInOrder(this);
        }
    }

    public void MoveBackwardInOrder()
    {
        if (WayPointManager.Instance != null)
        {
            WayPointManager.Instance.MoveBackwardInOrder(this);
        }
    }

    public GizmoPostAction GetPostAction()
    {
        return this.postAction;
    }


    [Command]
    public void CmdSetPostAction(GizmoPostAction postAction,bool isdancing)
    {
        Debug.Log("SetPostAction");
        this.IsDancing = isdancing;
        this.postAction = postAction;
        if (postAction != GizmoPostAction.None)
            this.postActionSprite.sprite = GetPostActionSprite(postAction);
    }

    public void SetPostAction(GizmoPostAction postAction)
    {
        SpectatorView.PlayerController.Instance.CmdSyncSetPostAction(gameObject,postAction,isDancing);
    }


    public void SetPostAction(GizmoPostAction postAction, float speed, bool isdancing = false)
    {
        this.IsDancing = isdancing;
        this.speed = speed;
        this.postAction = postAction;
        if (postAction != GizmoPostAction.None)
            this.postActionSprite.sprite = GetPostActionSprite(postAction);

    }
    /// <summary>
    /// Unet hook 回调
    /// </summary>
    public void SyncActionSprite(GizmoPostAction postAction)
    {
        this.postAction = postAction;
        if (postAction != GizmoPostAction.None)
            this.postActionSprite.sprite = GetPostActionSprite(postAction);
    }


    private Sprite GetPostActionSprite(GizmoPostAction postAction)
    {
        switch (postAction)
        {
            case GizmoPostAction.ClawOpen:
                return this.clawOpen;
            case GizmoPostAction.ClawClose:
                return this.clawClose;
            case GizmoPostAction.RotateLeft:
                return this.rotateLeft;
            case GizmoPostAction.RotateRight:
                return this.rotateRight;
            default:
                return null;
        }
    }

    public bool IsInSafeRange()
    {
        return this.isInSafeRange;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("SafeBox"))
        {
            GetComponent<MeshRenderer>().material.color = originalColor;
            this.isInSafeRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SafeBox"))
        {
            GetComponent<MeshRenderer>().material.color = dimmedColor;
            this.isInSafeRange = false;
            var safebox = other.GetComponent<SafeBoxController>();
            if (safebox != null)
            {
                safebox.DoAnimation();
            }
        }
    }

    ///在Hololens端隐藏三个轴
    public void HideAxisInHololens()
    {
        bool ishololens = RobotsMain.Instance.IsHoloLens();
        transform.Find("x").gameObject.SetActive(!ishololens);
        transform.Find("y").gameObject.SetActive(!ishololens);
        transform.Find("z").gameObject.SetActive(!ishololens);
    }


    ///隐藏自身
    public void RefreshSelfRenderer(int index, bool alldone, Action<Vector3> callback)
    {
        int limit = index;
        int slefsyncindex = syncIndex;
        if ((slefsyncindex == limit + 1))
        {
            callback.Invoke(transform.position);
        }

        if (!isDancing)
            return;

        bool isactive = (slefsyncindex > limit && slefsyncindex <= (limit + 3));

        if (alldone)
        {
            isactive = true;
            for (int i = 0; i < selfrenderers.Length; i++)
            {
                selfrenderers[i].enabled = isactive;
            }
            for (int i = 0; i < selfcollider.Length; i++)
            {
                selfcollider[i].enabled = isactive;
            }
        }
        else
        {
            for (int i = 0; i < selfrenderers.Length; i++)
            {
                selfrenderers[i].enabled = isactive;
            }
            for (int i = 0; i < selfcollider.Length; i++)
            {
                selfcollider[i].enabled = isactive;
            }
        }
    }
    void PlayEffect(string clipname)
    {
        Sound.Instance.PlayerEffect(clipname);
    }
}
