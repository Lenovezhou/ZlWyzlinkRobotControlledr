using System;
using HoloToolkit.Unity.InputModule;
using UnityEngine;
using UnityEngine.EventSystems;

public class HoverSoundEffect : MonoBehaviour, IPointerEnterHandler, IPointerDownHandler, IInputClickHandler
{
    public AudioSource audioSource;
    public AudioClip clickSound;
    public AudioClip clickFailSound;
    public AudioClip hoverSound;

    public void OnInputClicked(InputClickedEventData eventData)
    {
        ClickSound();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        ClickSound();
    }

    private void ClickSound()
    {
        bool interactive = GetComponent<UnityEngine.UI.Selectable>().interactable;
        if (interactive)
        {
            this.audioSource.PlayOneShot(clickSound);
        }
        else
        {
            this.audioSource.PlayOneShot(clickFailSound);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        bool interactive = GetComponent<UnityEngine.UI.Selectable>();
        if (interactive)
        {
            this.audioSource.PlayOneShot(hoverSound);
        }
        else
        {

        }
    }
}
