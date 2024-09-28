using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using VTOLVR.DLC.EW;

namespace DDArmory.Weapons.Loitering.ALTIUS.Patches;

[HarmonyPatch(typeof(MALDControlUI))]
public class Patch_MALDControlUI
{
    [HarmonyPatch(nameof(MALDControlUI.Awake))]
    [HarmonyPrefix]
    public static void Patch_Awake(MALDControlUI __instance)
    {
        var altiusMaldUI = __instance.gameObject.AddComponent<ALTIUS_MaldUI>();
        altiusMaldUI.maldControlUI = __instance;
    }
    
    [HarmonyPatch(nameof(MALDControlUI.UpdateTransmitModeText))]
    [HarmonyPrefix]
    public static bool Patch_TransitModeText(MALDControlUI __instance)
    {
        if (__instance.currentDecoy is not ALTIUSGuidance) return true;
        
        switch (__instance.selectedMode)
        {
            case AirLaunchedDecoyGuidance.TransmitModes.COLD:
                __instance.currentModeText.text = "COLD";
                return false;
            case AirLaunchedDecoyGuidance.TransmitModes.DRFM:
                __instance.currentModeText.text = "KILL";
                return false;
            case AirLaunchedDecoyGuidance.TransmitModes.NOISE:
                __instance.currentModeText.text = "INTCP";
                return false;
            case AirLaunchedDecoyGuidance.TransmitModes.DECOY:
                __instance.currentModeText.text = "DECOY " + ((__instance.selectedDecoyModel == null)
                    ? "ERROR"
                    : __instance.selectedDecoyModel.targetName);
                if (__instance.selectedDecoyModel == null)
                {
                    Debug.LogError("ERROR: selected mode is DECOY but selectedDecoyModel is null!");
                }

                return false;
            default:
                return false;
        }
    }
    
    [HarmonyPatch(nameof(MALDControlUI.UpdateDecoyStatusText))]
    [HarmonyPrefix]
    public static bool Patch_DecoyStatusText(MALDControlUI __instance)
    {
        if (__instance.currentDecoy is not ALTIUSGuidance currentDecoy) return true;
        
        if (currentDecoy.missile && currentDecoy.missile.fired && Time.time - currentDecoy.missile.timeFired > currentDecoy.missile.thrustDelay)
        {
            var missile = currentDecoy.missile;
            var terminal = currentDecoy.reachedTerminal ? "Orbit" : "Enroute";
            var mode = string.Empty;
            
            var transmitMode = currentDecoy.decoyTransmitMode.ToString();
            
            switch (currentDecoy.decoyTransmitMode)
            {
                case AirLaunchedDecoyGuidance.TransmitModes.DRFM:
                    
                    mode = currentDecoy.TargetActor ? $"{currentDecoy.TargetActor.trueIdentity.targetName}" : "Standby";
                    transmitMode = "KILL";
                    
                    break;
                case AirLaunchedDecoyGuidance.TransmitModes.NOISE:
                    
                    mode = currentDecoy.TargetActor ? $"{currentDecoy.TargetActor.trueIdentity.targetName}" : "Standby";
                    transmitMode = "INTCP";
                    
                    break;
                case AirLaunchedDecoyGuidance.TransmitModes.DECOY:
                    
                    mode = currentDecoy.decoyModel == null ? "NO MODEL" : currentDecoy.decoyModel.targetName;
                    
                    break;
            }
            
            var alt = WaterPhysics.GetAltitude(currentDecoy.transform.position);
            
            var mach = MeasurementManager.SpeedToMach(currentDecoy.missile.rb.velocity.magnitude, alt);
            mach = Mathf.Round(mach * 100f) / 100f;
            
            
            alt = __instance.measurements.ConvertedAltitude(alt);
            alt /= 1000f;
            alt = Mathf.Round(alt * 10f) / 10f;
            
            var missileTimeFired = Time.time - currentDecoy.missile.timeFired;
            var missileTotalTime = missile.thrustDelay + missile.boostTime + missile.cruiseTime;
            
            var percentLeft = Mathf.Max(0f, 1f - missileTimeFired / missileTotalTime);
            percentLeft = Mathf.Round(percentLeft * 100f);
            
            __instance.decoyStatusText.text = $"{terminal} {transmitMode} {mode}\nM{mach} A{alt} F{percentLeft}";
            
            return false;
        }
        if (currentDecoy.launched)
        {
            __instance.decoyStatusText.text = "Deploying...";
            return false;
        }
        __instance.decoyStatusText.text = "Stored";
        return false;
    }
    
    
    
