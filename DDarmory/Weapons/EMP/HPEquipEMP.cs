using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Events;
using VTOLVR.Multiplayer;

namespace DDArmory.Weapons.EMP;

public class HPEquipEMP : HPEquippable
{
    [Header("EMP")] 
    public float radius;

    public float cooldownTime;
    
    public float disableChargeTime = 7.5f;

    public bool invert;

    public AnimationCurve falloffCurve;

    public CWB_WindingWeapon windingWeapon;

    public RadarJammer radarJammer;

    public HeatEmitter heatEmitter;

    public AnimationCurve jammerCurve;

    public float heatSpeed = 2;

    [Header("Cosmetic")]
    public Gradient cooldownColor;

    public Light cooldownLight;

    public AudioSource fireSource;

    public AudioClip empClip;

    [HideInInspector]
    public UnityEvent OnEMP;

    [HideInInspector]
    public UnityEvent<int> OnEMPEntity;
    
    [HideInInspector]
    public UnityEvent<ulong> OnEMPPlayer;

    [Header("Debug")] 
    public Vector3 testPos;

    public Color sphereCol;

    public Color testCol;


    private bool _canWind;

    private float _cooldownT;

    public override int GetCount()
    {
        return (int)_cooldownT;
    }

    public override int GetMaxCount()
    {
        return 1;
    }

    public override void OnEquip()
    {
        base.OnEquip();
        
        if (!weaponManager || !weaponManager.ewCon)
            return;

        radarJammer.Initialize();
        
        radarJammer.jActor = weaponManager.ewCon.jActor;
        radarJammer.battery = weaponManager.battery;
        
        for (int i = 0; i < radarJammer.transmitters.Length; i++)
        {
            var transmitter = radarJammer.transmitters[i];
            transmitter.mode = RadarJammer.TransmitModes.NOISE;
            EMBands band;
            switch (i)
            {
                case 0:
                    band = EMBands.Low; 
                    break;
                case 1:
                    band = EMBands.Mid;
                    break;
                case 2:
                    band = EMBands.High;
                    break;
                default:
                    band = EMBands.High;
                    break;
            }
            transmitter.SetEMBand(band);
        }

        heatEmitter.actor = weaponManager.actor;
    }

    public override void OnStartFire()
    {
        base.OnStartFire();
        if (_canWind)
        {
            windingWeapon.StartWinding();
            for (int i = 0; i < radarJammer.transmitterCount; i++)
            {
                radarJammer.BeginJamProgram(i);
            }
        }
    }

    public override void OnStopFire()
    {
        base.OnStopFire();
        windingWeapon.StopWinding();
        for (int i = 0; i < radarJammer.transmitterCount; i++)
        {
            radarJammer.StopJam(i);
        }
    }

    public override void OnCycleWeaponButton()
    {
        if (windingWeapon.IsWoundUp())
            DoEMP();
        else
            base.OnCycleWeaponButton();
    }

    private void DoEMP()
    {
        List<Actor> actorsInRadius = new List<Actor>();
        Actor.GetActorsInRadius(transform.position, radius, weaponManager.actor.team,
            TeamOptions.BothTeams, actorsInRadius);
        
        //StartCoroutine(EMPActor(weaponManager.actor, transform.position));
        
        //PlayEMPAudio(transform);
        
        foreach (var actor in actorsInRadius)
        {
            if (actor == weaponManager.actor)
                continue;
            StartCoroutine(EMPActor(actor, transform.position));
        }
        
        if (OnEMP != null)
            OnEMP.Invoke();
        OnFire();
        windingWeapon.StopWindingImmediate();
    }

