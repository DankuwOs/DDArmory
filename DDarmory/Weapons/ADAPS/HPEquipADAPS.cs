using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HPEquipADAPS : HPEquipMissileLauncher, IMassObject
{
    public override int GetCount()
    {
        return launchers.Sum((MissileLauncher missileLauncher) => missileLauncher.hardpoints.Length);
    }

    private void FixedUpdate()
    {
        if (!isEquipped) return;

        _time += Time.deltaTime;
        if (_time >= detectionRate)
        {
            _time = 0f;
            StartCoroutine(Targeting());
        }

        if (detectedActors.Count <= 0) return;

        foreach (var actor in detectedActors.Keys.ToList<Actor>())
            if (!actor || detectedActors[actor] <= 0f)
            {
                detectedActors.Remove(actor);
            }
            else
            {
                var dictionary = detectedActors;
                var key = actor;
                dictionary[key] -= Time.deltaTime;
            }
    }

    private IEnumerator Targeting()
    {
        if (ml.missileCount == 0) yield break;
        var tgts = new List<Actor>();
        TargetManager.instance.GetAllOpticalTargetsInView(weaponManager.actor, fov, 0f, range, Actor.GetRoleMask(
            new Actor.Roles[]
            {
                roleMask
            }), ml.transform.position, ml.transform.forward, tgts, false, false);
        if (tgts.Count == 0) yield break;
        tgts.Sort((Actor a, Actor b) =>
            (a.position - ml.transform.position).sqrMagnitude.CompareTo((b.position - ml.transform.position)
                .sqrMagnitude));

        foreach (var actor in from e in tgts where !detectedActors.ContainsKey(e) select e)
        {
            detectedActors.Add(actor, forgetTimer);
            var missile = ml.GetNextMissile();
            missile.SetOpticalTarget(actor.transform, actor, null);
            ml.FireMissile();
            yield return new WaitForSeconds(timeBetweenFire);
        }

        yield break;
    }

    public float GetMass()
    {
        return weight + (from t in launchers
            let missile = t.GetNextMissile()
            where missile
            select missile.mass * t.missileCount).Sum();
    }

    [Header("ADAPS")] public MissileLauncher[] launchers;

    public Actor.Roles roleMask;

    public float range = 2000f;

    public float fov = 60f;

    public float detectionRate = 0.25f;

    public float timeBetweenFire = 0.3f;

    public float forgetTimer = 3f;

    public float weight;

    private Dictionary<Actor, float> detectedActors = new();

    private float _time;
}