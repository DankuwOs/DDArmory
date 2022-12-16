using Harmony;

[HarmonyPatch(typeof(WingVaporParticles), nameof(WingVaporParticles.UpdateEffect))]
public class Patch_WingVaporParticles
{
    public static bool Prefix(WingVaporParticles __instance)
    {
        return __instance.flightInfo; // Fix nullref for shwing
    }
}