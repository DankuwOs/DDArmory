using System.Collections.Generic;
using DDArmory.Weapons.EMP;
using HarmonyLib;
using UnityEngine;

namespace DDArmory.Patches;

[HarmonyPatch(typeof(JammerReceiver))]
public class Patch_JammerReciever
{
    [HarmonyPatch(nameof(JammerReceiver.GetJammerEnergy),
        [typeof(EMBands), typeof(Vector3), typeof(float), typeof(bool)])]
    public static bool GetJammerEnergy(JammerReceiver __instance, EMBands bands, Vector3 receiverDir, float fovDotPower, bool checkBurnthrough, ref float __result)
    {
        if (__instance.NeedsUpdate())
        {
            __instance.UpdateSignal();
        }
        receiverDir.Normalize();
        var energy = 0f;
        var position = __instance.actor.position;
        var radars = __instance.actor.GetRadars();
        var hasRadars = radars != null;
        foreach (var jTransmitter in __instance.incomingJammers)
        {
            if (!jTransmitter.rj || jTransmitter.mode == RadarJammer.TransmitModes.SAS)
            {
                continue;
            }
            if (checkBurnthrough && hasRadars)
            {
                var burntThrough = false;
                foreach (var r in radars)
                {
                    if (r.BurnedThroughJammer(jTransmitter.rj.jActor.actor))
                    {
                        burntThrough = true;
                        break;
                    }
                }
                if (burntThrough)
                {
                    continue;
                }
            }

            if (jTransmitter.emBand != bands) continue;
            
            var normalized = (jTransmitter.transform.position - position).normalized;
            
            if (jTransmitter.rj is EMPJammer)
            {
                var rot = Quaternion.FromToRotation(normalized, receiverDir);
                normalized = rot * normalized;
            }
            
            var dirPower = Mathf.Pow(Vector3.Dot(receiverDir, normalized), fovDotPower);
            Debug.Log($"[JammerRecieverPatch(JE)]: DirPower = {dirPower}");
            if (dirPower > 0f)
            {
                energy += jTransmitter.GetSignalStrength(position) * dirPower;
            }
        }

        __result = energy;
        return false;
    }

    [HarmonyPatch(nameof(JammerReceiver.GetJammerEnergy),
        [typeof(EMBands), typeof(Vector3), typeof(float), typeof(Vector3), typeof(bool)])]
    public static bool GetJammerEnergyWithDir(JammerReceiver __instance, EMBands bands, Vector3 receiverDir, float fovDotPower, ref Vector3 dirToSignal, bool checkBurnthrough, ref float __result)
    {
        if (__instance.NeedsUpdate())
        {
            __instance.UpdateSignal();
        }
        receiverDir.Normalize();
        var energy = 0f;
        Vector3 zero = Vector3.zero;
        var position = __instance.actor.position;
        var radars = __instance.actor.GetRadars();
        var hasRadars = radars != null;
        foreach (var jTransmitter in __instance.incomingJammers)
        {
            if (!jTransmitter.rj || jTransmitter.mode == RadarJammer.TransmitModes.SAS || !jTransmitter.isTransmitting)
            {
                continue;
            }
            if (checkBurnthrough && hasRadars)
            {
                var burntThrough = false;
                foreach (var r in radars)
                {
                    if (r.BurnedThroughJammer(jTransmitter.rj.jActor.actor))
                    {
                        burntThrough = true;
                        break;
                    }
                }
                if (burntThrough)
                {
                    continue;
                }
            }

            if (jTransmitter.emBand != bands) continue;
            
            var normalized = (jTransmitter.transform.position - position).normalized;
            
            if (jTransmitter.rj is EMPJammer)
            {
                var rot = Quaternion.FromToRotation(normalized, receiverDir);
                normalized = rot * normalized;
            }
            
            var dirPower = Mathf.Pow(Vector3.Dot(receiverDir, normalized), fovDotPower);
            Debug.Log($"[JammerRecieverPatch(JEWithDir)]: DirPower = {dirPower}");
            if (dirPower > 0f)
            {
                var signalStrength = jTransmitter.GetSignalStrength(position) * dirPower;
                zero += jTransmitter.transform.position * signalStrength;
                energy += signalStrength;
            }
        }

        __result = energy;
        return false;
    }
    
