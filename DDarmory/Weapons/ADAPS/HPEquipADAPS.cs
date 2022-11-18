using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HPEquipADAPS : HPEquipMissileLauncher, IMassObject
{
	public override int GetCount()
	{
		return this.launchers.Sum((MissileLauncher missileLauncher) => missileLauncher.hardpoints.Length);
	}

	private void FixedUpdate()
	{
		if (isEquipped)
		{
			this._time += Time.deltaTime;
			if (this._time >= this.detectionRate)
			{
				this._time = 0f;
				base.StartCoroutine(this.Targeting());
			}
			if (this.detectedActors.Count > 0)
			{
				foreach (Actor actor in this.detectedActors.Keys.ToList<Actor>())
				{
					if (!actor || this.detectedActors[actor] <= 0f)
					{
						this.detectedActors.Remove(actor);
					}
					else
					{
						Dictionary<Actor, float> dictionary = this.detectedActors;
						Actor key = actor;
						dictionary[key] -= Time.deltaTime;
					}
				}
			}
		}
	}

	private IEnumerator Targeting()
	{
		
		if (ml.missileCount == 0)
		{
			yield break;
		}
		List<Actor> tgts = new List<Actor>();
		TargetManager.instance.GetAllOpticalTargetsInView(base.weaponManager.actor, this.fov, 0f, this.range, Actor.GetRoleMask(new Actor.Roles[]
		{
			this.roleMask
		}), this.ml.transform.position, this.ml.transform.forward, tgts, false, false);
		if (tgts.Count == 0)
		{
			yield break;
		}
		tgts.Sort((Actor a, Actor b) => (a.position - this.ml.transform.position).sqrMagnitude.CompareTo((b.position - this.ml.transform.position).sqrMagnitude));
		
		foreach (Actor actor in from e in tgts where !this.detectedActors.ContainsKey(e) select e)
		{
			this.detectedActors.Add(actor, this.forgetTimer);
			Missile missile = this.ml.GetNextMissile();
			missile.SetOpticalTarget(actor.transform, actor, null);
			this.ml.FireMissile();
			yield return new WaitForSeconds(this.timeBetweenFire);
		}
		yield break;
	}

	public float GetMass()
	{
		return this.weight + (from t in this.launchers
		let missile = t.GetNextMissile()
		where missile
		select missile.mass * (float)t.missileCount).Sum();
	}

	[Header("ADAPS")]
	public MissileLauncher[] launchers;

	public Actor.Roles roleMask;

	public float range = 2000f;

	public float fov = 60f;

	public float detectionRate = 0.25f;

	public float timeBetweenFire = 0.3f;

	public float forgetTimer = 3f;

	public float weight;

	private Dictionary<Actor, float> detectedActors = new Dictionary<Actor, float>();

	private float _time;
}
