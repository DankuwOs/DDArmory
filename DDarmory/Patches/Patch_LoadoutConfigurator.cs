using Harmony;

[HarmonyPatch(typeof(LoadoutConfigurator), nameof(LoadoutConfigurator.CheckBusyReloadingAll))]
public class Patch_LoadoutConfigurator_1
{
    public static bool Prefix(ref bool __result)
    {
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(LoadoutConfigurator), nameof(LoadoutConfigurator.EndActiveRearmingPoint))]
public class Patch_LoadoutConfigurator_2
{
    public static void Prefix(ref bool ___reloadingAll)
    {
        ___reloadingAll = false;
    }
}