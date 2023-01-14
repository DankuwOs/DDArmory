using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VTOLVR.Multiplayer;

[RequireComponent(typeof(SwingWingController))]
public class HPEquipWing : HPEquippable, IMassObject
{
    public float mass = 0.25f;

    [Header("Parts")]
    public AeroController.ControlSurfaceTransform[] controlSurfaces;
    
    [Tooltip("Transforms of hardpoints, leave empty if you dont want to change it.")]
    public Transform[] hardpoints;

    public int[] detachHpIdx;

    public string[] disableObjects;

    public WingController controller;

    private WeaponManager _weaponManager;

    [Tooltip("List of hardpoints, goes this (Modified HP, Parent of HP, Position, Rotation)")] 
    private List<Tuple<Transform, Transform, Vector3, Quaternion>> hpTransforms = new List<Tuple<Transform, Transform, Vector3, Quaternion>>();

    protected override void OnEquip()
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
            ToggleObject(disableObject, true);
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

    public virtual void Initialize(LoadoutConfigurator configurator = null)
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
            
            for (var i = 0; i < controlSurfaces.Length; i++)
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
        }
        
        

        // Moving hardpoints to my own

        hpTransforms = new List<Tuple<Transform, Transform, Vector3, Quaternion>>();
        
        for (var index = 0; index < hardpoints.Length; index++)
        {
            
            var hardpoint = hardpoints[index];

            if (_weaponManager.hardpointTransforms[index] && hardpoint)
            {
                var wmHp = _weaponManager.hardpointTransforms[index];

                hpTransforms.Add(new Tuple<Transform, Transform, Vector3, Quaternion>(wmHp, wmHp.parent,
                    wmHp.localPosition, wmHp.localRotation));
                
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

        var flightInfo = _weaponManager.actor.flightInfo;
        
        // Set up wing vapor
        foreach (var wingVaporParticles in GetComponentsInChildren<WingVaporParticles>())
        {
            wingVaporParticles.flightInfo = flightInfo;
        }
        
        
        // Set up nav / strobe / formation lights
        controller.battery = _weaponManager.battery;
        controller.SetupLights();
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

    public override int GetMaxCount()
    {
        return 1; // If max count > count then reloading will reattach the weapon. this means that we can repair the wings.
    }

    public override int GetCount()
    {
        return 0;
    }

    public float GetMass()
    {
        return mass;
    }
}