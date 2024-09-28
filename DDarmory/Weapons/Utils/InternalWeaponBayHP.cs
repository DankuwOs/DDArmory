using System;
using UnityEngine;

namespace DDArmory.Weapons.Utils;

public class InternalWeaponBayHP : InternalWeaponBay
{
    public override void Awake()
    {
        Debug.Log($"[IWBHP]: Replaced Awake");
    }

    public override void Start()
    {
        Debug.Log($"[IWBHP]: Replaced Start");
        if (!hideWhenClosed) return;
        if (rotationToggle)
        {
            rotationToggle.OnFinishRetract += Hide;
            rotationToggle.OnStartDeploy += Show;
        }
        Hide();
    }
}