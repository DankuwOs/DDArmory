using System;
using System.Linq;
using Harmony;
using UnityEngine;

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

        Debug.Log($"[HPEquipSwingWing]: OnEquip");
        
        Initialize();
    }

    public override void OnConfigAttach(LoadoutConfigurator configurator)
    {
        base.OnConfigAttach(configurator);

        Initialize(configurator);
    }

    public override void OnConfigDetach(LoadoutConfigurator configurator)
    {
        base.OnConfigDetach(configurator);

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

        if (vesselVehiclePart)
            foreach (var vehiclePart in GetComponentsInChildren<VehiclePart>())
            {
                if (!vehiclePart.parent)
                    vehiclePart.parent = vesselVehiclePart;
            }
        
        Debug.Log($"[HPEquipSwingWing]: AeroController? = {aeroController}");
        Debug.Log($"[HPEquipSwingWing]: AC Length = {aeroController.controlSurfaces.Length}");
        if (aeroController)
        {
            for (int i = 0; i < controlSurfaces.Length; i++)
            {
                Debug.Log($"[HPEquipSwingWing]: Doing control surface {i}");
                var controlSurfaceTransform = controlSurfaces[i];

                if (!controlSurfaceTransform.transform)
                    continue;

                if (i < aeroController.controlSurfaces.Length)
                {
                    aeroController.controlSurfaces[i] = controlSurfaceTransform;
                    
                    aeroController.controlSurfaces[i].Init();
                    continue;
                }

                Debug.Log($"[HPEquipSwingWing]: Adding new surface {i}");

                var controllerList = aeroController.controlSurfaces.ToList(); // Simply adding it doesn't work :~(
                controllerList.Add(controlSurfaceTransform);
                aeroController.controlSurfaces = controllerList.ToArray();
                
                aeroController.controlSurfaces[i].Init();
            }
        }

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

        foreach (var i in detachHpIdx)
        {
            if (configurator)
            {
                configurator.DetachImmediate(i);
            }
            if (_weaponManager.GetEquip(i))
                _weaponManager.JettisonEq(i);
        }
        
        
        foreach (var disableObject in disableObjects)
        {
            ToggleObject(disableObject);
        }

        foreach (var componentsInChild in GetComponentsInChildren<Wing>())
        {
            componentsInChild.rb = _weaponManager.vesselRB;
        }

        controller.SetPivotImmediate(defaultSweep);

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

        var sweepLever = GetComponentInChildren<VRThrottle>();
        
        if (sweepLever)
            sweepLever.RemoteSetThrottle(1 - defaultSweep);
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