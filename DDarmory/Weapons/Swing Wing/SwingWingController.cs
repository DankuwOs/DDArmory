using System;
using System.Linq;
using System.Timers;
using DDArmory.Weapons.Utils;
using UnityEngine;
using VTOLVR.Multiplayer;


public class SwingWingController : MonoBehaviour
{
    [Header("Wings")]
    public RotationToggle toggle;
    
    public AnimationCurve sweepMachCurve;
    
    public SweptWing[] sweptWings;

    public bool useBattery = true;
    
    [Header("Cockpit")]
    public SweepDisplay display;

    [Range(0.02f,0.2f)]
    public float pivotDiffToManual = 0.05f;

    [Tooltip("Path to a VRLever separated by a '/'")]
    public string navSwitchPath;

    [Header("Ext. Lights")] 
    public StrobeLightController.StrobeLight[] strobeLights;

    public ObjectPowerUnit[] navLightPowerUnits;

    public FormationGlowController formationGlowController;

    [NonSerialized]
    public bool manual;

    [NonSerialized] 
    public FlightInfo flightInfo = null;

    [NonSerialized] 
    public Battery battery;

    [NonSerialized] 
    public AeroController aeroController;


    private float currentSweep;

    private float _lastPivot;

    private void Update()
    {
        currentSweep = toggle.transforms[0].currentT;
        
        display.currentSweep.SetScale(currentSweep);
        
        if (!manual)
            AutoSweep();

        CheckSurfaces();
    }

    public void SetPivotImmediate(float pivot)
    {
        toggle.SetNormalizedRotationImmediate(pivot);
        
        if (display)
            display.targetSweep.SetScale(pivot);
    }

    public void SetPivot(float pivot)
    {
        if (!manual)
        {
            if (Mathf.Abs(pivot - _lastPivot) > pivotDiffToManual)
                ToggleMode();
            else
                return;
        }

        toggle.SetNormalizedRotation(pivot);

        if (display)
            display.targetSweep.SetScale(pivot);
        _lastPivot = pivot;
    }

    public void ToggleMode()
    {
        if (useBattery && battery.connected && battery.currentCharge < 0.05f) 
            return;
        
        if (manual)
        {
            manual = false;
            display.manualObj.SetVisibility(true);
            display.autoObj.SetVisibility(false);
            return;
        }
        manual = true;
        display.manualObj.SetVisibility(false);
        display.autoObj.SetVisibility(true);
    }

    public void AutoSweep()
    {
        if (flightInfo == null || (useBattery && (!battery.connected ||  (battery.connected && battery.currentCharge < 0.05f)))) 
            return;

        var autoSweep = Mathf.Lerp(0, 1, sweepMachCurve.Evaluate(MeasurementManager.SpeedToMach(flightInfo.surfaceSpeed, flightInfo.altitudeASL)));
        display.targetSweep.SetScale(autoSweep);
        toggle.SetNormalizedRotation(autoSweep);
    }

    
    public void CheckSurfaces()
    {
        if (!aeroController)
            return;

        foreach (var sweptWing in sweptWings)
        {
            sweptWing.SetWingArea(currentSweep);
            
            if (!sweptWing.surface)
                continue;

            foreach (var controlSurfaceTransform in aeroController.controlSurfaces)
            {
                if (controlSurfaceTransform.transform == null || controlSurfaceTransform.transform != sweptWing.surface)
                    continue;
                
                
                switch (sweptWing.type)
                {
                    case SweptWing.SurfaceType.Pitch:
                        
                        controlSurfaceTransform.pitchFactor =
                            sweptWing.maxSweep >= currentSweep ? sweptWing.defaultValue : 0f;
                        
                        break;
                    case SweptWing.SurfaceType.Yaw:
                        
                        controlSurfaceTransform.yawFactor =
                            sweptWing.maxSweep >= currentSweep ? sweptWing.defaultValue : 0f;
                        
                        break;
                    case SweptWing.SurfaceType.Roll:
                        
                        controlSurfaceTransform.rollFactor =
                            sweptWing.maxSweep >= currentSweep ? sweptWing.defaultValue : 0f;
                        
                        break;
                    case SweptWing.SurfaceType.Flaps:
                        
                        controlSurfaceTransform.flapsFactor =
                            sweptWing.maxSweep >= currentSweep ? sweptWing.defaultValue : 0f;
                        
                        break;
                    case SweptWing.SurfaceType.Brakes:
                        
                        controlSurfaceTransform.brakeFactor =
                            sweptWing.maxSweep >= currentSweep ? sweptWing.defaultValue : 0f;
                        
                        break;
                    case SweptWing.SurfaceType.AoA:
                        
                        controlSurfaceTransform.AoAFactor =
                            sweptWing.maxSweep >= currentSweep ? sweptWing.defaultValue : 0f;
                        
                        break;
                }
            }
        }
    }

    public void SetupLights()
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
                Debug.Log($"[Swing Wing Controller]: Found nav light controller, count = {navLightController.powerUnits.Length}");
                var powerUnits = navLightController.powerUnits.ToList(); // Maybe need?
                powerUnits.AddRange(navLightPowerUnits);
                navLightController.powerUnits = powerUnits.ToArray();
                Debug.Log($"[Swing Wing Controller]: Found nav light controller, count after = {navLightController.powerUnits.Length}");
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