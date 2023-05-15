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
        Debug.Log($"[DD GunTrim]: Set trim {enableTrim}");
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
        Debug.Log("[DD GunTrim]: 1");
        // Wait for all the weapons to be equipped
        yield return null;
        
        var wm = gun.actor.weaponManager;
        Debug.Log("[DD GunTrim]: 2");

        List<HPEquippable> equips = new List<HPEquippable>();
        for (int i = 0; i < wm.equipCount; i++)
        {
            equips.Add(wm.GetEquip(i));
        }

        foreach (var equip in equips)
        {
            Debug.Log("[DD GunTrim]: A");
            var gunTrim = equip.GetComponentInChildren<GunTrim>();
            Debug.Log("[DD GunTrim]: B");
            if (!gunTrim || gunTrim.cockpitObject.name != cockpitObj.name)
            {
                if (gunTrim)
                    Debug.Log($"[DD GunTrim]: CockpitObj = {gunTrim.cockpitObject.name} | Other = {cockpitObj.name}");
                continue;
            }
            Debug.Log("[DD GunTrim]: C");
            count++;
        }

        Debug.Log($"[DD GunTrim]: Count = {count}");
    }

    private void FixedUpdate()
    {
        if (!trimming || !_flightAssist)
            return;

        var infoTraverse = Traverse.Create(_flightAssist);
        var trimAmount = trimCurve.Evaluate(gun.actor.flightInfo.airspeed) * count;
        Debug.Log($"[DD GunTrim]: Amt = {trimAmount}");
        infoTraverse.Field("takeOffTrimAmt").SetValue(invert ? -trimAmount : trimAmount);
    }
}