using VTNetworking;
using VTOLVR.Multiplayer;

namespace DDArmory.Weapons.Utils;

public class TransformMoverSync : VTNetSyncRPCOnly
{
    public TransformMover transformMover;

    public override void OnNetInitialized()
    {
        base.OnNetInitialized();

        if (isMine && transformMover)
            transformMover.IntEvent += TransformMoverOnIntEvent;
    }

    private void TransformMoverOnIntEvent(int slot)
    {
        SendRPC("RPC_SwapObject", slot);
    }

    [VTRPC]
    public void RPC_SwapObject(int slot)
    {
        transformMover.SwapObject(slot);
    }
}