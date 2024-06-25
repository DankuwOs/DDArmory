using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

[RequireComponent(typeof(SwingWingController))]
public class HPEquipSwingWing : HPEquipWing
{
    [Header("Swing Wing")]

    [Range(0, 1)] public float defaultSweep = 0;

    public bool manualByDefault;

    [Tooltip("List of hardpoints, goes this (Modified HP, Parent of HP, Position, Rotation)")] 
    private List<Tuple<Transform, Transform, Vector3, Quaternion>> hpTransforms = new List<Tuple<Transform, Transform, Vector3, Quaternion>>();

    public override void OnEquip()
    {
        base.OnEquip();

        Initialize();
    }

    public override void OnConfigAttach(LoadoutConfigurator configurator)
    {
        base.OnConfigAttach(configurator);
        if (configurator.uiOnly)
            return;
        
        Initialize(configurator);
    }

    public override void OnConfigDetach(LoadoutConfigurator configurator)
    {
        if (configurator.uiOnly)
            return;

        foreach (var disableObject in disableObjects)
        {
            ToggleObject(disableObject, configurator.wm, true);
        }

        if (hpTransforms.Count > 0)
        {
            foreach (var hpTransform in hpTransforms)
            {
                if (hpTransform.Item1 && hpTransform.Item2)
                {
                    var tf = hpTransform.Item1;
                    tf.SetParent(hpTransform.Item2);
                    tf.localPosition = hpTransform.Item3;
                    tf.localRotation = hpTransform.Item4;
                }
            }
        }
        
        base.OnConfigDetach(configurator);
    }

    public override void Initialize(LoadoutConfigurator configurator = null)
    {
        base.Initialize(configurator);

        var wm = configurator ? configurator.wm : weaponManager;

        if (controller == null || controller.GetType() != typeof(SwingWingController))
            return;

        var swingWingController = controller as SwingWingController;

        var aeroController = wm.GetComponent<AeroController>();
        
        if (aeroController)
            swingWingController.aeroController = aeroController;

            // Setting manual | auto mode
        if (manualByDefault)
        {
            swingWingController.display.manualObj.SetVisibility(false);
            swingWingController.display.autoObj.SetVisibility(true);
            swingWingController.manual = true;
        }
        else
        {
            swingWingController.display.manualObj.SetVisibility(true);
            swingWingController.display.autoObj.SetVisibility(false);
            swingWingController.manual = false;
        }

        if (wm.battery)
            swingWingController.toggle.battery = wm.battery;

        
        
        // Set default sweep
        var sweepLever = GetComponentInChildren<VRThrottle>();
        
        if (sweepLever)
            sweepLever.RemoteSetThrottle(1 - defaultSweep);
        
        swingWingController.SetPivotImmediate(defaultSweep);
    }
}