using System;
using UnityEngine;
using UnityEngine.Events;

public class HPEquipSG : HPEquipGun, IMassObject
{
	protected override void OnEquip()
	{
		base.OnEquip();
		_windingWeapon = GetComponent<WindingWeapon>();
		_targeter = weaponManager.opticalTargeter;
		foreach (ParticleSystem particleSystem in systems)
		{
			ParticleSystem.MinMaxCurve minMaxCurve = particleSystem.main.startSpeed;
			bool flag = Math.Abs(minMaxCurve.constant - startSpeed) > 3f;
			if (flag)
			{
				minMaxCurve.constant = startSpeed;
			}
		}
		turret.SetParent(null);
		CustomWeaponsBase.instance.AddObject(turret.gameObject);
		MoveSol(Vector3.zero);
	}

	public override Vector3 GetAimPoint()
	{
		RaycastHit raycastHit;
		return Physics.Raycast(turret.position, turret.forward, out raycastHit, 1000000f) ? raycastHit.point : base.GetAimPoint();
	}

	public virtual void FixedUpdate()
	{
		SatelliteTargeting(_targeter);
	}

	protected virtual Vector3 GetTargetPosition(OpticalTargeter targeter)
	{
		Vector3 result;
		if (!targeter || !targeter.lockTransform)
		{
			result = Vector3.zero;
		}
		else
		{
			Vector3 vector;
			if (targeter.lockedActor)
			{
				Actor lockedActor = targeter.lockedActor;
				vector = gun.GetCalculatedTargetPosition(lockedActor, true);
			}
			else
			{
				vector = targeter.lockTransform.position;
			}
			result = vector;
		}
		return result;
	}

	public virtual void SatelliteTargeting(OpticalTargeter targeter)
	{
		Vector3 targetPosition = GetTargetPosition(targeter);
		if (turret.position.y < Height * 0.95f)
		{
			MoveSol(this.transform.position);
		}
		Transform transform = gun.fireTransforms[0];
		Quaternion quaternion = Quaternion.LookRotation(targetPosition - transform.position);
		float num = gun.isFiring ? firingSpeed : slewSpeed;
		float maxDegreesDelta = num * Time.deltaTime;
		quaternion = Quaternion.RotateTowards(transform.rotation, quaternion, maxDegreesDelta);
		transform.rotation = quaternion;
		
		if (gun.isFiring)
		{
			CamRigRotationInterpolator.ShakeAll(-this.transform.up * 0.3f);
		}
	}

	public override void OnStartFire()
	{
		if (!_windingWeapon || gun.currentAmmo <= 0)
		{
			base.OnStartFire();
		}
		else
		{
			OnFire.Invoke();
		}
	}

	public override void OnStopFire()
	{
		bool flag = !_windingWeapon || gun.currentAmmo <= 0;
		if (flag)
		{
			base.OnStopFire();
		}
		else
		{
			OnStop.Invoke();
		}
	}

	public virtual void MoveSol(Vector3 xz)
	{
		bool flag = turret.rotation != Quaternion.identity;
		if (flag)
		{
			turret.rotation = Quaternion.identity;
		}
		turret.position = new Vector3(xz.x, Height, xz.z);
	}

	public float GetMass()
	{
		return 0.00018f;
	}

	[Header("Satellite Gun")]
	public float Height;

	public Transform turret;

	public float slewSpeed = 1f;

	public float firingSpeed = 0.1f;

	public UnityEvent OnFire = new UnityEvent();

	public UnityEvent OnStop = new UnityEvent();

	[Header("Override PS Values")]
	public ParticleSystem[] systems;

	public float startSpeed = 1000f;

	private WindingWeapon _windingWeapon;

	private OpticalTargeter _targeter;
}