    [HarmonyPatch(nameof(JammerReceiver.GetJammerEnergyOnFrequency),
        [typeof(float), typeof(Vector3), typeof(float)])]
    public static bool GetJammerEnergyOnFrequency(JammerReceiver __instance, float frequency, Vector3 receiverDir, float fovDotPower, ref float __result)
    {
        if (__instance.NeedsUpdate())
        {
            __instance.UpdateSignal();
        }
        receiverDir.Normalize();
        var energy = 0f;
        var position = __instance.actor.position;
        var radars = __instance.actor.GetRadars();
        var hasRadars = radars != null;
        foreach (var jTransmitter in __instance.incomingJammers)
        {
            if (!jTransmitter.rj || jTransmitter.mode == RadarJammer.TransmitModes.SAS)
            {
                continue;
            }
            if (hasRadars)
            {
                var burntThrough = false;
                foreach (var r in radars)
                {
                    if (r.BurnedThroughJammer(jTransmitter.rj.jActor.actor))
                    {
                        burntThrough = true;
                        break;
                    }
                }
                if (burntThrough)
                {
                    continue;
                }
            }

            bool sameFrequency = false;
            if (jTransmitter.mode == RadarJammer.TransmitModes.NOISE)
            {
                sameFrequency = jTransmitter.emBand == Radar.GetEMBand(frequency);
            }
            else if (jTransmitter.mode == RadarJammer.TransmitModes.DRFM)
            {
                sameFrequency = Mathf.Abs(frequency - jTransmitter.drfmFrequency) < VTOLVRConstants.DRFM_FREQ_FILTER_WIDTH;
            }

            if (!sameFrequency) continue;
            
            var normalized = (jTransmitter.transform.position - position).normalized;

            if (jTransmitter.rj is EMPJammer)
            {
                var rot = Quaternion.FromToRotation(normalized, receiverDir);
                normalized = rot * normalized;
            }

            var dirPower = Mathf.Pow(Vector3.Dot(receiverDir, normalized), fovDotPower);
            Debug.Log($"[JammerRecieverPatch(JEFreq)]: DirPower = {dirPower}");
            if (dirPower > 0f)
            {
                energy += jTransmitter.GetSignalStrength(position) * dirPower;
            }
        }

        __result = energy;
        return false;
    }
    
    [HarmonyPatch(nameof(JammerReceiver.GetJammerEnergyOnFrequencyOmni),
        [typeof(float), typeof(Vector3), typeof(float), typeof(bool)])]
    public static bool GetJammerEnergyOnFrequencyOmni(JammerReceiver __instance, float frequency, Vector3 receiverDir, float fovDotPower, bool checkBurnthrough, ref float __result)
    {
        if (__instance.NeedsUpdate())
        {
            __instance.UpdateSignal();
        }
        receiverDir.Normalize();
        var energy = 0f;
        var position = __instance.actor.position;
        var radars = __instance.actor.GetRadars();
        var hasRadars = checkBurnthrough && radars != null;
        foreach (var jTransmitter in __instance.incomingJammers)
        {
            if (!jTransmitter.rj || jTransmitter.mode == RadarJammer.TransmitModes.SAS)
            {
                continue;
            }
            if (hasRadars)
            {
                var burntThrough = false;
                foreach (var r in radars)
                {
                    if (r.BurnedThroughJammer(jTransmitter.rj.jActor.actor))
                    {
                        burntThrough = true;
                        break;
                    }
                }
                if (burntThrough)
                {
                    continue;
                }
            }

            bool sameFrequency = false;
            if (jTransmitter.mode == RadarJammer.TransmitModes.NOISE)
            {
                sameFrequency = jTransmitter.emBand == Radar.GetEMBand(frequency);
            }
            else if (jTransmitter.mode == RadarJammer.TransmitModes.DRFM)
            {
                sameFrequency = Mathf.Abs(frequency - jTransmitter.drfmFrequency) < VTOLVRConstants.DRFM_FREQ_FILTER_WIDTH;
            }

            if (!sameFrequency) continue;
            
            var normalized = (jTransmitter.transform.position - position).normalized;

            if (jTransmitter.rj is EMPJammer)
            {
                var rot = Quaternion.FromToRotation(normalized, receiverDir);
                normalized = rot * normalized;
            }

            var dirPower = Mathf.Pow((Vector3.Dot(receiverDir, normalized) + 1) / 2, fovDotPower);
            Debug.Log($"[JammerRecieverPatch(JEFreqOmni)]: DirPower = {dirPower}");
            if (dirPower > 0f)
            {
                energy += jTransmitter.GetSignalStrength(position) * dirPower;
            }
        }

        __result = energy;
        return false;
    }
}