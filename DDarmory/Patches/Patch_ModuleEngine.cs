using Harmony;
using UnityEngine;
using VTOLVR.Multiplayer;

[HarmonyPatch(typeof(ModuleEngine), nameof(ModuleEngine.Update))]
public class Patch_ModuleEngine
{
    private static WeaponManager _weaponManager;

    public static void Postfix(ModuleEngine __instance)
    {
        if (__instance.isRemote)
            return;
        
        if (VTOLMPUtils.IsMultiplayer() && VTOLMPSceneManager.instance.localPlayer.vehicleObject == null)
            return;
            
        if (!_weaponManager)
            _weaponManager = VTOLAPI.GetPlayersVehicleGameObject().GetComponent<WeaponManager>();
        
        if (!_weaponManager)
            return;
        
        
        for (var i = 0; i < _weaponManager.equipCount; i++)
        {
            var equip = _weaponManager.GetEquip(i);
            if (equip is not HPEquipThrustReverser reverser) continue;

            if (!reverser.engines.Contains(__instance))
                return;

            var animTime = reverser.animationToggle.GetT();


            if (animTime <= 0.05f)
                return;

            var moduleEngineTraverse = Traverse.Create(__instance);

            moduleEngineTraverse.Property("finalThrust")
                .SetValue(__instance.finalThrust * reverser.reverserCurve.Evaluate(animTime));
        }
    }
}