using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TButton : MonoBehaviour
{
    protected bool currentStatus;
    public bool Status { get => currentStatus; }
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