using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerTeleport : MonoBehaviour
{
    bool ableToTeleport = true;
    bool inTeleArea = false;
    float teleCooldown = 3;
    float teleCurrentTime = 0;
    bool teleNow = false;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (ableToTeleport && inTeleArea && Time.time - teleCurrentTime >= teleCooldown && Input.GetKeyDown(KeyCode.T))
        {
            teleCurrentTime = Time.time;
            teleNow = true;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (teleNow && collision.tag == "Teleporter")
        {
            teleNow = false;
            Teleporter teleporter = collision.GetComponent<Teleporter>();
            transform.position = new Vector3(teleporter.GetTarget().position.x, teleporter.GetTarget().position.y + teleporter.GetTarget().GetComponentInChildren<SpriteRenderer>().bounds.extents.y, transform.position.z);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Teleporter")
        {
            inTeleArea = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Teleporter")
        {
            inTeleArea = false;
        }
    }
}
