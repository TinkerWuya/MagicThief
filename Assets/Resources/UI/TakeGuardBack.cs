﻿using UnityEngine;
using System.Collections;

public class TakeGuardBack : MonoBehaviour
{
    public Guard guard;
    void Awake()
    {
        UnityEngine.UI.Button btn = GetComponent<UnityEngine.UI.Button>();
        btn.onClick.AddListener(() => btnClicked());
    }

    public void btnClicked()
    {
        Globals.selectGuardUI.ShowBtns();
        Globals.DestroyGuard(guard);        
    }
}