using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VTOLVR.DLC.EW;

namespace DDArmory.Weapons.Loitering.ALTIUS;

public class ALTIUSGuidance : AirLaunchedDecoyGuidance
{
    // COLD = COLD
    // DRFM = KILL
    // NOISE = INTERCEPT
    // DECOY = DECOY

    public float boosterDist = 3000f;

    public float propLeadTimeMult = 0.15f;
    
    public float killProxyRange = 7.5f;
    public float interceptProxyRange = 20f;
    
    public UnityEvent OnKILLTIME;
    public Actor.Roles killTargets;

    public Transform opticalFwd;
    public float opticalFoV;
    public float opticalDist;

    public float maxEngageDist = 16000f;

    [HideInInspector]
    public List<ModuleRWR.RWRContact> rwrContacts = new List<ModuleRWR.RWRContact>();
    [HideInInspector]
    public List<Missile> missiles = new List<Missile>();

    public Actor TargetActor => _targetActor;
    
    private Actor _targetActor;
    private Vector3 _lastKnownVel = Vector3.zero;
    private float _lastT = -1;
    private bool _boosted;

    public override void Awake()
    {
        base.Awake();
        if (_uploadedPathPoints == null)
        {
            _uploadedPathPoints = new List<FixedPoint>();
        }
    }
    
    

    public override void InitJamming()
    {
        if (initialMode == TransmitModes.DECOY)
        {
            SetDecoy(decoyModel);
        }
    }

    public void Custom_SetJamDRFM(Actor manualTarget = null)
    {
        if (!guidanceEnabled)
        {
            initialMode = TransmitModes.DRFM;
            decoyTransmitMode = TransmitModes.DRFM;
            initialDRFMTarget = manualTarget;
            _targetActor = manualTarget;
            Debug.Log($"[ALTIUSGuidance_KILL]: Set target actor '{(_targetActor ? _targetActor.actorName : "null")}'");
            targetMode = (manualTarget ? TargetModes.TSD : TargetModes.AUTO);
            return;
        }
        StopAllJam();
        
        _targetActor = manualTarget;
        
        if (killTargets.HasFlag(Actor.Roles.Missile))
            killTargets &= ~Actor.Roles.Missile;
        
        Debug.Log($"[ALTIUSGuidance_KILL]: Set target actor post guidance mode '{(_targetActor ? _targetActor.actorName : "null")}'");
    }

    public void Custom_SetJamNoise(EMBands noiseBand, Actor manualTgt = null)
    {
        if (!guidanceEnabled)
        {
            initialMode = TransmitModes.NOISE;
            decoyTransmitMode = TransmitModes.NOISE;
            targetMode = TargetModes.AUTO;
            
            FindInterceptTarget();
            if (_targetActor == null)
                killTargets |= Actor.Roles.Missile;
            return;
        }
        StopAllJam();
        if (!rwrContacts.Any() && !missiles.Any())
        {
            initialMode = TransmitModes.DRFM;
            decoyTransmitMode = TransmitModes.DRFM;
            initialDRFMTarget = manualTgt;
            _targetActor = manualTgt;
            targetMode = (manualTgt ? TargetModes.TSD : TargetModes.AUTO);
            killTargets |= Actor.Roles.Missile;
            return;
        }
        
        FindInterceptTarget();
        
        Debug.Log($"[ALTIUSGuidance_INTERCEPT]: Set target actor '{(_targetActor ? _targetActor.actorName : "null")}'");
    }

    public override Vector3 GetGuidedPoint()
    {
        var gotCustomPoint = GetCustomPoint(out var overridePoint);
        return gotCustomPoint ? overridePoint : base.GetGuidedPoint();
    }

    private bool GetCustomPoint(out Vector3 point)
    {
        if (_lastT < 0)
            _lastT = Time.time;
        
        point = Vector3.zero;
        
        switch (decoyTransmitMode)
        {
            case TransmitModes.NOISE:
            case TransmitModes.DRFM:

                var tgt = FindTarget();
                if (tgt == null)
                    return false;
                

                var vel = tgt.velocity;
                var dT = Time.time - _lastT;
                var estAccel = (vel - _lastKnownVel) / dT;
                
                _lastT = Time.time;
                _lastKnownVel = vel;

                float leadTimeMult = missile.leadTimeMultiplier;
                if (!_boosted)
                    leadTimeMult = propLeadTimeMult;

                var targetLeadPoint = Missile.BallisticLeadTargetPoint(tgt.position,
                    tgt.velocity, missile.rb.position, missile.rb.velocity,
                    Mathf.Max(missile.minBallisticCalcSpeed, missile.currentSpeed), leadTimeMult,
                    missile.maxBallisticOffset, missile.maxLeadTime, estAccel, missile.minGuidanceSimSpeed);
                
                point = targetLeadPoint;

                if (!_boosted && ShouldBoost(tgt, targetLeadPoint))
                {
                    OnKILLTIME?.Invoke();
                    _boosted = true;
                }

                return true;
        }

        return false;
    }

