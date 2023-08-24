using System;
using System.Collections;
using System.Collections.Generic;
using Harmony;
using UnityEngine;

public class GunTrim : MultiEquipCockpitElement
{
    [Header("Trim")]
    public AnimationCurve trimCurve;

    public Gun gun;

    public bool invert = true;

    private bool enableTrim;

    private bool trimming;

    private FlightAssist _flightAssist;

    private int count;

    public void SetTrim(Int32 trim)
    {
        enableTrim = trim != 0;
    }

    public override void Start()
    {
        OnCreateCockpitObject.AddListener(CreateObject);
        
        base.Start();

        if (!gun.actor)
        {
            Debug.Log($"[DD GunTrim]: No actor!");
            return;
        }

        _flightAssist = gun.actor.GetComponent<FlightAssist>();
        if (!_flightAssist)
        {
            Debug.Log($"[DD GunTrim]: No flight assist!");
            return;
        }

        gun.OnSetFire.AddListener(delegate(bool enable)
        {
            if (enableTrim || trimming)
            {
                trimming = enable;
            }
        });
    }

    private void CreateObject(GameObject cockpitObj)
    {
        var lever = cockpitObj.GetComponentInChildren<VRLever>();
        lever.OnSetState.AddListener(SetTrim);


        StartCoroutine(GetCount(cockpitObj)); // i think this is the best way to wait a frame
    }

    private IEnumerator GetCount(GameObject cockpitObj)
    {
        // Wait for all the weapons to be equipped
        yield return null;
        
        var wm = gun.actor.weaponManager;
        
        List<HPEquippable> equips = new List<HPEquippable>();
        for (int i = 0; i < wm.equipCount; i++)
        {
            equips.Add(wm.GetEquip(i));
        }

        foreach (var equip in equips)
        {
            var gunTrim = equip.GetComponentInChildren<GunTrim>();
            if (!gunTrim || gunTrim.cockpitObject.name != cockpitObj.name)
            {
                continue;
            }
            count++;
        }
    }

    private void FixedUpdate()
    {
        if (!trimming || !_flightAssist)
            return;

        var infoTraverse = Traverse.Create(_flightAssist);
        var trimAmount = trimCurve.Evaluate(gun.actor.flightInfo.airspeed) * count;
        
        infoTraverse.Field("takeOffTrimAmt").SetValue(invert ? -trimAmount : trimAmount);
    }
}