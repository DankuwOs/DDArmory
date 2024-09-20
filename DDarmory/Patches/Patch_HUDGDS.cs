using DDArmory.Weapons.Utils;
using HarmonyLib;
using UnityEngine;

namespace DDArmory.Patches;

[HarmonyPatch(typeof(HUDGunDirectorSight), nameof(HUDGunDirectorSight.LateUpdate))]
public class Patch_HUDGDS
{
    public static void Prefix(HUDGunDirectorSight __instance)
    {
        if (__instance.weapon is not IGunAimpointOnly) return;
        
        __instance.targetLocked = false; // Skips all of the code that makes the pipper hud thing do the thing idk.
        Debug.Log($"[DDA]: gun gun is aimpoint only, setting target locked to false yippee");
    }
    
    public static void Postfix(HUDGunDirectorSight __instance)
    {
        if (__instance.weapon is not IGunAimpointOnly) return;
        
        __instance.fixedSightTransform.gameObject.SetActive(true);
        __instance.SetHUDPosition(__instance.fixedSightTransform, __instance.fireTransform.position + __instance.fireTransform.forward * 4000f);
    }
}