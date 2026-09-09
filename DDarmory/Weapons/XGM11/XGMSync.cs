using VTNetworking;

namespace DDArmory.Weapons.XGM11;

public class XGMSync : VTNetSyncRPCOnly
{
    public XGMGuidance xgmGuidance;
    
    public MissileFairing[] fairings;
    
    public override void OnNetInitialized()
    {
        base.OnNetInitialized();

        if (isMine)
        {
            xgmGuidance.OnJettisonFairings.AddListener(OnJettisonFairings);
        }
    }

    public void OnJettisonFairings()
    {
        SendRPC("RPC_OnJettisonFairings");
    }

    public void RPC_OnJettisonFairings()
    {
        foreach (var missileFairing in fairings)
        {
            missileFairing.Jettison();
        }
    }
}