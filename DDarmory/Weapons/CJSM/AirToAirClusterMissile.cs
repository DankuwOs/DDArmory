using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTOLVR.Multiplayer;

public class AirToAirClusterMissile : ClusterMissile
{
    public override void Start()
    {
        base.Start();
        if (!clusterMissileSync) clusterMissileSync = GetComponent<ClusterMissileSync>();
    }

    public override void Update()
    {
        if (fired && !mpRemote && !launchedSubMl && lastTargetDistance < deployDistance)
        {
            launchedSubMl = true;
            if (clusterMissileSync) clusterMissileSync.CMissile_OnSubLaunch();
            StartCoroutine(AASubLaunchRoutine());
        }
    }

    private IEnumerator AASubLaunchRoutine()
    {
        JettisonFairings();
        yield return new WaitForSeconds(0.5f);
        altOverTarget = transform.position.y - estTargetPos.y;
        isLaunchingSubML = true;
        var roleMask = Actor.GetRoleMask(new Actor.Roles[]
        {
            targetRole
        });
        var i = 0;
        var vector = transform.position + 10f * transform.forward;
        var tgts = new List<Actor>();
        TargetManager.instance.GetAllOpticalTargetsInView(actor, clusterTargetFov, 10f, 2f * deployDistance, roleMask,
            vector, estTargetPos - vector, tgts, true, false);
        var tgtCount = tgts.Count;
        tgts.Sort((Actor a, Actor b) =>
            (a.position - estTargetPos).sqrMagnitude.CompareTo((b.position - estTargetPos).sqrMagnitude));
        if (tgtCount > 0)
            while (subMl.missileCount > 0)
            {
                var tgt = tgts[i];
                FireSubMissile(tgt);
                i = (i + 1) % tgtCount;
                yield return new WaitForSeconds(subMissileInterval);
                tgt = null;
            }

        yield return new WaitForSeconds(0.5f);
        proxyDetonateRange = 3000f;
        yield break;
    }

    [Header("Cluster Missile")] public Actor.Roles targetRole;

    public ClusterMissileSync clusterMissileSync;
}