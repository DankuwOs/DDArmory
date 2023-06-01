using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using VTOLVR.Multiplayer;

[RequireComponent(typeof(WingController))]
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

    [Tooltip("List of hardpoints, goes this (Modified HP, Parent of HP, Position, Rotation)")] 
    private List<Tuple<Transform, Transform, Vector3, Quaternion>> hpTransforms = new();

    private VehiclePart[] _myParts;

    private VehiclePart[] _vanillaParts;

    protected override void OnEquip()
    {
        Initialize();
    }
    
    public override void OnUnequip()
    {
        if (!weaponManager)
            return;
        
        foreach (var disableObject in disableObjects)
        {
            ToggleObject(disableObject, weaponManager, true);
        }
        
        if (VTOLMPUtils.IsMultiplayer())
        {
            var playerVehicleNetSync = weaponManager.GetComponent<PlayerVehicleNetSync>();
            if (playerVehicleNetSync)
            {
                var parts = playerVehicleNetSync.vehicleParts.ToList();
                foreach (var vehiclePart in parts)
                {
                    if (_myParts.Contains(vehiclePart))
                        parts.Remove(vehiclePart);
                }
                playerVehicleNetSync.vehicleParts = parts.ToArray();
            }
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
    }

    public override void OnConfigAttach(LoadoutConfigurator configurator)
    {
        base.OnConfigAttach(configurator);
        if (configurator.uiOnly)
            return;

        foreach (var i in detachHpIdx)
        {
            if (configurator.hpNodes[i])
                ToggleNode(configurator.hpNodes[i].gameObject);
        }
        
        Initialize(configurator);
    }

    public override void OnConfigDetach(LoadoutConfigurator configurator)
    {
        if (configurator.uiOnly)
            return;
        
        foreach (var i in detachHpIdx)
        {
            if (configurator.hpNodes[i])
                ToggleNode(configurator.hpNodes[i].gameObject, false);
        }

        foreach (var disableObject in disableObjects)
        {
            ToggleObject(disableObject, configurator.wm, true);
        }
        
        if (VTOLMPUtils.IsMultiplayer())
        {
            var playerVehicleNetSync = configurator.wm.GetComponent<PlayerVehicleNetSync>();
            if (playerVehicleNetSync)
            {
                var parts = playerVehicleNetSync.vehicleParts.ToList();
                foreach (var vehiclePart in parts)
                {
                    if (_myParts.Contains(vehiclePart))
                        parts.Remove(vehiclePart);
                }
                playerVehicleNetSync.vehicleParts = parts.ToArray();
            }
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
    }

    public virtual void Initialize(LoadoutConfigurator configurator = null)
    {
        var wm = configurator ? configurator.wm : weaponManager;

        var vesselVehiclePart = wm.GetComponent<VehiclePart>();
        var aeroController = wm.GetComponent<AeroController>();

        _myParts = GetComponentsInChildren<VehiclePart>(true);

        if (VTOLMPUtils.IsMultiplayer())
        {
            var playerVehicleNetSync = wm.GetComponent<PlayerVehicleNetSync>();
            if (playerVehicleNetSync)
            {
                _vanillaParts = playerVehicleNetSync.vehicleParts;
                var parts = _vanillaParts.ToList();
                parts.Add(_myParts);
                playerVehicleNetSync.vehicleParts = parts.ToArray();
            }

            var damageSync = GetComponent<DamageSync>();
            if (damageSync)
                damageSync.actor = wm.actor;
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

            if (wm.hardpointTransforms[index] && hardpoint)
            {
                var wmHp = wm.hardpointTransforms[index];

                hpTransforms.Add(new Tuple<Transform, Transform, Vector3, Quaternion>(wmHp, wmHp.parent,
                    wmHp.localPosition, wmHp.localRotation));
                
                wmHp.SetParent(hardpoint);
                wmHp.localPosition = Vector3.zero;
                wmHp.localRotation = Quaternion.identity;

                // Adding some code for stuff so equips (hopefully) jettison when hp dies.
                
                if (!hardpoint.GetComponent<VehiclePart>()) continue;
                
                var hpVp = hardpoint.gameObject.AddComponent<HardpointVehiclePart>();
                hpVp.wm = wm;
                hpVp.hpIdx = index;
            }
        }

        // Detaching hardpoints that no longer exist
        foreach (var i in detachHpIdx)
        {
            if (configurator)
            {
                configurator.DetachImmediate(i);
            }
            if (wm.GetEquip(i))
                wm.JettisonEq(i);
        }
        
        // Toggling existing wings off
        foreach (var disableObject in disableObjects)
        {
            ToggleObject(disableObject, wm);
        }

        foreach (var wing in GetComponentsInChildren<Wing>(true))
        {
            wing.SetParentRigidbody(wm.vesselRB);
            wing.enabled = true;
        }

        var flightInfo = wm.actor.flightInfo;
        
        // Set up wing vapor
        foreach (var wingVaporParticles in GetComponentsInChildren<WingVaporParticles>(true))
        {
            wingVaporParticles.flightInfo = flightInfo;
        }
        
        
        // Set up nav / strobe / formation lights
        controller.battery = wm.battery;
        controller.flightInfo = flightInfo;
        controller.SetupLights();
    }

    public void ToggleObject(string path, WeaponManager wm, bool enable = false)
    {
        var t = wm.transform;

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

    private void ToggleNode(GameObject obj, bool cull = true)
    {
        var renderers = obj.GetComponentsInChildren<Renderer>();
        var canvasRenderers = obj.GetComponentsInChildren<CanvasRenderer>();
        var vrInt = obj.GetComponentInChildren<VRInteractable>();
        vrInt.enabled = !cull;
        foreach (var renderer in renderers)
        {
            renderer.enabled = !cull;
        }

        foreach (var canvasRenderer in canvasRenderers)
        {
            canvasRenderer.cull = cull;
        }
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
