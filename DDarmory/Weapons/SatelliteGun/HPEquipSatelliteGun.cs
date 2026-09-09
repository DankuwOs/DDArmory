using System;
using UnityEngine;
using VTOLVR.Multiplayer;

namespace DDArmory.Weapons.SatelliteGun;

public class HPEquipSatelliteGun : HPEquipGun
{
    public CWB_WindingWeapon windingWeapon;
    

    public Transform satelliteTF;

    public AnimationCurve rotationSpeed;

    public float height;
    
    public Light laserSpotLight;
    public float fogLightMult = 5;
    public Light laserFogLight;
    public AnimationCurve lightCurve;
    
    public ParticleSystem ps;
    
    [Header("Handheld")]
    public bool handheld;
    public Transform laserTf;
    public LineRenderer laserLineRenderer;
    public Light laserLight;
    public LayerMask raycastLayers;

    [HideInInspector]
    public bool remote;

    public Action<LaserObjectParams> OnUpdateLaser;

    private Quaternion _prevRot = Quaternion.identity;
    
    

    public override void OnEquip()
    {
        base.OnEquip();
        windingWeapon.OnWind.AddListener(OnWind);

        var psMain = ps.main;
        
        psMain.startSpeed = new ParticleSystem.MinMaxCurve(gun.bulletInfo.speed);
    }

    public override Vector3 GetAimPoint()
    {
        return Physics.Raycast(gun.fireTransforms[0].position, gun.fireTransforms[0].forward, out var hit, height * 10) ? hit.point : base.GetAimPoint();
    }

    public override void OnStartFire()
    {
        if (handheld)
            return;
        windingWeapon.StartWinding();
    }

    public void RemoteStartFire()
    {
        windingWeapon.StartWinding();
    }

    public override void OnStopFire()
    {
        if (handheld)
            return;
        windingWeapon.StopWinding();
    }

    public void RemoteStopFire()
    {
        windingWeapon.StopWinding();
    }

    private void Update()
    {
        satelliteTF.transform.position = new Vector3(transform.position.x, height, transform.position.z);
        var point = Physics.Raycast(gun.fireTransforms[0].position, gun.fireTransforms[0].forward, out var hit, height * 10) ? hit.point : GetAimPoint();
        laserFogLight.transform.position = point + Vector3.up * (laserFogLight.range * 0.65f);
        //laserFogLight.transform.position = point + (-(point - satelliteTF.position).normalized * (laserFogLight.range * 0.75f)); // Doesn't even function and i cant math lmeow
        
        UpdateTargeting();
        UpdateLaser();
    }

    protected virtual Vector3 GetTargetPoint()
    {
        if (weaponManager == null)
            return gun.fireTransforms[0].forward * height;

        if (handheld)
        {
            var targetPos = Physics.Raycast(laserTf.position, laserTf.forward, out var laserPoint, height * 10)
                ? laserPoint.point
                : gun.fireTransforms[0].forward * height;
            return targetPos;
        }
        
        var targeter = weaponManager.opticalTargeter;
        if (targeter == null)
            return gun.fireTransforms[0].forward * height;
        if (targeter.locked)
        {
            var targetPos = targeter.lockedActor ? gun.GetCalculatedTargetPosition(targeter.lockedActor, true) : targeter.lockTransform.position;
            return targetPos;
        }
        var point = Physics.Raycast(gun.fireTransforms[0].position, gun.fireTransforms[0].forward, out var hit, height * 10) ? hit.point : gun.fireTransforms[0].forward * height;
        
        return point;
    }

    protected virtual void UpdateTargeting()
    {
        var targetPoint = GetTargetPoint();
        
        var lookRotation = Quaternion.LookRotation(targetPoint - satelliteTF.position);
        // Use previous rotation instead of the current transforms one to keep it stable since the satellite is still parented to us.
        var newRot = Quaternion.RotateTowards(_prevRot, lookRotation,
            rotationSpeed.Evaluate(windingWeapon.WindT()));
        satelliteTF.rotation = newRot;
        _prevRot = newRot;
    }

    public void OnWind(float t)
    {
        if (!remote)
            gun.SetFire(t > windingWeapon.windUpTime - 0.0001f);
        
        laserSpotLight.intensity = lightCurve.Evaluate(t / windingWeapon.windUpTime);
        laserFogLight.intensity = lightCurve.Evaluate(t / windingWeapon.windUpTime) * fogLightMult;
    }

    public void UpdateLaser()
    {
        if (!handheld)
            return;
        
        bool sendLaserParams = !remote && VTOLMPUtils.IsMultiplayer();
        
        LaserObjectParams laserObjectParams = null;
        if (sendLaserParams)
            laserObjectParams = new LaserObjectParams();
        
        if (Physics.Raycast(laserTf.position, laserTf.forward, out var hitInfo, 75000f, raycastLayers,
                QueryTriggerInteraction.Ignore))
        {
            laserLineRenderer.SetPosition(1, hitInfo.point);
            
            var lightPos = hitInfo.point - (laserTf.forward * 0.05f); // Move the light position a little in front of the hit point 
            
            laserLight.transform.position = lightPos;
            
            if (sendLaserParams)
            {
                laserObjectParams.laserEnd = VTMapManager.WorldToGlobalPoint(hitInfo.point);
                laserObjectParams.laserLightPos = VTMapManager.WorldToGlobalPoint(lightPos);
            }
            return;
        }

        var forwardPos = laserTf.position + laserTf.forward * 8000f;
        
        laserLineRenderer.SetPosition(1, forwardPos);
        laserLight.transform.position = forwardPos;

        if (!sendLaserParams) return;
        
        laserObjectParams.laserEnd = VTMapManager.WorldToGlobalPoint(forwardPos);
        laserObjectParams.laserLightPos = VTMapManager.WorldToGlobalPoint(forwardPos);

        OnUpdateLaser?.Invoke(laserObjectParams);
    }

    public void RemoteUpdatelaser(LaserObjectParams laserObjectParams)
    {
        laserLineRenderer.SetPosition(1, VTMapManager.GlobalToWorldPoint(laserObjectParams.laserEnd));
        laserLight.transform.position = VTMapManager.GlobalToWorldPoint(laserObjectParams.laserLightPos);
    }
    
    
    public class LaserObjectParams
    {
        public Vector3D laserEnd;
        public Vector3D laserLightPos;
    }
}