using UnityEngine;
using VTNetworking;

	public class SoakieSystemSync : VTNetSyncRPCOnly
	{
		private HPEquipSoakieSystem _soakie;

		private HPEquipSoakieSystemMulticrew _soakieMulticrew;

		public override void OnNetInitialized()
		{
			if (netEntity == null)
			{
				Debug.LogError("Soakie has no netEntity!");
			}
		
			_soakie = GetComponent<HPEquipSoakieSystem>();
			_soakieMulticrew = GetComponent<HPEquipSoakieSystemMulticrew>();
			var wm = _soakie ? _soakie.weaponManager : _soakieMulticrew.weaponManager;
			if (isMine)
			{
				if (_soakie)
				{
					_soakie._brightnessEvent.AddListener(SetBrightness);
				}
				if (_soakieMulticrew)
				{
					_soakieMulticrew._colorBrightnessEvent.AddListener(SetColorBrightness);
					_soakieMulticrew._depthBrightnessEvent.AddListener(SetDepthBrightness);
				}
			}
		}

		public void SetBrightness(float brightness)
		{
			SendRPC("RPC_D_BrightnessEvent", brightness);
		}

		public void SetColorBrightness(float brightness)
		{
			SendRPC("RPC_D_CBrightnessEvent", brightness);
		}

		public void SetDepthBrightness(float brightness)
		{
			SendRPC("RPC_D_DBrightnessEvent", brightness);
		}

		[VTRPC]
		public void RPC_D_BrightnessEvent(float brightness)
		{
			_soakie.SetBrightness(brightness);
		}

		[VTRPC]
		public void RPC_D_CBrightnessEvent(float brightness)
		{
			_soakieMulticrew.SetColorBrightness(brightness);
		}

		[VTRPC]
		public void RPC_D_DBrightnessEvent(float brightness)
		{
			_soakieMulticrew.SetDepthBrightness(brightness);
		}

	
	}