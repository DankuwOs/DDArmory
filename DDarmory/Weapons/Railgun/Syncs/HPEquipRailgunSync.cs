using VTNetworking;

namespace DDArmory.Weapons.Railgun.Syncs;

public class HPEquipRailgunSync : VTNetSyncRPCOnly
{
    public HPEquipRailgun railgun;
    
    public override void OnNetInitialized()
    {
        base.OnNetInitialized();

        if (isMine)
        {
            railgun.OnWindEvent.AddListener(OnWind);
            railgun.OnStopWindEvent.AddListener(OnStopWind);
            railgun.OnFireEvent.AddListener(OnFire);
        }
    }

    private void OnWind()
    {
        SendRPC(nameof(RPC_OnWind));
    }

    [VTRPC]
    private void RPC_OnWind()
    {
        if (isMine)
            return;
        
        railgun.windingWeapon.StartWinding();
    }
    
    private void OnStopWind()
    {
        SendRPC(nameof(RPC_OnStopWind));
    }

    [VTRPC]
    private void RPC_OnStopWind()
    {
        if (isMine)
            return;
        
        railgun.windingWeapon.StopWinding();
    }
    
    private void OnFire()
    {
        SendRPC(nameof(RPC_OnFire));
    }

    [VTRPC]
    private void RPC_OnFire()
    {
        if (isMine)
            return;
        
        railgun.UpdateHeatsink(railgun.cooldownTime);
        railgun.windingWeapon.StopWindingImmediate();
    }
}