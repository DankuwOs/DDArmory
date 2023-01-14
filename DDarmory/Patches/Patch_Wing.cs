using Harmony;

[HarmonyPriority(Priority.First)]
[HarmonyPatch(typeof(Wing), nameof(Wing.BPU_FixedUpdate))]
public class Patch_Wing
{
    public static bool Prefix(Wing __instance)
    {
        return __instance.rb != null; // removing nullref, the one in start can stay cause i dont care
    }
}