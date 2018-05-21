using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemPanel : MonoBehaviour
{
    public InputField positionX;
    public InputField positionY;
    public InputField positionZ;
    public InputField rotationX;
    public InputField rotationY;
    public InputField rotationZ;
    public Button ApplyButton;
    public Button RemoveButton;
    public Text Caption;
    public delegate void ApplyPositionRotationHandler(Vector3 position, Quaternion rotation);
    public event ApplyPositionRotationHandler ApplyPositionRotation;

    private Transform item;

    //public void InitPositions(Vector3 position, Quaternion rotation, int enableMask)
    //{
    //    //
    //}

    private void Start()
    {
        ApplyButton.onClick.AddListener(OnApplyButton);
        RemoveButton.onClick.AddListener(OnRemoveButton);
    }

    public void InitItem(Transform item, int enableMask, string caption)
    {
        this.item = item;
        positionX.text = item.transform.localPosition.x.ToString();
        positionY.text = item.transform.localPosition.y.ToString();
        positionZ.text = item.transform.localPosition.z.ToString();
        rotationX.text = item.transform.localRotation.eulerAngles.x.ToString();
        rotationY.text = item.transform.localRotation.eulerAngles.y.ToString();
        rotationZ.text = item.transform.localRotation.eulerAngles.z.ToString();

        this.Caption.text = caption;

        // TODO: Apply enableMask
        if ((enableMask & 0x01) != 0)
        {
            positionX.interactable = true;
        }
        if ((enableMask & 0x02) != 0)
        {
            positionY.interactable = true;
        }
        if ((enableMask & 0x04) != 0)
        {
            positionZ.interactable = true;
        }
        if ((enableMask & 0x08) != 0)
        {
            rotationX.interactable = true;
        }
        if ((enableMask & 0x10) != 0)
        {
            rotationY.interactable = true;
        }
        if ((enableMask & 0x20) != 0)
        {
            rotationZ.interactable = true;
        }
        if ((enableMask & 0x40) != 0)
        {
            ApplyButton.interactable = true;
        }
        if ((enableMask & 0x80) != 0)
        {
            RemoveButton.interactable = true;
        }
    }

    public void OnApplyButton()
    {
        var position = new Vector3(Parse(positionX.text), Parse(positionY.text), Parse(positionZ.text));
        var rotation = Quaternion.Euler(Parse(rotationX.text), Parse(rotationY.text), Parse(rotationZ.text));

        item.transform.localPosition = position;
        item.transform.localRotation = rotation;
    }

    private float Parse(string text)
    {
        return string.IsNullOrEmpty(text) ? 0 : float.Parse(text);
    }

    public void OnRemoveButton()
    {
        ItemManager.Instance.RemoveItem(item);
        Destroy(this.gameObject);
    }
}
