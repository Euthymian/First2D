using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("TuanThanh/PlayerManager")]
public class PlayerManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
