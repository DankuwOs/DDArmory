using HarmonyLib;

namespace DDArmory.Weapons.Utils.Patches;

[HarmonyPatch(typeof(InternalWeaponBay))]
public class Patch_IWB
{
    [HarmonyPatch(nameof(InternalWeaponBay.Open))]
    [HarmonyPrefix]
    public static bool Prefix_Open(InternalWeaponBay __instance)
    {
        if (__instance is not InternalWeaponBayHP)
            return true;
        
        if (!__instance.opening)
            __instance.opening = true;
        
        if (__instance.rotationToggle)
            __instance.rotationToggle.SetDeployed();
        
        if (__instance.animationToggle)
            __instance.animationToggle.Deploy();
        
        return false;
    }
    
    [HarmonyPatch(nameof(InternalWeaponBay.Close))]
    [HarmonyPrefix]
    public static bool Prefix_Close(InternalWeaponBay __instance)
    {
        if (__instance is not InternalWeaponBayHP)
            return true;
        
        if (__instance.opening)
            __instance.opening = false;
        
        if (__instance.rotationToggle)
            __instance.rotationToggle.SetDefault();
        
        if (__instance.animationToggle)
            __instance.animationToggle.Retract();
        
        return false;
    }
    
    
    [HarmonyPatch(nameof(InternalWeaponBay.Hide))]
    [HarmonyPrefix]
    public static bool Prefix_Hide(InternalWeaponBay __instance)
    {
        return __instance is not InternalWeaponBayHP;
    }
    
    [HarmonyPatch(nameof(InternalWeaponBay.Show))]
    [HarmonyPrefix]
    public static bool Prefix_Show(InternalWeaponBay __instance)
    {
        return __instance is not InternalWeaponBayHP;
    }
}