using UnityEngine;
using VTNetworking;

// Token: 0x02000021 RID: 33
public class SoakieSystemSync : VTNetSyncRPCOnly
{
	// Token: 0x060000A3 RID: 163 RVA: 0x00005998 File Offset: 0x00003B98
	protected override void OnNetInitialized()
	{
		bool flag = netEntity == null;
		if (flag)
		{
			Debug.LogError("Soakie has no netEntity!");
		}
		_soakie = GetComponent<HPEquipSoakieSystem>();
		_soakieMulticrew = GetComponent<HPEquipSoakieSystemMulticrew>();
		bool isMine = this.isMine;
		if (isMine)
		{
			bool flag2 = _soakie;
			if (flag2)
			{
				_soakie._brightnessEvent.AddListener(SetBrightness);
			}
			bool flag3 = _soakieMulticrew;
			if (flag3)
			{
				_soakieMulticrew._colorBrightnessEvent.AddListener(SetColorBrightness);
				_soakieMulticrew._depthBrightnessEvent.AddListener(SetDepthBrightness);
			}
		}
	}

	// Token: 0x060000A4 RID: 164 RVA: 0x00005A5E File Offset: 0x00003C5E
	public void SetBrightness(float brightness)
	{
		SendRPC("RPC_D_BrightnessEvent", brightness);
	}

	// Token: 0x060000A5 RID: 165 RVA: 0x00005A73 File Offset: 0x00003C73
	public void SetColorBrightness(float brightness)
	{
		SendRPC("RPC_D_CBrightnessEvent", brightness);
	}

	// Token: 0x060000A6 RID: 166 RVA: 0x00005A88 File Offset: 0x00003C88
	public void SetDepthBrightness(float brightness)
	{
		SendRPC("RPC_D_DBrightnessEvent", brightness);
	}

	// Token: 0x060000A7 RID: 167 RVA: 0x00005A9D File Offset: 0x00003C9D
	[VTRPC]
	public void RPC_D_BrightnessEvent(float brightness)
	{
		_soakie.SetBrightness(brightness);
	}

	// Token: 0x060000A8 RID: 168 RVA: 0x00005AAD File Offset: 0x00003CAD
	[VTRPC]
	public void RPC_D_CBrightnessEvent(float brightness)
	{
		_soakieMulticrew.SetColorBrightness(brightness);
	}

	// Token: 0x060000A9 RID: 169 RVA: 0x00005ABD File Offset: 0x00003CBD
	[VTRPC]
	public void RPC_D_DBrightnessEvent(float brightness)
	{
		_soakieMulticrew.SetDepthBrightness(brightness);
	}

	// Token: 0x040000D4 RID: 212
	[Header("One or the other")]
	private HPEquipSoakieSystem _soakie;

	// Token: 0x040000D5 RID: 213
	private HPEquipSoakieSystemMulticrew _soakieMulticrew;
}
