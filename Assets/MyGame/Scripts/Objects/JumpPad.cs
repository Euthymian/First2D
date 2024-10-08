using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPad : MonoBehaviour
{
    [SerializeField] float jumpPadSpeed;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            PlayerController pc = collision.GetComponent<PlayerController>();
            pc.Fell = false;
            Rigidbody2D rb = pc.GetComponent<Rigidbody2D>();
            rb.velocity = new Vector2(rb.velocity.x, jumpPadSpeed);
            // Reset multiple jump
            pc.avaiableJumps = 1;
        }
    }
}
