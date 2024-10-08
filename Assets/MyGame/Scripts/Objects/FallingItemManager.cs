using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

public class FallingItemManager : MonoBehaviour
{
    [HideInInspector] public Transform intitialParent;
    [HideInInspector] public Vector2 initialLocalScale;
    bool initialTriggerStatus;
    [SerializeField] LayerMask ground;
    // Start is called before the first frame update
    void Start()
    {
        intitialParent = transform.parent;
        initialTriggerStatus = GetComponent<BoxCollider2D>().isTrigger;
        initialLocalScale = GetComponent<Transform>().localScale;
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
                if (Physics2D.BoxCast(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size * 1.15f, 0, Vector2.zero, 0, ground))
                {
                    RaycastHit2D tmp = Physics2D.BoxCast(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size * 1.15f, 0, Vector2.zero, 0, ground);
                    //print(tmp.collider.gameObject);
                    if (tmp.collider.tag == "MovingPlatform") transform.parent = tmp.collider.gameObject.transform;
                    Destroy(rb2D);
                    GetComponent<BoxCollider2D>().isTrigger = initialTriggerStatus;
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(GetComponent<BoxCollider2D>().bounds.center, GetComponent<BoxCollider2D>().bounds.size * 1.1f);
    }
}
