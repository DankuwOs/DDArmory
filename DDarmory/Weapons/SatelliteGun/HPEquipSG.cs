using System;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class HPEquipSG : HPEquipGun, IMassObject
	{
		public override void OnEquip()
		{
			base.OnEquip();
			_windingWeapon = GetComponent<WindingWeapon>();
			_targeter = weaponManager.opticalTargeter;
			foreach (var particleSystem in systems)
			{
				var minMaxCurve = particleSystem.main.startSpeed;
				var flag = Math.Abs(minMaxCurve.constant - startSpeed) > 3f;
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
			return Physics.Raycast(gun.fireTransforms[0].position, gun.fireTransforms[0].forward, out var raycastHit, 1000000f) ? raycastHit.point : base.GetAimPoint();
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
					var lockedActor = targeter.lockedActor;
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
			var targetPosition = GetTargetPosition(targeter);
			if (turret.position.y < Height * 0.95f)
			{
				MoveSol(this.transform.position);
			}
			var transform = gun.fireTransforms[0];
			var quaternion = Quaternion.LookRotation(targetPosition - transform.position);
			var num = gun.isFiring ? firingSpeed : slewSpeed;
			var maxDegreesDelta = num * Time.deltaTime;
			quaternion = Quaternion.RotateTowards(transform.rotation, quaternion, maxDegreesDelta);
			transform.rotation = quaternion;
		
			if (gun.isFiring)
			{
				CamRigRotationInterpolator.ShakeAll(Random.onUnitSphere * shakeMagnitude);
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
			var flag = !_windingWeapon || gun.currentAmmo <= 0;
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
			var flag = turret.rotation != Quaternion.identity;
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