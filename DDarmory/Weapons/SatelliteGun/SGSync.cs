using System;
using UnityEngine;
using VTNetworking;

public class SGSync : VTNetSyncRPCOnly
{
	protected override void OnNetInitialized()
	{
		if (netEntity == null)
		{
			Debug.LogError("Floaty has no netEntity!");
		}
		_satellite = GetComponent<HPEquipSG>();
		_windingWeapon = GetComponent<WindingWeapon>();
		if (isMine)
		{
			_satellite.OnFire.AddListener(StartFire);
			_satellite.OnStop.AddListener(StopFire);
			_windingWeapon.onFired.AddListener(Fire);
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

	// Token: 0x0600008C RID: 140 RVA: 0x00004D34 File Offset: 0x00002F34
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

	// Token: 0x0600008E RID: 142 RVA: 0x00004DC1 File Offset: 0x00002FC1
	[VTRPC]
	public void RPC_Fired()
	{
		_windingWeapon.onFired.Invoke();
	}

	// Token: 0x040000B0 RID: 176
	private HPEquipSG _satellite;

	// Token: 0x040000B1 RID: 177
	private WindingWeapon _windingWeapon;
}
