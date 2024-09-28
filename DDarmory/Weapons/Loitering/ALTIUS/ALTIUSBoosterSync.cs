using System.Collections.Generic;
using VTNetworking;

namespace DDArmory.Weapons.Loitering.ALTIUS;

public class ALTIUSBoosterSync : VTNetSyncRPCOnly
{
    public ALTIUSGuidance altiusGuidance;
    
    public List<SolidBooster> boosters;

    public override void Awake()
    {
        base.Awake();
        if (isMine)
            altiusGuidance.OnKILLTIME.AddListener(OnKILLTIME);
    }

    private void OnKILLTIME()
    {
        SendRPC("RPC_OnKILLTIME");
    }

    [VTRPC]
    private void RPC_OnKILLTIME()
    {
        foreach (var solidBooster in boosters)
        {
            if (!solidBooster.fired)
                solidBooster.Fire();
        }
    }
}