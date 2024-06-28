using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnHoldButton : TButton
{
    Animator anim;
    private void Start()
    {
        anim = GetComponentInChildren<Animator>();
    }
    private void Update()
    {
        lastStatus = currentStatus;
        currentStatus = isOnTriggerPlayer;

        if (!lastStatus && currentStatus)
        {
            anim.SetBool("Activate", true);
        }

        if (lastStatus && !currentStatus)
        {
            anim.SetBool("Activate", false);
        }
    }
}
