using System;
using System.Linq;
using System.Timers;
using DDArmory.Weapons.Utils;
using UnityEngine;
using VTOLVR.Multiplayer;


public class SwingWingController : WingController
{
    [Header("Swing Wings")]
    public RotationToggle toggle;
    
    public AnimationCurve sweepMachCurve;
    
    public SweptWing[] sweptWings;

    public bool useBattery = true;
    
    [Header("Cockpit")]
    public SweepDisplay display;

    [Range(0.02f,0.2f)]
    public float pivotDiffToManual = 0.05f;

    [NonSerialized]
    public bool manual;

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
            // When in Auto mode and battery dead the sweep bar will skip, this fixes that in a bad way.
            if (useBattery && (!battery.connected || (battery.connected && battery.currentCharge < 0.05f)))
                goto mothersHouseOutsideOfIfStatementGetFucked;

            if (Math.Abs(pivot - _lastPivot) > pivotDiffToManual)
                ToggleMode();
            else
                return;
        }
        mothersHouseOutsideOfIfStatementGetFucked: // Yes, I know. Bad practice and unreadable, don't care didn't ask.

        toggle.SetNormalizedRotation(pivot);

        if (display)
            display.targetSweep.SetScale(pivot);
        
        _lastPivot = pivot;
    }
    

    public void ToggleMode()
    {
        if (useBattery && (!battery.connected || (battery.connected && battery.currentCharge < 0.05f))) 
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
}