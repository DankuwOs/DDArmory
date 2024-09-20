using System;
using System.Collections;
using VTOLVR.Multiplayer;

namespace DDArmory.Weapons.HandheldCamera.Syncs;

public class CameraSyncMulticrew : CameraSync
{
    protected MultiUserVehicleSync muvs;

    private void OnEnable()
    {
        if (camera == null)
            camera = GetComponent<HPEquipHandheldCamera>();
        if (camera != null)
        {
            muvs = camera.weaponManager.muvs;
        }
        
        camera.OnCapture.AddListener(OnCapture_MUVS);
    }

    private void OnCapture_MUVS()
    {
        muvs?.SendRPCToCopilots(this, "RPC_OnCapture_MUVS");
    }

    [VTRPC]
    private void RPC_OnCapture_MUVS()
    {
        camera.RemoteCapture();
    }
}