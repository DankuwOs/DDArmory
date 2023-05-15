using UnityEngine;
using VTNetworking;

public class CameraSync : VTNetSyncRPCOnly
{
    public HPEquipHandheldCamera camera;
    
    protected override void OnNetInitialized()
    {
        base.OnNetInitialized();

        if (isMine)
        {
            if (!camera)
                camera = GetComponent<HPEquipHandheldCamera>();
            camera.OnCapture.AddListener(OnCapture);
            camera.OnFlashEnabled.AddListener(OnFlashEnabled);
            camera.OnFlashDisabled.AddListener(OnFlashDisabled);
        }
    }

    public void OnCapture()
    {
        if (isMine)
        {
            SendRPC("RPC_CameraOnCapture");
        }
    }
    
    public void OnFlashEnabled()
    {
        if (isMine)
        {
            SendRPC("RPC_CameraOnFlashEnabled");
        }
    }
    
    public void OnFlashDisabled()
    {
        if (isMine)
        {
            SendRPC("RPC_CameraOnFlashDisabled");
        }
    }

    [VTRPC]
    public virtual void RPC_CameraOnCapture()
    {
        if (!isMine)
        {
            camera.OnCapture.Invoke();
        }
    }
    
    [VTRPC]
    public void RPC_CameraOnFlashEnabled()
    {
        if (!isMine)
        {
            camera.OnFlashEnabled.Invoke();
        }
    }
    
    [VTRPC]
    public void RPC_CameraOnFlashDisabled()
    {
        if (!isMine)
        {
            camera.OnFlashDisabled.Invoke();
        }
    }
}