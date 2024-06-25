using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HPEquipBurstLauncher : HPEquipOpticalML
{
    [Header("Burst Launcher")] public int[] burstCount;

    [Tooltip("Delay between each missile")]
    public float delay;

    [Tooltip("Delay between each burst")] public float fireDelay;

    public bool friendlyFire;

    public float searchRadius;

    [Tooltip("Optional")] public List<MissileFairing> caps = new();

    private bool _firing;

    private int _burstIdx;

    private Coroutine _coroutine;

    private VehicleControlManifest _vcm;

    public override void OnEquip()
    {
        base.OnEquip();

        var eqFuncs = equipFunctions.ToList();

        var equipFunction1 = new EquipFunction
        {
            optionName = "Burst Count",
            optionReturnLabel = burstCount[_burstIdx].ToString()
        };
        equipFunction1.optionEvent =
            (EquipFunction.OptionEvent)Delegate.Combine(equipFunction1.optionEvent,
                new EquipFunction.OptionEvent(CycleRipple));

        eqFuncs.Add(equipFunction1);
        equipFunctions = eqFuncs.ToArray();


        oml.OnFiredMissileIdx += delegate(int i)
        {
            if (i < caps.Count)
                caps[i].Jettison();
        };

        _vcm = weaponManager.GetComponent<VehicleControlManifest>();
        foreach (var joystick in _vcm.joysticks)    
        {
            joystick.OnMenuButtonUp.AddListener(OnReleasedWeaponButton);
        }
    }

    public override void OnUnequip()
    {
        base.OnUnequip();

        foreach (var joystick in _vcm.joysticks)
        {
            joystick.OnMenuButtonUp.RemoveListener(OnReleasedWeaponButton);
        }
    }

    public override void Fire()
    {
        if (burstCount[_burstIdx] == 1)
        {
            base.Fire();
            return;
        }
        
        if (_coroutine != null)
            StopCoroutine(_coroutine);
        
        StartCoroutine(FiringRoutine());
    }

    public override void OnCycleWeaponButton()
    {
        if (autoUncage || !manualUncaged)
        {
            base.OnCycleWeaponButton();
            return;
        }

        Fire();
    }

    private void OnReleasedWeaponButton()
    {
        _firing = false;
    }
    
    private IEnumerator FiringRoutine()
    {
        _firing = true;
        while (_firing)
        {
            var tgtPoint = weaponManager.opticalTargeter.laserPoint;

            List<Actor> actors = new();
            Actor.GetActorsInRadius(tgtPoint.point, searchRadius, weaponManager.actor.team,
                friendlyFire ? TeamOptions.BothTeams : TeamOptions.OtherTeam, actors);
            actors.OrderBy(actor => Vector3.Distance(actor.position, tgtPoint.point));

            if (actors.Count == 0)
            {
                base.Fire();
                break;
            }
            
            for (int i = 0; i < burstCount[_burstIdx]; i++)
            {
                Actor actor;
                if (i < actors.Count)
                    actor = actors[i];
                else
                    break;

                if (!actor || !actor.alive)
                {
                    for (int j = i; j < actors.Count; j++)
                    {
                        var newActor = actors[j];
                        if (!newActor)
                            continue;

                        actor = newActor;
                        break;
                    }
                }

                if (!TryFireOnActor(actor))
                {
                    break;
                }

                yield return new WaitForSeconds(delay);
            }
            yield return new WaitForSeconds(fireDelay);
        }
        
        ToggleCombinedWeapon();
    }

    private bool TryFireOnActor(Actor actor)
    {
        if (oml.missileCount <= 0) return false;
        var missile = oml.GetNextMissile();
        
        if (!missile)
            return false;

        missile.SetOpticalTarget(actor.transform, actor);
        oml.FireMissile();
        
        return true;
    }

    public void ToggleCombinedWeapon()
    {
        weaponManager.ToggleCombinedWeapon();
        if (autoUncage || !weaponManager.currentEquip || weaponManager.currentEquip is not HPEquipBurstLauncher) return;
        
        manualUncaged = false;
        oml.boresightFOVFraction = 0f;
        HPEquipBurstLauncher hpequipOpticalML = (HPEquipBurstLauncher)weaponManager.currentEquip;
        hpequipOpticalML.manualUncaged = _firing;
        hpequipOpticalML.oml.boresightFOVFraction = uncagedFOVFraction;
    }

    private string CycleRipple()
    {
        _burstIdx = (_burstIdx + 1) % burstCount.Length;
        return burstCount[_burstIdx].ToString();
    }

    public override void SaveEquipData(ConfigNode weaponNode)
    {
        base.SaveEquipData(weaponNode);
        weaponNode.SetValue("burstIdx", _burstIdx);
    }

    public override void LoadEquipData(ConfigNode weaponNode)
    {
        base.LoadEquipData(weaponNode);
        weaponNode.TryGetValue("burstIdx", out int idx);
        _burstIdx = idx;
    }
}