using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchButton : TButton
{
    private void Update()
    {
        lastStatus = currentStatus;
        if (isOnTriggerPlayer && Input.GetKeyDown(KeyCode.B)) currentStatus = !currentStatus;

        if (!lastStatus && currentStatus)
        {
            // Activation anim
        }

        if (lastStatus && !currentStatus)
        {
            // Deactivation anim
        }
    }
}
