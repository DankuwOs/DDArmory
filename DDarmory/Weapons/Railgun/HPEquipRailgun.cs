using System.Collections;
using DDArmory.Weapons.Utils;
using UnityEngine;
using UnityEngine.Events;

namespace DDArmory.Weapons.Railgun;

public class HPEquipRailgun : HPEquipGun, IGunAimpointOnly
{
    [Header("Railgun")]
    public AnimationToggle hatchToggle;
    
    public AnimationToggle heatsinkToggle;

    
    [Header("Heatsink")]
    public MeshRenderer heatsink;
    

    public float cooldownTime;
    
    [ColorUsage(true, true)]
    public Color heatsinkColor;
    
    public AudioSource heatsinkSource;

    public float heatsinkAudioLerpSpeed;
    

    [Header("Winding")]
    public CWB_WindingWeapon windingWeapon;
    
    public AudioSource cantWindSource;

    public ParticleSystem barrelSystem;

    public AnimationCurve emissionCurve;

    public AnimationCurve intensityCurve;

    public MeshRenderer volumetricRenderer;
    
    
    private float _currentCD;
    private bool _trigHeld;

    private MFDRadarUI _radarUI;
    
    
    public UnityEvent OnWindEvent;
    public UnityEvent OnFireEvent;
    public UnityEvent OnStopWindEvent;

    public override void OnEquip()
    {
        base.OnEquip();
        heatsinkSource.volume = 0;
        barrelSystem.SetEmissionRate(0);

        _radarUI = base.weaponManager.GetComponentInChildren<MFDRadarUI>(true);
    }

    public override void OnStartFire()
    {
        hatchToggle.Deploy();
        _trigHeld = true;
    }

    public override void OnStopFire()
    {
        if (windingWeapon.IsWinding()) 
            windingWeapon.StopWinding();
        
        hatchToggle.Retract();
        _trigHeld = false;
    }

    public override IEnumerator ActivatedRtn()
    {
        while (itemActivated)
        {
            aimPoint.point = CalculateImpact(12000f, 0.5f);
            yield return null;
        }
    }

    public override void OnCycleWeaponButton()
    {
        if (!_trigHeld)
        {
            base.OnCycleWeaponButton();
            return;
        }
        
        if (!hatchToggle.deployed || _currentCD > 0)
        {
            if (cantWindSource)
                cantWindSource.Play();
            return;
        }
        
        windingWeapon.StartWinding();
        
        if (OnWindEvent != null)
            OnWindEvent.Invoke();
    }

    public override void OnReleasedCycleWeaponButton()
    {
        if (!_trigHeld)
        {
            
            base.OnReleasedCycleWeaponButton();
            return;
        }
        
        if (windingWeapon.IsWoundUp())
        {
            gun.FireBullet();
            _currentCD = cooldownTime;
            windingWeapon.StopWindingImmediate();
            
            
            if (OnFireEvent != null)
                OnFireEvent.Invoke();
        }
        else
        {
            
            windingWeapon.StopWinding();
            
            if (OnStopWindEvent != null)
                OnStopWindEvent.Invoke();
        }
    }


    public void Update()
    {
        var windT = windingWeapon.WindT();
        
        barrelSystem.SetEmissionRate(emissionCurve.Evaluate(windT));
        
        if (volumetricRenderer)
            volumetricRenderer.material.SetFloat("_Intensity", intensityCurve.Evaluate(windT));

        if (_currentCD > 0)
            _currentCD = Mathf.Max(0, _currentCD -= Time.deltaTime);
        
        UpdateHeatsink();
    }

    private float CooldownT()
    {
        return 1 - (_currentCD / cooldownTime);
    }

    public void UpdateHeatsink(float remote = -1)
    {
        if (remote > 0)
            _currentCD = remote;
        
        if (_currentCD > 0.0001)
        {
            if (!heatsinkSource.isPlaying)
                heatsinkSource.Play();
            heatsinkToggle.Deploy();
        }
        else
        {
            if (heatsinkSource.isPlaying)
                heatsinkSource.Stop();
            heatsinkToggle.Retract();
        }

        var cdt = CooldownT();
        
        
        heatsink.material.SetColor("_EmissionColor", Color.Lerp(heatsinkColor, Color.black, cdt));
        heatsinkSource.volume = Mathf.MoveTowards(heatsinkSource.volume, cdt, heatsinkAudioLerpSpeed * Time.deltaTime) * 5;
    }

