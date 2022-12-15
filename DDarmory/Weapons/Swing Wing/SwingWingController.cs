using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using UnityEngine.Events;


public class SwingWingController : MonoBehaviour
{
    public RotationToggle toggle;
    
    public SweepDisplay display;

    [Range(0.02f,0.2f)]
    public float pivotDiffToManual = 0.05f;

    public AnimationCurve sweepMachCurve;

    [NonSerialized]
    public bool manual;

    [NonSerialized] 
    public FlightInfo flightInfo = null;

    private float _lastPivot;

    private void Update()
    {
        display.currentSweep.SetScale(toggle.transforms[0].currentT);
        
        if (!manual)
            AutoSweep();
        
        if (Input.GetKeyDown(KeyCode.B))
            Debug.Log($"Is sweep mode manual? {manual}");
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
        var autoSweep = Mathf.Lerp(0, 1, sweepMachCurve.Evaluate(MeasurementManager.SpeedToMach(flightInfo.surfaceSpeed, flightInfo.altitudeASL)));
        display.targetSweep.SetScale(autoSweep);
        toggle.SetNormalizedRotation(autoSweep);
    }
}