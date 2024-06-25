using DDArmory.Weapons.EMP;
using HarmonyLib;
using UnityEngine;

namespace DDArmory.Patches;

[HarmonyPatch(typeof(RadarJammer.JTransmitter))]
public class Patch_RadarJammer_JTransmitter
{
    [HarmonyPatch(nameof(RadarJammer.JTransmitter.hasTarget), MethodType.Getter)]
    [HarmonyPostfix]
    public static void HasTargetPostfix(RadarJammer.JTransmitter __instance, ref bool __result)
    {
        if (__instance.rj is EMPJammer)
            __result = true;
    }

    [HarmonyPatch(nameof(RadarJammer.JTransmitter.GetSignalStrength),
        argumentTypes: [typeof(Vector3), typeof(bool), typeof(bool)])]
    [HarmonyPrefix]
    public static void GetSignalStrengthPrefix(RadarJammer.JTransmitter __instance, ref Vector3 receiverPos)
    {
        if (__instance.rj is EMPJammer)
        {
            var rot = Quaternion.FromToRotation(receiverPos - __instance.transform.position, __instance.transmitDirection);

            var fixedRecPos = __instance.transform.position + rot * (receiverPos - __instance.transform.position);
            
            receiverPos = fixedRecPos; // This ensures the EMP will hit everything i think
            
            Debug.Log($"[Patched jammer]: Dot of thing is {Vector3.Dot(receiverPos, __instance.transmitDirection)}");
        }
    }
}