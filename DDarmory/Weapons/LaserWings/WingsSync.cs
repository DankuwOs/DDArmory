using UnityEngine;
using VTNetworking;

public class WingsSync : VTNetSyncRPCOnly
{
	protected override void OnNetInitialized()
	{
		bool flag = netEntity == null;
		if (flag)
		{
			Debug.LogError("WingsSync has no netEntity!");
		}
		_laserWings = GetComponent<HPEquipLaserWings>();
		bool isMine = this.isMine;
		if (isMine)
		{
			_laserWings.firedEvent.AddListener(FiredWings);
		}
	}

	public void FiredWings(bool fired)
	{
		bool isMine = this.isMine;
		if (isMine)
		{
			SendRPC("RPC_FiredWings", new object[]
			{
				fired ? 1 : 0
			});
		}
	}

	[VTRPC]
	public void RPC_FiredWings(int fired)
	{
		bool fire = fired == 1;
		bool flag = !isMine;
		if (flag)
		{
			_laserWings.Fire(fire);
		}
	}

	private HPEquipLaserWings _laserWings;
}
