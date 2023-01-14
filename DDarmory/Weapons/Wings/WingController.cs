﻿using System;
using System.Linq;
using System.Timers;
using DDArmory.Weapons.Utils;
using UnityEngine;
using VTOLVR.Multiplayer;


public class WingController : MonoBehaviour
{
    [Header("Wings")]
    [Tooltip("Path to a VRLever separated by a '/'")]
    public string navSwitchPath;

    [Header("Ext. Lights")] 
    public StrobeLightController.StrobeLight[] strobeLights;

    public ObjectPowerUnit[] navLightPowerUnits;

    public FormationGlowController formationGlowController;

    [NonSerialized] 
    public FlightInfo flightInfo = null;

    [NonSerialized] 
    public Battery battery;


    public virtual void SetupLights(bool detach = false)
    {
        Transform startTf = null;
        if (flightInfo)
        {
            startTf = flightInfo.transform;
        }
        else
        {
            Debug.Log($"[Swing Wing Controller]: No start transform. :~(");
            return;
        }
        
        
        if (!battery)
        {
            Debug.Log($"[Swing Wing Controller]: Battery gone!");
            return;
        }

        var navSwitch = FindTransform.FindTranny(startTf, navSwitchPath)?.GetComponent<VRLever>();
        var strobeController = startTf.GetComponentInChildren<StrobeLightController>();

        if (VTOLMPUtils.IsMultiplayer())
        {
            var extLightSync = startTf.GetComponentInChildren<ExteriorLightSync>();
            
            if (extLightSync && formationGlowController)
                extLightSync.formationLights = formationGlowController;
        }

        if (navSwitch)
        {
            var navLightController = startTf.GetComponentInChildren<NavLightController>();
            
            // Assigning battery to fix null
            foreach (var navLightPowerUnit in navLightPowerUnits)
            {
                navLightPowerUnit.battery = battery;
            }

            foreach (var formationLight in formationGlowController.lights)
            {
                formationLight.battery = battery;
            }

            // Adding units to list
            if (navLightController)
            {
                var powerUnits = navLightController.powerUnits.ToList(); // Maybe need?
                powerUnits.AddRange(navLightPowerUnits);
                navLightController.powerUnits = powerUnits.ToArray();
            }

            // Toggling formation with switch.
            navSwitch.OnSetState.AddListener(delegate(int state)
            {
                formationGlowController.SetStatus(state);
            });
        }
        
        if (strobeController)
        {
            strobeController.lights.AddRange(strobeLights);
        }
    }
}