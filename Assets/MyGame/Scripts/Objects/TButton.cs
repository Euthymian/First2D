using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TButton : MonoBehaviour
{
    [SerializeField] protected Rigidbody2D player;
    protected bool currentStatus;
    public bool Status
    {
        get => currentStatus; 
    }
    protected bool lastStatus;
    protected bool isOnTriggerPlayer;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player") isOnTriggerPlayer = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Player") isOnTriggerPlayer = false;
    }
}

public class OnHoldButton : TButton
{
    private void Update()
    {
        lastStatus = currentStatus;
        if (isOnTriggerPlayer) currentStatus = true;

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