using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class FallingItemManager : MonoBehaviour
{
    [SerializeField] LayerMask ground;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (transform.tag == "OnAirPlatform" && Input.GetKeyDown(KeyCode.P))
        {
            gameObject.layer = 8;
            if (TryGetComponent<Rigidbody2D>(out var rb2D))
                rb2D.bodyType = RigidbodyType2D.Dynamic;
        }
        FallToGround();
    }

    void FallToGround()
    {
        if (TryGetComponent<Rigidbody2D>(out var rb2D))
        {
            if (rb2D.bodyType == RigidbodyType2D.Dynamic)
            {
                if (Physics2D.BoxCast(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size * 1.05f, 0, Vector2.zero, 0, ground))
                {
                    Destroy(rb2D);
                    GetComponent<BoxCollider2D>().isTrigger = true;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawCube(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size * 1.05f);
    }
}
