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
    //private SwitchButton sButton;
    //private bool stopNow = false;
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
        //MovePlatform();
        UpdateNextNode();
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
        MovePlatformByRb();
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
                //collision.gameObject.transform.parent = transform;
                collision.GetComponent<PlayerController>().OnMovingPlatform = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            //collision.gameObject.transform.parent = null;
            collision.GetComponent<PlayerController>().OnMovingPlatform = false;
        }
    }
}
