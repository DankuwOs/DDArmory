using System;
using HarmonyLib;
using UnityEngine;

public class HPEquipThrustReverser : HPEquippable, IMassObject
{
    public enum ReverserState
    {
        Off,
        Auto,
        On
    }

    public ReverserState defaultState = ReverserState.Off;

    public AnimationToggle animationToggle;

    public AnimationCurve reverserCurve;
    
    [Tooltip("Minimum value on the throttle to deploy it")]
    public float minTriggerValue;
    
    public float mass;

    private ReverserState _state = ReverserState.Off;

    private ModuleEngine[] engines;
    
    private bool _deployed;

    public override void OnEquip()
    {
        base.OnEquip();
        
        if (!weaponManager)
            return;

        var vcm = weaponManager.GetComponent<VehicleControlManifest>();
        if (!vcm)
            return;

        engines = weaponManager.vm.engines;

        vcm.throttle.OnTriggerAxis.AddListener(TriggerAxis);

        _state = defaultState;
    }

    private void TriggerAxis(float axis)
    {
        if (_state != ReverserState.Auto) return;
        
        if (axis > minTriggerValue && !_deployed)
        {
            animationToggle.Deploy();
            _deployed = true;
            return;
        }
        
        
        if (axis < minTriggerValue && _deployed)
        {
            animationToggle.Retract();
            _deployed = false;
        }
    }

    public void SetReverserState(Int32 state)
    {
        _state = (ReverserState)state;
        switch (state)
        {
            case 0:
                animationToggle.Retract();
                break;
            case 2:
                animationToggle.Deploy();
                break;
            case 1:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private void LateUpdate()
    {
        if (engines == null || engines.Length == 0)
            return;

        foreach (var moduleEngine in engines)
        {
            var animTime = animationToggle.GetT();


            if (animTime <= 0.05f)
                return;

            var moduleEngineTraverse = Traverse.Create(moduleEngine);

            moduleEngineTraverse.Property("finalThrust")
                .SetValue(moduleEngine.finalThrust * reverserCurve.Evaluate(animTime));
        }
    }

    public float GetMass()
    {
        return mass;
    }
}