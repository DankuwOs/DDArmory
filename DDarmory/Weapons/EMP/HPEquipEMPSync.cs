using System.Linq;
using UnityEngine;
using VTNetworking;
using VTOLVR.Multiplayer;

namespace DDArmory.Weapons.EMP;

public class HPEquipEMPSync : VTNetSyncRPCOnly
{
    public HPEquipEMP emp;
    
    public override void OnNetInitialized()
    {
        base.OnNetInitialized();
        
        if (base.isMine)
        {
            emp.OnEMP.AddListener(OnEMP);
            emp.OnEMPEntity.AddListener(OnEMPEntity);
            emp.OnEMPPlayer.AddListener(OnEMPPLayer);
            emp.windingWeapon.OnStartWind.AddListener(OnEMPStartWind);
            emp.windingWeapon.OnStopWind.AddListener(OnEMPStopWind);
        }
    }

    private void OnEMPStartWind()
    {
        SendRPC("RPC_OnEMPStartWind");
    }
    private void OnEMPStopWind()
    {
        SendRPC("RPC_OnEMPStopWind");
    }

    private void OnEMP()
    {
        SendRPC("RPC_OnEMP");
    }

    private void OnEMPEntity(int netEntityID)
    {
        Debug.Log($"[EMP]: Sending RPC for {netEntityID}");
        SendRPC("RPC_OnEMPEntity", netEntityID);
    }
    
    private void OnEMPPLayer(ulong playerID)
    {
        SendDirectedRPC(playerID, "RPC_OnEMPPlayer");
    }
    
    [VTRPC]
    private void RPC_OnEMPStartWind()
    {
        emp.windingWeapon.StartWinding();
    }
    
    [VTRPC]
    private void RPC_OnEMPStopWind()
    {
        emp.windingWeapon.StopWinding();
    }
    
    [VTRPC]
    private void RPC_OnEMP()
    {
        emp.OnFire();
        emp.windingWeapon.StopWindingImmediate();
    }

    [VTRPC]
    private void RPC_OnEMPEntity(int netEntityID)
    {
        var entity = VTNetSceneManager.instance.sceneEntities.FirstOrDefault(e => e.entityID == netEntityID);
        
        Debug.Log($"[EMPRPC]: Recieved RPC for '{netEntityID}' and I am questioning if I am found? {entity != null}");
        
        if (entity && entity.GetComponent<Actor>() is var actor)
        {
            emp.RemoteEMPActor(actor, transform.position);
        }
    }
    
    [VTRPC]
    private void RPC_OnEMPPlayer()
    {
        emp.PlayEMPAudio(VTOLMPSceneManager.instance.localPlayer.vehicleObject.transform);
    }
}