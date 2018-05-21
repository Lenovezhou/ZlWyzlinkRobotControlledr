using HoloToolkit.Sharing;
using HoloToolkit.Sharing.SyncModel;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickibleItem : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private MeshRenderer meshRenderer;
    public Material highLightMat;
    private Material originalMat;
    private SyncTestSphere model;
    private Color[] colors = new Color[] { Color.red, Color.blue, Color.cyan, Color.yellow, Color.magenta, Color.gray, Color.green };
    private int currentIndex = 0;

    public void OnPointerClick(PointerEventData eventData)
    {
        currentIndex++;
        currentIndex = currentIndex % colors.Length;
        originalMat.color = colors[currentIndex];
        if (model != null)
        {
            Debug.Log("Setting new color");
            model.color.Value = colors[currentIndex];
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(this.gameObject);
        originalMat.SetFloat("_Outline", 0.03f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
        originalMat.SetFloat("_Outline", 0f);
    }

    // Use this for initialization
    void Start () {
        this.meshRenderer = GetComponent<MeshRenderer>();
        this.originalMat = this.meshRenderer.material;

        var accessor = GetComponent<DefaultSyncModelAccessor>();
        Debug.Assert(accessor != null);
        this.model = (SyncTestSphere)(accessor.SyncModel);
        if (this.model != null)
        {
            this.model.color.ObjectChanged += (SyncObject obj) =>
            {
                originalMat.color = ((SyncColor)obj).Value;
            };
        }
	}

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Begin dragging");
    }

    public void OnDrag(PointerEventData eventData)
    {
        var distance = Vector3.Dot(this.transform.position - Camera.main.transform.position, Camera.main.transform.forward);
        var point = Camera.main.ScreenToWorldPoint(new Vector3(eventData.position.x, eventData.position.y, distance));
        this.transform.position = point;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("End dragging");
    }
}
