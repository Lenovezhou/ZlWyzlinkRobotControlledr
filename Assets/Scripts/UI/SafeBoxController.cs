using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class SafeBoxController : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        this.animator = GetComponent<Animator>();
    }

    public void DoAnimation()
    {
        this.animator.SetTrigger("Flash");
    }
}