    private IEnumerator EMPActor(Actor actor, Vector3 startPos)
    {
        var wPos = VTMapManager.WorldToGlobalPoint(startPos);
            
        if (VTOLMPUtils.IsMultiplayer())
        {
            var netEntity = actor.GetNetEntity();
            if (netEntity)
                OnEMPEntity?.Invoke(netEntity.entityID);
            var playerInfo = VTOLMPSceneManager.instance.GetPlayer(actor);
            if (netEntity && playerInfo != null)
                OnEMPPlayer.Invoke(playerInfo.steamUser.Id);
        }
        
        Battery battery = null;
        
        if (actor.weaponManager && actor.weaponManager.battery)
        {
            battery = actor.weaponManager.battery;
        }
        
        AIPilot aiPilot = actor.GetComponent<AIPilot>();
        if (aiPilot)
        {
            // Ai pilot stuff is hard ill just destroy em.
            aiPilot.health.Damage(500, transform.position, Health.DamageTypes.Impact, weaponManager.actor);
            yield break;
        }

        AIUnitSpawn aiunitSpawn = actor.GetComponent<AIUnitSpawn>(); // easiest way i thought to disable brozos.
        bool aiUnitSpawnEngageEnemies = false;
        if (aiunitSpawn && aiunitSpawn.engageEnemies)
        {
            aiUnitSpawnEngageEnemies = aiunitSpawn.engageEnemies;
            aiunitSpawn.SetEngageEnemies(false);
        }

        Missile missile = actor.GetComponent<Missile>();
        if (missile)
            missile.Detonate();

        var radars = actor.GetRadars().Where(radar => radar.radarEnabled).ToList(); // Forbids an emp from affecting disabled radars, if 2 emps occur at the same time then the radars can be fucked forevers.

        Dictionary<Radar, bool> radarDict = null;
            
        if (radars.Any())
        {
            radarDict = radars.ToDictionary(radar => radar, radar => radar.radarEnabled);
            foreach (var radar in radars)
            {
                radar.radarEnabled = false;
            }
        }

        yield return new WaitForFixedUpdate();
        
        
        float t = 0;
        float dChargeTime = (1 - GetDrainT(wPos, actor.position)) * disableChargeTime; // Change the duration of being disabled based on distance.
        while (t < dChargeTime)
        {
            if (battery != null)
            {
                float drainAmt = battery.maxCharge * GetDrainT(wPos, actor.position);
                battery.SetCharge(battery.currentCharge - drainAmt); // Not using drain because reasons (why the fuck?????)
            }
            
            t += Time.deltaTime;
            yield return null;
        }

        if (aiunitSpawn)
            aiunitSpawn.SetEngageEnemies(aiUnitSpawnEngageEnemies);

        if (radars.Count > 0)
        {
            foreach (var radar in radars)
            {
                if (radarDict != null && radarDict.TryGetValue(radar, out var enable))
                    radar.radarEnabled = enable;
            }
        }
    }

    public void RemoteEMPActor(Actor actor, Vector3 startPos)
    {
        StartCoroutine(EMPActor(actor, startPos));
    }

    public void OnFire()
    {
        fireSource.Play();
        _cooldownT = cooldownTime;
    }

    private float GetDrainT(Vector3D startPos, Vector3 pos)
    {
        var wPos = VTMapManager.GlobalToWorldPoint(startPos); // Position can change post floating origin shift, this should fix it.
        float t = falloffCurve.Evaluate(Vector3.Distance(wPos, pos) / radius);
        if (invert)
            t = 1 - t;
        return t;
    }

    private void Update()
    {
        _cooldownT = Mathf.MoveTowards(_cooldownT, 0, Time.deltaTime);
        cooldownLight.color = cooldownColor.Evaluate( 1 - (_cooldownT / cooldownTime));
        _canWind = _cooldownT <= 0.0001f; // i dont trust 0.

        if (windingWeapon.IsWinding() || windingWeapon.IsWoundUp())
        {
            heatEmitter.AddHeat(windingWeapon.WindT() * heatSpeed * Time.deltaTime);
        }
        radarJammer.transmitters.Do(e =>
        {
            e.SetGPSTarget(transform.position + (Vector3.forward * 100f), false); // set a gps position just somewhere so the dot can be something.
            e.power = Mathf.MoveTowards(e.power, jammerCurve.Evaluate(windingWeapon.WindT()), 1 * Time.deltaTime);
        });
    }

    public void PlayEMPAudio(Transform parentTf)
    {
        AudioController.instance.PlayOneShot(empClip, parentTf.position, 1, 1, 25, 50, false, 128, parentTf);
    }

    [ContextMenu("Test Audio")]
    private void TestAudio()
    {
        fireSource.Play();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = sphereCol;
        Gizmos.DrawSphere(transform.position, radius);

        float t = falloffCurve.Evaluate(Vector3.Distance(transform.position, transform.TransformPoint(testPos)) / radius);

        if (invert)
            t = 1 - t;
        
        Gizmos.color = Color.Lerp(Color.black, testCol, t);
        Gizmos.DrawSphere(transform.TransformPoint(testPos), 0.15f);
    }
}