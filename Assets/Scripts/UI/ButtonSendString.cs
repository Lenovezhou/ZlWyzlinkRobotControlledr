using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonSendString : MonoBehaviour,IPointerEnterHandler
{
    [SerializeField]
    NumpadController nc;

    public void OnPointerEnter(PointerEventData eventData)
    {
        PlayEffect("Focus");
    }

    public void SendString()
    {
        nc.AddString(GetComponentInChildren<UnityEngine.UI.Text>().text);
        PlayEffect("buttonInterface");
    }

    void PlayEffect(string path)
    {
        Sound.Instance.PlayerEffect(path);
    }

}
