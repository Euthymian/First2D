using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class DataManager
{
    public static int DataCrystal
    {
        get => PlayerPrefs.GetInt(ConstantKeys.KeyCrystalID, 0);
        set => PlayerPrefs.SetInt(ConstantKeys.KeyCrystalID, value);
    }
}
