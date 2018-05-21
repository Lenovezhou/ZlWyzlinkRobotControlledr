using UnityEngine;
using UnityEngine.UI;

public class GizmoMenuController : MonoBehaviour
{
    // Assume the order: 
    //   Claw Open
    //   Claw Close
    //   Rotate Left
    //   Rotate Right
    public Toggle[] toggles;
    private GizmoController.GizmoPostAction currentState;
    private GizmoController gizmoController;
    private Animator menuAnimator;

    private void Start()
    {
        this.gizmoController = this.transform.parent.GetComponent<GizmoController>();
        Debug.Assert(this.gizmoController != null);
        this.menuAnimator = GetComponent<Animator>();
        Debug.Assert(this.menuAnimator != null);
        Debug.Assert(toggles.Length == 4);
        toggles[0].onValueChanged.AddListener(OnClawOpenChange);
        toggles[1].onValueChanged.AddListener(OnClawCloseChange);
        toggles[2].onValueChanged.AddListener(OnRotateLeftChange);
        toggles[3].onValueChanged.AddListener(OnRotateRightChange);
    }

    public void ToggleMenu(GizmoController.GizmoPostAction postAction)
    {
        Debug.Log("Open Menu");
        var isOpen = this.menuAnimator.GetBool("Open");
        if (!isOpen)
        {
            this.UpdateState(postAction, true);
        }
        this.menuAnimator.transform.LookAt(2 * this.transform.position - Camera.main.transform.position);
        this.menuAnimator.SetBool("Open", !isOpen);
    }

    public void OnClawOpenChange(bool state)
    {
        UpdateState(GizmoController.GizmoPostAction.ClawOpen, state);
    }

    public void OnClawCloseChange(bool state)
    {
        UpdateState(GizmoController.GizmoPostAction.ClawClose, state);
    }
    public void OnRotateLeftChange(bool state)
    {
        UpdateState(GizmoController.GizmoPostAction.RotateLeft, state);
    }
    public void OnRotateRightChange(bool state)
    {
        UpdateState(GizmoController.GizmoPostAction.RotateRight, state);
    }

    void UpdateState(GizmoController.GizmoPostAction action, bool state)
    {
        if (currentState == action)
        {
            if (!state)
            {
                currentState = GizmoController.GizmoPostAction.None;
            }
        }
        else
        {
            if (currentState != GizmoController.GizmoPostAction.None)
            {
                GetCurrentToggle(currentState).isOn = false;
            }
            currentState = action;
        }
        this.gizmoController.SetPostAction(currentState);
    }

    private Toggle GetCurrentToggle(GizmoController.GizmoPostAction actionState)
    {
        Debug.Assert(actionState != GizmoController.GizmoPostAction.None);
        return toggles[(int)currentState - 1];
    }

    public void RemoveGizmo()
    {
        ToggleMenu(this.currentState);
        Run.After(0.25f, () => { this.gizmoController.RemoveGizmo(); });
    }
}
