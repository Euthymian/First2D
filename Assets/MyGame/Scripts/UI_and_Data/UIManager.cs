using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI crystal;
    // Start is called before the first frame update
    void Start()
    {
        crystal.text = GameManager.Instance.GetCrystal().ToString();
        EventGameManager.crystalEvent.AddListener(AddUICrystal);
    }

    private void AddUICrystal(int arg0)
    {
        crystal.text = arg0.ToString();
    }



    // Update is called once per frame
    //void Update()
    //{
    //    crystal.text = GameManager.Instance.GetCrystal().ToString();
    //}
}
