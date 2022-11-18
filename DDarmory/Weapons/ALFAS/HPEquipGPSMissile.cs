using UnityEngine;

public class HPEquipGPSMissile : HPEquipMissileLauncher
{
	public virtual bool IsLaunchAuthorized()
	{
		return ml.missileCount > 0 && weaponManager.gpsSystem.hasTarget && Vector3.Angle(weaponManager.transform.forward, weaponManager.gpsSystem.currentGroup.currentTarget.worldPosition - weaponManager.transform.position) < offBoresightLaunchAngle;
	}

	public override void OnStartFire()
	{
		base.OnStartFire();
		if (IsLaunchAuthorized())
		{
			Missile nextMissile = ml.GetNextMissile();
			LoiterGuidance component = nextMissile.GetComponent<LoiterGuidance>();
			if (weaponManager.gpsSystem.currentGroup.targets.Count > 0)
			{
				component.gpsPoint = weaponManager.gpsSystem.currentGroup.currentTarget;
			}
			ml.FireMissile();
			ml.OnFiredMissileIdx += delegate(int i)
			{
				ml.missiles[i].actor.role = Actor.Roles.Air;
			};
			weaponManager.ToggleCombinedWeapon();
		}
	}

	[Header("GPS Missile")]
	public float offBoresightLaunchAngle = 15f;
}
