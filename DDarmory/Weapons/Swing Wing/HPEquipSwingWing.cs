using System;
using System.Linq;
using Harmony;
using UnityEngine;
using VTOLVR.Multiplayer;

[RequireComponent(typeof(SwingWingController))]
public class HPEquipSwingWing : HPEquippable, IMassObject
{
    public float mass = 0.25f;

    [Header("Swing Wing")]

    [Range(0, 1)] public float defaultSweep = 0;

    public SwingWingController controller;

    public bool manualByDefault;
    
    [Header("Parts")]
    public AeroController.ControlSurfaceTransform[] controlSurfaces;
    
    [Tooltip("Transforms of hardpoints, leave empty if you dont want to change it.")]
    public Transform[] hardpoints;

    public int[] detachHpIdx;

    public string[] disableObjects;

    private WeaponManager _weaponManager;

    protected override void OnEquip()
    {
        base.OnEquip();

        Initialize();
    }

    public override void OnConfigAttach(LoadoutConfigurator configurator)
    {
        base.OnConfigAttach(configurator);
        
        if (!VTOLMPUtils.IsMultiplayer())
            Initialize(configurator);
    }

    public override void OnConfigDetach(LoadoutConfigurator configurator)
    {
        base.OnConfigDetach(configurator);

        if (!VTOLMPUtils.IsMultiplayer())
            Initialize(configurator);
        
        foreach (var disableObject in disableObjects)
        {
            ToggleObject(disableObject, true);
        }
    }

    public void Initialize(LoadoutConfigurator configurator = null)
    {
        _weaponManager = weaponManager;
        if (!_weaponManager && configurator != null)
            _weaponManager = configurator.wm;
        
        if (!_weaponManager)
            return;

        var vesselVehiclePart = _weaponManager.GetComponent<VehiclePart>();
        var aeroController = _weaponManager.GetComponent<AeroController>();

        // Old objects are still present in lists and cause nullrefs, this fixes that while also adding our own objects.
        if (VTOLMPUtils.IsMultiplayer())
        {
            var playerVehicleNetSync = _weaponManager.GetComponent<PlayerVehicleNetSync>();
            if (playerVehicleNetSync)
            {
                var list = _weaponManager.GetComponentsInChildren<VehiclePart>();

                playerVehicleNetSync.vehicleParts = list;
            }
            
            var damageSync = _weaponManager.GetComponent<DamageSync>();
            if (damageSync)
            {
                var list = _weaponManager.GetComponentsInChildren<Health>();

                damageSync.healths = list;
            }
        }

        // Assign parent to the vehicle parts for reasons
        if (vesselVehiclePart)
            foreach (var vehiclePart in GetComponentsInChildren<VehiclePart>())
            {
                if (!vehiclePart.parent)
                    vehiclePart.parent = vesselVehiclePart;
            }
        
        
        if (aeroController)
        {
            // Adding my own control surfaces to the existing ones.
            
            for (int i = 0; i < controlSurfaces.Length; i++)
            {
                var controlSurfaceTransform = controlSurfaces[i];

                if (!controlSurfaceTransform.transform)
                    continue;

                if (i < aeroController.controlSurfaces.Length)
                {
                    aeroController.controlSurfaces[i] = controlSurfaceTransform;
                    
                    aeroController.controlSurfaces[i].Init(); // Need to init every time you change a control surface
                    continue;
                }
                
                // Add extra surfaces
                
                var controllerList = aeroController.controlSurfaces.ToList(); // Simply adding it doesn't work :~(
                controllerList.Add(controlSurfaceTransform);
                aeroController.controlSurfaces = controllerList.ToArray();
                
                aeroController.controlSurfaces[i].Init();
            }

            
            controller.aeroController = aeroController;
        }

        // Moving hardpoints to my own
        for (var index = 0; index < hardpoints.Length; index++)
        {
            var hardpoint = hardpoints[index];
            if (_weaponManager.hardpointTransforms[index] && hardpoint)
            {
                var wmHp = _weaponManager.hardpointTransforms[index];
                wmHp.SetParent(hardpoint);
                wmHp.localPosition = Vector3.zero;
                wmHp.localRotation = Quaternion.identity;
            }
        }

        // Detaching hardpoints that no longer exist
        foreach (var i in detachHpIdx)
        {
            if (configurator)
            {
                configurator.DetachImmediate(i);
            }
            if (_weaponManager.GetEquip(i))
                _weaponManager.JettisonEq(i);
        }
        
        // Toggling existing wings off
        foreach (var disableObject in disableObjects)
        {
            ToggleObject(disableObject);
        }
        
        
        foreach (var wing in GetComponentsInChildren<Wing>(true))
        {
            wing.rb = _weaponManager.vesselRB;
            wing.enabled = true;
        }

        // Setting manual | auto mode
        if (manualByDefault)
        {
            controller.display.manualObj.SetVisibility(false);
            controller.display.autoObj.SetVisibility(true);
            controller.manual = true;
        }
        else
        {
            controller.display.manualObj.SetVisibility(true);
            controller.display.autoObj.SetVisibility(false);
            controller.manual = false;
        }

        var flightInfo = _weaponManager.actor.flightInfo;

        if (flightInfo)
            controller.flightInfo = flightInfo;
        
        
        // Set up wing vapor
        foreach (var wingVaporParticles in GetComponentsInChildren<WingVaporParticles>())
        {
            wingVaporParticles.flightInfo = flightInfo;
        }
        
        
        // Set up nav / strobe / formation lights
        controller.battery = _weaponManager.battery;
        controller.SetupLights();

        if (_weaponManager.battery)
            controller.toggle.battery = _weaponManager.battery;
        
        var sweepLever = GetComponentInChildren<VRThrottle>();
        
        if (sweepLever)
            sweepLever.RemoteSetThrottle(1 - defaultSweep);
        
        controller.SetPivotImmediate(defaultSweep);
    }

    public void ToggleObject(string path, bool enable = false)
    {
        var t = _weaponManager.transform;

        var subString = path.Split(new []{'/'}, StringSplitOptions.RemoveEmptyEntries);

        foreach (var s in subString)
        {
            var child = t.Find(s);
            if (child)
                t = child;
            else
                continue;
        }
        
        t.gameObject.SetActive(enable);
    }
    
    

    public float GetMass()
    {
        return mass;
    }
}