using System;
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

    [HideInInspector]
    public ModuleEngine[] engines;
    
    private bool _deployed;

    protected override void OnEquip()
    {
        base.OnEquip();

        var vcm = weaponManager.GetComponent<VehicleControlManifest>();
        if (!vcm)
            return;

        engines = weaponManager.vm.engines;

        vcm.throttle.OnTriggerAxis.AddListener(TriggerAxis);

        _state = defaultState;
        
        Debug.Log($"[Thrust Reversers]: Engine count: {engines.Length} | VCM Throttle: {vcm.throttle.gameObject} | State: {_state}");
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
        Debug.Log($"[ThrustReversers]: Set state: {state}");
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

    public float GetMass()
    {
        return mass;
    }
}