    public override Actor GetAutoJamTarget()
    {
        var overrideTarget = FindTarget();
        return overrideTarget == null ? base.GetAutoJamTarget() : overrideTarget;
    }

    private Actor FindTarget()
    {
        if (_targetActor != null && (_targetActor.hadHealth && _targetActor.alive))
        {
            // Update the target data for proxy det
            missile.estTargetPos = _targetActor.position;
            missile.estTargetVel = _targetActor.velocity;
            
            return _targetActor;
        }

        if (initialMode == TransmitModes.NOISE)
        {
            FindInterceptTarget();
            if (_targetActor != null) 
                return _targetActor;
        }
        
        var contacts = rwr.contacts.Where(c => c.radarActor != null && killTargets.HasFlag(c.radarActor.finalCombatRole)).ToList();
        if (!contacts.Any())
            return null;

        List<Actor> targetsSeen = new List<Actor>();
        TargetManager.instance.GetAllOpticalTargetsInView(missile.actor, opticalFoV, 500f, opticalDist, (int)killTargets, opticalFwd.position, opticalFwd.forward, targetsSeen);

        List<Actor> actors = new List<Actor>();
        actors.AddRange(contacts.ConvertAll(c => c.radarActor));
        actors.AddRange(targetsSeen);
        
        actors.Sort(((actor, actor1) => Vector3.Distance(transform.position, actor.position).CompareTo(Vector3.Distance(transform.position, actor1.position))));
        actors.RemoveAll(a => !a.alive);
                    
        var target = actors.FirstOrDefault();

        if (target != null && Vector3.Distance(transform.position, target.position) < maxEngageDist)
        {
            Debug.Log($"[ALTIUSGuidance_FindTarget]: Set target to '{target.actorName}'");
            _targetActor = target;
        }

        return target;
    }

    private void FindInterceptTarget()
    {
        bool foundTgt = false;
        if (missiles.Any())
        {
            if (missile?.launchedByActor)
            {
                var launchedByActor = missile.launchedByActor;
                missiles.Sort((m1, m2) => Vector3.Distance(launchedByActor.position, m1.transform.position).CompareTo(Vector3.Distance(launchedByActor.position, m2.transform.position)));
            }
            else
            {
                missiles.Sort((m1, m2) => Vector3.Distance(transform.position, m1.transform.position).CompareTo(Vector3.Distance(transform.position, m2.transform.position)));
            }

            var tgtMissile = missiles.First();
            if (tgtMissile.actor)
            {
                _targetActor = missiles.First().actor;
                foundTgt = true;
            }
        }
        
        if (!rwrContacts.Any() || foundTgt)
            return;
        
        if (missile?.launchedByActor)
        {
            var launchedByActor = missile.launchedByActor;
            rwrContacts.Sort((c1, c2) => Vector3.Distance(launchedByActor.position, c1.estimatedPosition).CompareTo(Vector3.Distance(launchedByActor.position, c2.estimatedPosition)));
        }
        else
        {
            rwrContacts.Sort((c1, c2) => Vector3.Distance(transform.position, c1.estimatedPosition).CompareTo(Vector3.Distance(transform.position, c2.estimatedPosition)));
        }
        
        // is this cursed?
        var contact = rwrContacts.FirstOrDefault(c => c.radarActor?.GetMissile() != null) ?? rwrContacts.FirstOrDefault(c => c.supportingMissile) ?? rwrContacts.First();
        
        if (Vector3.Distance(transform.position, contact.estimatedPosition) > maxEngageDist)
            return;
        
        _targetActor = contact.radarActor;
    }

    private bool ShouldBoost(Actor tgt, Vector3 targetLeadPos)
    {
        var leadDir = (targetLeadPos - transform.position).normalized;

        if (Vector3.Angle(transform.forward, leadDir) > 20)
            return false;

        var dist = Vector3.Distance(transform.position, targetLeadPos);
        
        return tgt.velocity.magnitude > 100f && (dist < boosterDist ||
                                                 (Vector3.Dot(transform.forward, tgt.transform.forward) > 0.25f &&
                                                  dist < boosterDist * 2.5f));
    }
}