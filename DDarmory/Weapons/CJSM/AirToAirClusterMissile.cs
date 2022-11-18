using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VTOLVR.Multiplayer;

public class AirToAirClusterMissile : ClusterMissile
{
	protected override void Start()
	{
		base.Start();
		if (!this.clusterMissileSync)
		{
			this.clusterMissileSync = base.GetComponent<ClusterMissileSync>();
		}
	}

	public override void Update()
	{
		if (base.fired && !this.mpRemote && !this.launchedSubMl && this.lastTargetDistance < this.deployDistance)
		{
			this.launchedSubMl = true;
			if (this.clusterMissileSync)
			{
				this.clusterMissileSync.CMissile_OnSubLaunch();
			}
			base.StartCoroutine(this.AASubLaunchRoutine());
		}
	}

	private IEnumerator AASubLaunchRoutine()
	{
		this.JettisonFairings();
		yield return new WaitForSeconds(0.5f);
		this.altOverTarget = base.transform.position.y - base.estTargetPos.y;
		this.isLaunchingSubML = true;
		int roleMask = Actor.GetRoleMask(new Actor.Roles[]
		{
			this.targetRole
		});
		int i = 0;
		Vector3 vector = base.transform.position + 10f * base.transform.forward;
		List<Actor> tgts = new List<Actor>();
		TargetManager.instance.GetAllOpticalTargetsInView(base.actor, this.clusterTargetFov, 10f, 2f * this.deployDistance, roleMask, vector, base.estTargetPos - vector, tgts, true, false);
		int tgtCount = tgts.Count;
		tgts.Sort((Actor a, Actor b) => (a.position - base.estTargetPos).sqrMagnitude.CompareTo((b.position - base.estTargetPos).sqrMagnitude));
		if (tgtCount > 0)
		{
			while (this.subMl.missileCount > 0)
			{
				Actor tgt = tgts[i];
				this.FireSubMissile(tgt);
				i = (i + 1) % tgtCount;
				yield return new WaitForSeconds(this.subMissileInterval);
				tgt = null;
			}
		}
		yield return new WaitForSeconds(0.5f);
		this.proxyDetonateRange = 3000f;
		yield break;
	}

	[Header("Cluster Missile")]
	public Actor.Roles targetRole;

	public ClusterMissileSync clusterMissileSync;
}
