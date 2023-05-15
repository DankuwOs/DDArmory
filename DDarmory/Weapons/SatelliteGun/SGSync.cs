using System;
using UnityEngine;
using VTNetworking;

	public class SGSync : VTNetSyncRPCOnly
	{
		protected override void OnNetInitialized()
		{
			if (netEntity == null)
			{
				Debug.LogError("SG Sync has no netEntity!");
			}
			_satellite = GetComponent<HPEquipSG>();
			_windingWeapon = GetComponent<WindingWeapon>();
			if (isMine)
			{
				if (_satellite is HPEquipSGHandHeld sgHandHeld)
				{
					_satelliteHH = sgHandHeld;
					_satelliteHH.OnSetLaser += LaserSync;
				}
				_satellite.OnFire.AddListener(StartFire);
				_satellite.OnStop.AddListener(StopFire);
				_windingWeapon.onFired.AddListener(Fire);
			}
		}

		public void LaserSync(HPEquipSGHandHeld.LaserObjectSync laserObjectSync)
		{
			if (isMine)
			{
				SendRPC("RPC_SGLaserSync", laserObjectSync.enabled? 1 : 0, laserObjectSync.laserStart,
					laserObjectSync.laserEnd, laserObjectSync.laserLightPos);
			}
		}

		public void StartFire()
		{
			if (isMine)
			{
				SendRPC("RPC_SGShouldFire", 1);
			}
		}

		public void StopFire()
		{
			if (isMine)
			{
				SendRPC("RPC_SGShouldFire", 0);
			}
		}
		
		public void Fire()
		{
			if (isMine)
			{
				SendRPC("RPC_Fired", Array.Empty<object>());
			}
		}

		[VTRPC]
		public void RPC_SGShouldFire(int deploy)
		{
			var flag = deploy == 1;
			var flag2 = !isMine;
			if (flag2)
			{
				_satellite.MoveSol(_satellite.transform.position);
				var flag3 = flag;
				if (flag3)
				{
					_satellite.OnStartFire();
				}
				else
				{
					_satellite.OnStopFire();
				}
			}
		}

		[VTRPC]
		public void RPC_Fired()
		{
			_windingWeapon.onFired.Invoke();
		}

		[VTRPC]
		public void RPC_SGLaserSync(int objEnabled, Vector3 startPos, Vector3D endPos, Vector3D lightPos)
		{
			if (!isMine && _satelliteHH)
			{
				_satelliteHH.laserTf.gameObject.SetActive(objEnabled == 1);
				Vector3[] positions = new Vector3[]
				{
					startPos,
					VTMapManager.GlobalToWorldPoint(endPos)
				};
				_satelliteHH.laserRenderer.SetPositions(positions);
				_satelliteHH.pointLight.transform.position = VTMapManager.GlobalToWorldPoint(lightPos);
			}
		}

		private HPEquipSG _satellite;

		private HPEquipSGHandHeld _satelliteHH;

		private WindingWeapon _windingWeapon;
	}