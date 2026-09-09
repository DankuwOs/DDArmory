using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DebugUtils;
using UnityEngine;
using UnityEngine.Events;

namespace DDArmory.Weapons.XGM11;

public class XGMGuidance : MissileGuidanceUnit
{
    public float deployRadius = 5000f;

    public float fov = 5f;

    public float minRadius = 250f;

    public Actor.Roles targetRoles = Actor.Roles.Ground;
    
    [Header("Fairings")]
    public MissileFairing[] fairings;
    
    public float fairingDeployTime = 1f;

    public UnityEvent OnJettisonFairings;
    
    [Header("Guns")]
    
    public Gun gun;
    
    public float gunBurstTime = 1f;

    public LineRenderer targetDir;
    public LineRenderer forwardDir;
    public LineRenderer velDir;
    
    private bool _deployed = false;
    
    private Actor _tgtActor;

    private DebugObject.PossiblyMortalText _fwdText;
    private DebugObject.PossiblyMortalText _velText;
    private DebugObject.PossiblyMortalText _targetDotText;

    /*private void Update()
    {
        if (!missile.fired)
            return;
        
        forwardDir?.SetPositions([transform.position, transform.position + transform.forward * 1000]);
        velDir?.SetPositions([transform.position, transform.position + missile.rb.velocity.normalized * 1000]);
        
        /*var fwd = transform.forward;
        var vel = missile.rb.velocity.normalized;
        _fwdText.text = $"Fwd = ({fwd.x:F},{fwd.y:F},{fwd.z:F})";
        _velText.text = $"Vel = ({vel.x:F},{vel.y:F},{vel.z:F})";
        _targetDotText.text = $"Dot = ({Vector3.Dot(fwd, vel):0.0000})";#1#
        
        if (!_deployed)
            return;
        
        targetDir?.SetPositions([transform.position, GetGuidedPoint()]);
    }*/

    public override void OnBeginGuidance()
    {
        base.OnBeginGuidance();
        
        /*targetDir = DebugUtils.DebugUtils.CreateLineRenderer(transform.position, transform.position, Color.red, transform);
        forwardDir = DebugUtils.DebugUtils.CreateLineRenderer(transform.position, transform.position, Color.blue, transform);
        velDir = DebugUtils.DebugUtils.CreateLineRenderer(transform.position, transform.position, Color.green, transform);
        var debugObject = DebugUtils.DebugUtils.CreateDebugObject(transform);

        var fwd = transform.forward;
        _fwdText = debugObject.AddText($"Fwd = ({fwd.x:F1},{fwd.y:F1},{fwd.z:F1})");
        _velText = debugObject.AddText($"Fwd = ({0:F1},{0:F1},{0:F1})");
        _targetDotText = debugObject.AddText($"Dot = ({0:F1})");*/
        
        
        StartCoroutine(TargetSearchRoutine());
    }

    public override Vector3 GetGuidedPoint()
    {
        if (!_deployed || !_tgtActor)
            return missile.GuidedPoint();
        
        return gun.GetCalculatedTargetPosition(_tgtActor.position, _tgtActor.velocity);
    }

    private IEnumerator TargetSearchRoutine()
    {
        Debug.Log($"Target search routine [Actor = {missile.actor}");
        gun.actor = missile.actor;
        
        while (_guidanceEnabled)
        {
            if (!missile.hasTarget)
            {
                yield return null;
                continue;
            }

            Vector3 tgtPos = missile.staticOpticalTargetLock.point;
            if (missile.opticalTarget)
            {
                tgtPos = missile.opticalTarget.transform.position;
            }
            
            if (Vector3.Distance(transform.position, tgtPos) > deployRadius)
            {
                yield return null;
                continue;
            }

            _deployed = true;
            foreach (var missileFairing in fairings)
            {
                missileFairing.Jettison();
            }
            yield return new WaitForSeconds(fairingDeployTime);
            
            OnJettisonFairings?.Invoke();
            
            List<Actor> tgtActors = new List<Actor>();
            TargetManager.instance.GetAllOpticalTargetsInView(missile.actor, fov, minRadius, deployRadius, (int)targetRoles, transform.position, transform.forward, tgtActors, false, false);
            
            var sortedActors = tgtActors.OrderBy(a => (a.position - transform.position).sqrMagnitude);
            
            foreach (var actor in sortedActors)
            {
                _tgtActor = actor;
                
                yield return null;
                
                while (Vector3.Dot(transform.forward, (GetGuidedPoint() - transform.position).normalized) < 0.99f)
                {
                    yield return null;
                }
                
                float gunTime = 0;
                
                gun.SetFire(true);
                while (gunTime < gunBurstTime)
                {
                    gunTime += Time.deltaTime;
                    yield return null;
                }
                gun.SetFire(false);
                
                if (gun.currentAmmo == 0)
                {
                    yield return new WaitForSeconds(1);
                    if (!missile.detonated)
                    {
                        Debug.Log($"[DDArmory] DETONATING FROM NO AMMO");
                        missile.Detonate();
                    }
                }

                _tgtActor = null;
                yield return null;
            }
            
            yield return null;
        }
    }
}