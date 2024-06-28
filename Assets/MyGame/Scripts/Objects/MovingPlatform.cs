using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    private List<Vector3> nodes = new List<Vector3>();
    private int nextNodeIndex = 0;
    private Rigidbody2D rb;
    [SerializeField] private float speed;
    float distanceForChangeDir = 0.1f;
    [SerializeField] private GameObject activateButton;
    private bool stopNow = false;

    // remove condition in FixedUpdate to disable effect of button
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Transform thisParent = transform.parent;
        for (int i = 1; i < thisParent.childCount; i++)
        {
            nodes.Add(thisParent.GetChild(i).transform.position);
        }
    }

    float distanceToNextNode => Vector2.Distance(transform.position, nodes[nextNodeIndex]);

    void Update()
    {
        UpdateActivationStatus();
        //MovePlatform();
        UpdateNextNode();
    }

    void UpdateActivationStatus()
    {
        stopNow = activateButton.GetComponent<OnHoldButton>().Status;
    }

    void MovePlatform()
    {
        transform.position = Vector2.MoveTowards(transform.position, nodes[nextNodeIndex], Time.deltaTime * speed);
    }

    void UpdateNextNode()
    {
        if (distanceToNextNode <= distanceForChangeDir)
        {
            if (nextNodeIndex + 1 == nodes.Count) nextNodeIndex = 0;
            else nextNodeIndex++;
        }
    }

    private void FixedUpdate()
    {
        if (stopNow) rb.velocity = Vector2.zero;
        else MovePlatformByRb();
    }

    void MovePlatformByRb()
    {
        Vector2 dir = (nodes[nextNodeIndex] - transform.position).normalized;
        rb.velocity = dir * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.GetComponent<Rigidbody2D>().velocity.y <= 0.5)
            {
                collision.GetComponent<PlayerController>().OnMovingPlatform = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.GetComponent<PlayerController>().OnMovingPlatform = false;
        }
    }
}
