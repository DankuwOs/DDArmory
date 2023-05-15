using UnityEngine;
using VTNetworking;

public class PolaroidSync : CameraSync
{
    protected override void OnNetInitialized()
    {
        base.OnNetInitialized();

        if (camera is not HPEquipPolaroid polaroid)
        {
            Debug.Log($"[Polaroid Sync]: Camera is not HPEquipPolaroid?");
            return;
        }
        
        if (polaroid.polaroidObject)
        {
            Debug.Log($"[PolaroidSync]: Registering polaroid object");
            var obj = Instantiate(polaroid.polaroidObject);
            DontDestroyOnLoad(obj);
            VTNetworkManager.RegisterOverrideResource("DDA/PolaroidObj", obj);
        }

        polaroid.remote = !isMine;
    }

    [VTRPC]
    public override void RPC_CameraOnCapture()
    {
        Debug.Log($"Running polaroid RPC");
        if (!isMine)
        {
            Debug.Log($"Sending polaroid object");
            StartCoroutine(((HPEquipPolaroid)camera).SendPolaroid());
            camera.OnCapture.Invoke();
        }
    }
}