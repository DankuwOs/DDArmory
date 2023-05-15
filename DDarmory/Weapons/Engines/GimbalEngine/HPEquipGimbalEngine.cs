using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DefaultExecutionOrder(-1)]
public class HPEquipGimbalEngine : HPEquipEngine
{
    [Header("Gimbal Engine")]
    public List<AeroController.ControlSurfaceTransform> controlSurfaceTransforms = new List<AeroController.ControlSurfaceTransform>();

    private AeroController _controller;

    private VehicleInputManager _vehicleInputManager;

    public override void Initialize(LoadoutConfigurator configurator = null)
    {
        base.Initialize(configurator);
        
        var wm = configurator ? configurator.wm : weaponManager;
        _controller = wm.GetComponent<AeroController>();

        var controlSurfaces = _controller.controlSurfaces.ToList();
        controlSurfaces.AddRange(controlSurfaceTransforms);
        _controller.controlSurfaces = controlSurfaces.ToArray();
        foreach (var controlSurfaceTransform in controlSurfaceTransforms)
        {
            controlSurfaceTransform.Init(); // Neo 
        }
        
        _vehicleInputManager = wm.GetComponent<VehicleInputManager>();
    }

    public override void UnEquip(LoadoutConfigurator configurator = null)
    {
        base.UnEquip();
        
        var wm = configurator ? configurator.wm : weaponManager;
        _controller = wm.GetComponent<AeroController>();

        var controlSurfaces = _controller.controlSurfaces.ToList();
        foreach (var controllerSurface in _controller.controlSurfaces)
        {
            foreach (var gimbalSurface in controlSurfaceTransforms)
            {
                if (controllerSurface.transform == gimbalSurface.transform)
                {
                    controlSurfaces.Remove(gimbalSurface);
                }
            }
        }
        _controller.controlSurfaces = controlSurfaces.ToArray();
    }
    
    private void Update()
    {
        if (!_vehicleInputManager)
            return;

        var yaw = _vehicleInputManager.outputPYR.y;
        var absYaw = Mathf.Abs(yaw);

        if (absYaw < 0.4f)
            return;

        var left = yaw < 0;

        foreach (var engine in _engines)
        {
            var tfPoint = transform.InverseTransformPoint(engine.transform.position);
            var leftEngine = tfPoint.x < 0;

            var throttle = engine.inputThrottle * (leftEngine ? left ? 1 - absYaw : 1 : left ? 1 : 1 - absYaw);
            
            engine.SetThrottle(throttle);
        }
    }
}