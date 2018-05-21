using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayInteractiveSound : MonoBehaviour,IPointerEnterHandler,IPointerDownHandler {


    public void OnPointerDown(PointerEventData eventData)
    {
        bool interactive = GetComponent<UnityEngine.UI.Selectable>().interactable;
        if (interactive)
        {
            PlayOneShout("buttonInterface");
        }
        else {
            PlayOneShout("Error");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bool interactive = GetComponent<UnityEngine.UI.Selectable>();
        if (interactive)
        {
            PlayEffect("Focus");
        }
        else
        {

        }
    }



    void PlayEffect(string clipname)
    {
        Sound.Instance.PlayerEffect(clipname);
    }

    void PlayOneShout(string clipname)
    {
        Sound.Instance.PlayerOneShout(clipname, transform.position);
    }

}
