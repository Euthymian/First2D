using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

[AddComponentMenu("TuanThanh/PlayerManager")]
public class PlayerManager : MonoBehaviour
{
    [SerializeField] LayerMask pickableItem;
    [SerializeField] Transform pickUpPoint;
    int maxNumOfItems = 2;
    int currentNumOfItems = 0;
    int startIndexOfPickUpItem = 4;

    void Start()
    {

    }

    bool PickUpIndicator() => Input.GetKeyDown(KeyCode.E);
    bool DropIndicator() => Input.GetKeyDown(KeyCode.R);

    // Update is called once per frame
    void Update()
    {
        if (PickUpIndicator() && currentNumOfItems < maxNumOfItems)
            PickUp();
        else if (DropIndicator() && currentNumOfItems > 0)
            Drop(transform.GetChild(startIndexOfPickUpItem).gameObject);
    }

    void PickUp()
    {
        if (Physics2D.OverlapCircle(pickUpPoint.transform.position, 0.1f, pickableItem))
        {
            GameObject tmp = Physics2D.OverlapCircle(pickUpPoint.transform.position, 0.1f, pickableItem).gameObject;
            if (tmp.TryGetComponent<Rigidbody2D>(out var rigidbody2D)) Destroy(rigidbody2D);
            currentNumOfItems++;

            tmp.GetComponent<BoxCollider2D>().isTrigger = true;
            tmp.transform.parent = transform;

            if (maxNumOfItems == 1)
                tmp.transform.localPosition = new Vector3(0, 0.5f, 0);
            if (maxNumOfItems == 2)
                tmp.transform.localPosition = new Vector3(-0.1f + (currentNumOfItems - 1) * 0.25f, 0.5f, 0);
            else if (maxNumOfItems == 3)
                tmp.transform.localPosition = new Vector3(-0.25f + (currentNumOfItems - 1) * 0.25f, 0.5f, 0);

            tmp.transform.rotation = Quaternion.identity;
            tmp.transform.localScale = new Vector3(0.3f, 0.3f);
            if (tmp.transform.tag == "OnAirPlatform")
            {
                tmp.transform.localPosition = new Vector3(tmp.transform.localPosition.x, 0.6f, 0);
                tmp.transform.localScale = new Vector3(0.2f, 0.2f);
            }
        }
    }

    void Drop(GameObject item)
    {
        currentNumOfItems--;
        item.transform.parent = item.GetComponent<FallingItemManager>().intitialParent;
        item.transform.localScale = item.GetComponent<FallingItemManager>().initialLocalScale;
        item.GetComponent<BoxCollider2D>().isTrigger = false;
        item.transform.position = new Vector3(transform.position.x + PlayerController.IsFacingRight() * (0.01f + item.GetComponent<BoxCollider2D>().bounds.extents.x + GetComponent<BoxCollider2D>().bounds.extents.x), transform.position.y, 0);
        item.AddComponent<Rigidbody2D>();
        item.GetComponent<Rigidbody2D>().collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        if (item.transform.tag == "OnAirPlatform")
        {
            Rigidbody2D rb = item.GetComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Static;
            item.layer = 6; // 6 -> Ground
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Crystal")
        {
            GameManager.Instance.AddCrystal(1);
            Destroy(collision.gameObject);
        }
    }
}
