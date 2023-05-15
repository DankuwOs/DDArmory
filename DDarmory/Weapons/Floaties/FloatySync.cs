using UnityEngine;
using VTNetworking;

public class FloatySync : VTNetSyncRPCOnly
{
    protected override void OnNetInitialized()
    {
        if (netEntity == null) Debug.LogError("Floaty has no netEntity!");
        _floaty = GetComponent<HPEquipFloaty>();
        if (isMine)
        {
            _floaty.OnDeploy.AddListener(Deployed);
            _floaty.OnRetract.AddListener(Retracted);
        }
    }

    public void Deployed()
    {
        if (isMine) SendRPC("RPC_DeployFloaty", 1);
    }

    public void Retracted()
    {
        if (isMine) SendRPC("RPC_DeployFloaty", 0);
    }

    [VTRPC]
    public void RPC_DeployFloaty(int deploy)
    {
        if (!isMine)
        {
            if (deploy == 1)
                _floaty.Deploy();
            else
                _floaty.Retract();
        }
    }

    private HPEquipFloaty _floaty;
}