using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using HoloToolkit.Unity.InputModule;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GizmoChildController : MonoBehaviour,
    IDragHandler, IBeginDragHandler,
    IPointerEnterHandler, IPointerExitHandler
{
    public enum AxisConstrain
    {
        None,
        X,
        Y,
        Z,
    };
    [SerializeField]
    private AxisConstrain axisConstrain;
    [SerializeField]
    private float draggingSpeed = 2.0f;

    private Material originalMat;
    private Vector3 startPosition;
    private TextMesh indexText;

    // Use this for initialization
    void Start()
    {
        this.originalMat = GetComponent<MeshRenderer>().material;
    }

    public void OnDrag(PointerEventData eventData)
    {
        var distance = Vector3.Dot(this.transform.position - Camera.main.transform.position, Camera.main.transform.forward);
        var point = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, distance));
        this.transform.parent.position = ApplyAxisConstrains(this.axisConstrain, this.transform.parent.position, point + startPosition);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(this.gameObject);
        originalMat.SetFloat("_Outline", 0.0005f);
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
        startPosition = this.transform.parent.position - point;
    }

    public void OnManipulationStarted(ManipulationEventData eventData)
    {
        Debug.Log("Manipulation started");
        this.startPosition = this.transform.parent.position;
    }
    
    public void OnManipulationUpdated(ManipulationEventData eventData)
    {
        Debug.Log("Manipulation updated: " + this.transform.position.ToString() + " " + startPosition.ToString() + "^" + eventData.CumulativeDelta.ToString() + "@" + this.GetHashCode());
        this.transform.parent.position = startPosition + ApplyAxisConstrainsRelative(this.axisConstrain, eventData.CumulativeDelta * draggingSpeed);
    }

    public void OnManipulationCompleted(ManipulationEventData eventData)
    {
        Debug.Log("Manipulation completed");
    }

    public void OnManipulationCanceled(ManipulationEventData eventData)
    {
        Debug.Log("Manipulation canceled");
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
}
