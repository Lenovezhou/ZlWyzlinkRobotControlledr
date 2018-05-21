using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumpadController : MonoBehaviour
{
    [SerializeField]
    UnityEngine.UI.InputField ipAddressInputField;

    private AudioSource audioSource;
    public AudioClip buttonClickAudio;

    private void Start()
    {
        this.audioSource = this.transform.parent.GetComponent<AudioSource>();
    }

    public void AddString(string s)
    {
        if (ipAddressInputField.text.Length < 20)
        {
            ipAddressInputField.text += s;
        }
    }

    public void Backspace()
    {
        if(ipAddressInputField.text.Length > 0)
        {
            ipAddressInputField.text = ipAddressInputField.text.Substring(0, ipAddressInputField.text.Length - 1);
        }
    }

    public void KeyClick(int keyInputValue)
    {
        if (keyInputValue == -1)
        {
            AddString(".");
        }
        else if (keyInputValue == -2)
        {
            Backspace();
        }
        else
        {
            AddString(keyInputValue.ToString());
        }
        if (this.audioSource != null)
        {
            this.audioSource.PlayOneShot(buttonClickAudio);
        }
    }
}
