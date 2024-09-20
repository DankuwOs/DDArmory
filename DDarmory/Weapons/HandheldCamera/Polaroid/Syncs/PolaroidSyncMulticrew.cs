using DDArmory.Weapons.HandheldCamera.Syncs;
using UnityEngine;
using VTNetworking;

public class PolaroidSyncMulticrew : CameraSyncMulticrew
{
    public Texture remoteTexture;

    public override void OnNetInitialized()
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
            obj.SetActive(false);
        }

        polaroid.remote = !isMine;
    }

    [VTRPC]
    public void RPC_SendPolaroid(int id)
    {
        if (isMine)
            return;
        var entity = VTNetworkManager.instance.GetEntity(id);
        if (!entity)
        {
            Debug.Log($"[PolaroidSync.SendPolaroid]: Entity null!");
        }

        var polaroidObj = entity.gameObject;

        var polaroidTransform = polaroidObj.transform;

        polaroidTransform.parent = ((HPEquipPolaroid)camera).polaroidParent;
        polaroidTransform.localPosition = Vector3.zero;
        polaroidTransform.localRotation = Quaternion.identity;

        polaroidObj.SetActive(true);

        var polaroid = polaroidObj.GetComponentInChildren<PolaroidObject>();

        // Create and set the polaroids RT.
        polaroid.polaroidMaterial = polaroid.renderer.materials[polaroid.materialIndex];
        
        polaroid.polaroidMaterial.mainTexture = remoteTexture;

        // Set rootTf for the polaroid to set to after yoinked and then do the polaroid nyoom out
        polaroid.rootTf = transform;

        polaroid.translationToggle.SetDeployed();
    }
    

    [VTRPC]
    public override void RPC_CameraOnCapture()
    {
        if (!isMine)
        {
            Debug.Log($"Running polaroid RPC for other player");
            camera.OnCapture.Invoke();
        }
    }
}