using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[AddComponentMenu("TuanThanh/GameManager")]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance
    {
        get => instance;
    }
    private static GameManager instance;
    private void Awake()
    {
        if (instance != null)
        {
            DestroyImmediate(gameObject);
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

     int crystal;

    private void Start()
    {
        crystal = DataManager.DataCrystal;
        if (EventGameManager.crystalEvent == null) 
        {
            EventGameManager.crystalEvent = new GameEvent();
        }
    }

    public void AddCrystal(int num)
    {
        crystal += num;
        DataManager.DataCrystal = this.crystal;
        EventGameManager.crystalEvent?.Invoke(this.crystal);
    }

    public int GetCrystal()
    {
        return crystal;
    }
}
