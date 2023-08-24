using VTNetworking;

public class PolaroidObjectSync : VTNetSyncRPCOnly
{
    // The polaroid object needs to know if its yoinked or not for wobble and maybe another thing idk

    public PolaroidObject polaroidObject;

    protected override void OnNetInitialized()
    {
        base.OnNetInitialized();

        if (!isMine)
            return;

        polaroidObject.OnYoinked.AddListener(OnYoinked);
    }

    private void OnYoinked()
    {
        SendRPC("RPC_PolaroidYoinked");
    }

    [VTRPC]
    private void RPC_PolaroidYoinked()
    {
        if (isMine)
            return;

        polaroidObject.Yoinked();
    }
}