    [HarmonyPatch(nameof(MALDControlUI.SetTargetTSD))]
    [HarmonyPrefix]
    public static bool Patch_SetTargetTSD(MALDControlUI __instance)
    {
        if (!__instance.currentDecoy || __instance.currentDecoy is not ALTIUSGuidance altiusDecoy) return true;

        var mode = __instance.tentativeTransmitMode;
        
        if (mode is not (AirLaunchedDecoyGuidance.TransmitModes.DRFM or AirLaunchedDecoyGuidance.TransmitModes.NOISE))
            return true;
        
        if (__instance.wm.tsc.GetCurrentSelectionActor())
        {
            var targetInfo = (TacticalSituationController.TSActorTargetInfo)(__instance.wm.tsc.ewSelectionMode
                ? __instance.wm.tsc.GetCurrentSelectionInfoEW()
                : __instance.wm.tsc.GetCurrentSelectionInfo());
            if (!altiusDecoy.killTargets.HasFlag(targetInfo.actor.finalCombatRole))
            {
                __instance.errorFlasher.DisplayError("INVALID TARGET", 2);
            }
            else
            {
                __instance.selectedMode = __instance.tentativeTransmitMode;
                __instance.selectedTSDActor = targetInfo;
                __instance.selectedTargetMode = AirLaunchedDecoyGuidance.TargetModes.TSD;
                altiusDecoy.uploadedTsdTarget = targetInfo;
                
                if (!altiusDecoy.launched)
                    altiusDecoy.targetMode = __instance.selectedTargetMode;
                
                switch (__instance.selectedMode)
                {
                    case AirLaunchedDecoyGuidance.TransmitModes.DRFM:
                    {
                        if (!altiusDecoy.launched)
                        {
                            altiusDecoy.Custom_SetJamDRFM(targetInfo.actor);

                            // Able to invoke actions from an external class MAYBE
                            var eventDelegate = Traverse.Create(__instance).Field(nameof(MALDControlUI.OnSetULDRFM))
                                .GetValue<MulticastDelegate>();
                            if (eventDelegate != null)
                            {
                                foreach (var handler in eventDelegate.GetInvocationList())
                                {
                                    try
                                    {
                                        handler.Method.Invoke(handler.Target, [__instance.currentDecoy, targetInfo.actor]);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"[Exception_MALDControlUI_DRFM]: {ex}");
                                    }
                                }
                            }

                        }

                        break;
                    }
                    /*case AirLaunchedDecoyGuidance.TransmitModes.NOISE:
                    {
                        __instance.selectedNoiseBand = __instance.tentativeNoiseBand;
                        if (!altiusDecoy.launched)
                        {
                            altiusDecoy.Custom_SetJamNoise(__instance.selectedNoiseBand, targetInfo.actor);
                        }

                        // Able to invoke actions from an external class MAYBE
                        var eventDelegate = Traverse.Create(__instance).Field(nameof(MALDControlUI.OnSetULNoise))
                            .GetValue<MulticastDelegate>();
                        if (eventDelegate != null)
                        {
                            foreach (var handler in eventDelegate.GetInvocationList())
                            {
                                try
                                {
                                    handler.Method.Invoke(handler.Target, [
                                        __instance.currentDecoy, __instance.selectedTargetMode, default(FixedPoint),
                                        targetInfo.actor, __instance.selectedNoiseBand
                                    ]);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"[Exception_MALDControlUI_NOISE]: {ex}");
                                }
                            }
                        }

                        break;
                    }*/
                }
            }
            
            __instance.ReturnToMain();
            return false;
        }

        if (__instance.wm.tsc.GetCurrentSelectionActorInfo() == null)
        {
            __instance.errorFlasher.DisplayError("NO TSD TARGET", 2);
            __instance.ReturnToMain();
            return false;
        }