    public override Vector3 GetAimPoint()
    {
        return GetTargetPosition();
    }

    private Vector3 GetTargetPosition()
    {
        Vector3 pos = gun.fireTransforms[0].position + gun.fireTransforms[0].forward * 1000;
        Vector3 vel = Vector3.zero;
        
        
        if (weaponManager.opticalTargeter != null && weaponManager.opticalTargeter.locked)
        {
            pos = weaponManager.opticalTargeter.lockTransform.position;
            vel = weaponManager.opticalTargeter.targetVelocity;
        }
        else if (_radarUI.currentLockedActor && _radarUI.isSOI)
        {
            pos = _radarUI.currentLockedActor.position;
            vel = _radarUI.currentLockedActor.velocity;
        }

        var predictedPos = pos + vel * Time.deltaTime - gun.fireTransforms[0].position;
        var relVel = vel - gun.actor.velocity;
        var leadT = VectorUtils.CalculateLeadTime(predictedPos, relVel, gun.bulletInfo.speed);
        
        pos += vel * leadT;

        var initialAimPos = pos - 0.5f * leadT * leadT * Physics.gravity;

        var rec = 1;

        pos = Recurr_GetCalcTgtPosInteg(initialAimPos, pos, 0.1f, ref rec);

        return pos;
    }
    
    private Vector3 Recurr_GetCalcTgtPosInteg(Vector3 initialAimPos, Vector3 targetPos, float hitThreshold, ref int recurrsions)
    {
        var vector = Integ_CalculateImpact(initialAimPos, targetPos, VTOLVRConstants.GUN_TURRET_CALCULATION_DELTA_TIME);
        if ((vector - targetPos).sqrMagnitude < hitThreshold || recurrsions > 10)
        {
            return initialAimPos;
        }
        initialAimPos -= (vector - targetPos) * VTOLVRConstants.GUN_TURRET_CALCULATION_CORRECTION;
        recurrsions++;
        return Recurr_GetCalcTgtPosInteg(initialAimPos, targetPos, hitThreshold, ref recurrsions);
    }

    private Vector3 Integ_CalculateImpact(Vector3 initialAimPos, Vector3 targetPos, float simDeltaTime)
    {
        var muzzleVelocity = GetMuzzleVelocity();
        var vector = GetFireTransform().position;
        var vector2 = vector;
        var vector3 = weaponManager.actor.velocity + (initialAimPos - vector2).normalized * muzzleVelocity;
        var magnitude = (targetPos - vector2).magnitude;
        var wind = weaponManager.windMaster.wind;
        Ray ray;
        float num;
        for (;;)
        {
            var vector4 = wind - vector3;
            var vector5 = Bullet.CalculateBulletDragAccel(gun.bulletInfo.projectileMass, vector, vector4);
            var vector6 = vector + vector3 * simDeltaTime + 0.5f * simDeltaTime * simDeltaTime * Physics.gravity +
                          0.125f * simDeltaTime * simDeltaTime * vector5;
            vector3 += Physics.gravity * simDeltaTime + vector5 * simDeltaTime / 2f;
            var vector7 =
                Bullet.CalculateBulletDragAccel(gun.bulletInfo.projectileMass, vector6, wind - vector3);
            vector6 += 0.125f * simDeltaTime * simDeltaTime * vector7;
            vector3 += vector7 * simDeltaTime / 2f;
            var plane = new Plane(-vector3, targetPos);
            ray = new Ray(vector, vector6 - vector);
            if (plane.Raycast(ray, out num) && num * num < (vector6 - vector).sqrMagnitude)
            {
                break;
            }

            if ((vector6 - vector2).sqrMagnitude > 2f * magnitude * magnitude)
            {
                return vector6;
            }

            vector = vector6;
        }

        return ray.GetPoint(num);
    }

}