using UnityEngine;
using VTNetworking;

namespace DDArmory.Weapons.SatelliteGun;

public class SatelliteGunSync : VTNetSyncRPCOnly
{
    public HPEquipSatelliteGun satelliteGun;
    public CWB_WindingWeapon windingWeapon;

    public CWB_GrabInteractable grabInteractable;
    
    public override void OnNetInitialized()
    {
        base.OnNetInitialized();
        
        if (!isMine)
        {
            satelliteGun.remote = true;
            return;
        }

        windingWeapon.OnStartWind.AddListener(OnStartWind);
        windingWeapon.OnStopWind.AddListener(OnStopWind);
        windingWeapon.OnWind.AddListener(OnWind);
        
        if (satelliteGun.handheld)
        {
            satelliteGun.OnUpdateLaser += OnUpdatelaser;
            grabInteractable.OnThumbButtonPressed.AddListener(OnThumbButtonPressed);
            grabInteractable.OnThumbButtonReleased.AddListener(OnThumbButtonReleased);
        }
    }

    private void OnStartWind()
    {
        SendRPC("RPC_OnStartWind");
    }

    private void OnStopWind()
    {
        SendRPC("RPC_OnStopWind");
    }

    private void OnWind(float t)
    {
        SendRPC("RPC_OnWind", t);
    }

    private void OnUpdatelaser(HPEquipSatelliteGun.LaserObjectParams obj)
    {
        SendRPC("RPC_OnSyncLaser", obj.laserEnd, obj.laserLightPos);
    }

    private void OnThumbButtonPressed(VRHandController _)
    {
        SendRPC("RPC_OnStartWind");
    }

    private void OnThumbButtonReleased(VRHandController _)
    {
        SendRPC("RPC_OnStopWind");
    }

    [VTRPC]
    private void RPC_OnStartWind()
    {
        windingWeapon.StartWinding();
    }

    [VTRPC]
    private void RPC_OnStopWind()
    {
        windingWeapon.StopWinding();
    }

    [VTRPC]
    private void RPC_OnWind(float t)
    {
        satelliteGun.OnWind(t);
    }
    
    [VTRPC]
    private void RPC_OnSyncLaser(Vector3D endPos, Vector3D lightPos)
    {
        HPEquipSatelliteGun.LaserObjectParams laserObjectParams = new HPEquipSatelliteGun.LaserObjectParams()
        {
            laserEnd = endPos,
            laserLightPos = lightPos
        };
        satelliteGun.RemoteUpdatelaser(laserObjectParams);
    }
}