        if (mode == AirLaunchedDecoyGuidance.TransmitModes.NOISE)
        {
            __instance.SetTargetGPS();
            return false;
        }
        __instance.errorFlasher.DisplayError("INVALID TARGET", 2);
        __instance.ReturnToMain();
        return false;
    }

    [HarmonyPatch(nameof(MALDControlUI.SetTargetAuto))]
    [HarmonyPrefix]
    public static void Patch_SetTargetAuto(MALDControlUI __instance)
    {
        if (!__instance.currentDecoy || __instance.currentDecoy is not ALTIUSGuidance altiusGuidance) return;
        switch (__instance.tentativeTransmitMode)
        {
            case AirLaunchedDecoyGuidance.TransmitModes.NOISE:
                    
                var altiusUI = __instance.GetComponent<ALTIUS_MaldUI>();
                if (altiusUI.MissileDetector)
                {
                    var missiles = altiusUI.MissileDetector.missiles;
                    
                    altiusGuidance.missiles = missiles;
                }
                
                var rwrs = __instance.wm.actor.rwrs;
                List<ModuleRWR.RWRContact> contacts = new List<ModuleRWR.RWRContact>();
                foreach (var rwr in rwrs)
                {
                    contacts.AddRange(rwr.contacts);
                }

                altiusGuidance.rwrContacts = contacts;
                
                altiusGuidance.Custom_SetJamNoise(__instance.tentativeNoiseBand);
                    
                break;
        }
    }

    [HarmonyPatch(nameof(MALDControlUI.DeployDecoy))]
    [HarmonyPrefix]
    public static void Patch_DeployDecoy(MALDControlUI __instance)
    {
        if (!__instance.currentDecoy || __instance.currentDecoy is not ALTIUSGuidance altiusGuidance || __instance.selectedTargetMode == AirLaunchedDecoyGuidance.TargetModes.TSD) return;
        switch (__instance.selectedMode)
        {
            case AirLaunchedDecoyGuidance.TransmitModes.NOISE:
                
                var altiusUI = __instance.GetComponent<ALTIUS_MaldUI>();
                if (altiusUI.MissileDetector)
                {
                    var missiles = altiusUI.MissileDetector.missiles;
                    altiusGuidance.missiles = missiles;
                }
                
                var rwrs = __instance.wm.actor.rwrs;
                List<ModuleRWR.RWRContact> contacts = new List<ModuleRWR.RWRContact>();
                foreach (var rwr in rwrs)
                {
                    contacts.AddRange(rwr.contacts);
                }

                altiusGuidance.rwrContacts = contacts;
                
                // Possible fix issues where it just doesnt go for the obviously there missile
                altiusGuidance.Custom_SetJamNoise(__instance.tentativeNoiseBand);
                
                break;
        }
    }
    
    [HarmonyPatch(nameof(MALDControlUI.SendUpdate))]
    [HarmonyPostfix]
    public static void Patch_SendUpdate(MALDControlUI __instance)
    {
        if (!__instance.currentDecoy || __instance.currentDecoy is not ALTIUSGuidance altiusGuidance) return;
        switch (__instance.selectedMode)
        {
            case AirLaunchedDecoyGuidance.TransmitModes.DRFM:
                    
                altiusGuidance.Custom_SetJamDRFM(__instance.selectedTargetMode == AirLaunchedDecoyGuidance.TargetModes.AUTO ? null : __instance.selectedTSDActor.actor);
                    
                break;
            case AirLaunchedDecoyGuidance.TransmitModes.NOISE:
                    
                var altiusUI = __instance.GetComponent<ALTIUS_MaldUI>();
                if (altiusUI.MissileDetector)
                {
                    var missiles = altiusUI.MissileDetector.missiles;
                    altiusGuidance.missiles = missiles;
                }
                
                var rwrs = __instance.wm.actor.rwrs;
                List<ModuleRWR.RWRContact> contacts = new List<ModuleRWR.RWRContact>();
                foreach (var rwr in rwrs)
                {
                    contacts.AddRange(rwr.contacts);
                }
                altiusGuidance.rwrContacts = contacts;
                
                altiusGuidance.Custom_SetJamNoise(__instance.tentativeNoiseBand);
                    
                break;
        }
    }

    [HarmonyPatch(nameof(MALDControlUI.SetMode), [typeof(AirLaunchedDecoyGuidance.TransmitModes)])]
    [HarmonyPostfix]
    public static void Patch_SetMode(MALDControlUI __instance, AirLaunchedDecoyGuidance.TransmitModes mode)
    {
        if (mode == AirLaunchedDecoyGuidance.TransmitModes.NOISE)
        {
            __instance.tentativeNoiseBand = EMBands.Mid;
            
            __instance.SetTargetAuto();
            /*__instance.UpdateTransmitModeText();
            __instance.UpdateTargetModeText();*/
            
            __instance.ReturnToMain();
            /*__instance.tsdTargetButton.SetActive(false);
            __instance.tgpTargetButton.SetActive(false);
            __instance.gpsTargetButton.SetActive(false);*/
        }
    }
}