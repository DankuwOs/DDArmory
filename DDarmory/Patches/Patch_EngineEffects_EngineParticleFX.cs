using System.Runtime.CompilerServices;
using HarmonyLib;
using UnityEngine;

namespace DDArmory.Patches;

[HarmonyPatch(typeof(EngineEffects.EngineParticleFX), nameof(EngineEffects.EngineParticleFX.Evaluate))]
public class Patch_EngineEffects_EngineParticleFX
{
    public static bool Prefix(EngineEffects.EngineParticleFX __instance, float throttle)
    {
        // Having a reference to either ps.emission or ps.main (don't remember which) gives you an error except for baha for some reason, so im just gonna patch it out.
        var particleSystemEmission = __instance.particleSystem.emission;
        particleSystemEmission.rateOverTime =
            new ParticleSystem.MinMaxCurve(__instance.emissionCurve.Evaluate(throttle));

        var particleSystemMain = __instance.particleSystem.main;
        particleSystemMain.startSpeed = __instance.speedCurve.Evaluate(throttle);
        particleSystemMain.startSize = __instance.sizeCurve.Evaluate(throttle);
        if (__instance.useColorGradient)
        {
            particleSystemMain.startColor = __instance.colorGradient.Evaluate(throttle);
        }

        return false;
    }
}