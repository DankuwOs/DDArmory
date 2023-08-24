using System;
using System.Linq;
using DDArmory.Weapons.Utils;
using UnityEngine;
using VTOLVR.Multiplayer;


public class WingController : MonoBehaviour
{
    [Header("Wings")]
    [Tooltip("Path to a VRLever separated by a '/'")]
    public string navSwitchPath;

    public string wingFoldPath;

    public RotationToggle wingFoldToggle;

    [Header("Ext. Lights")] 
    public StrobeLightController.StrobeLight[] strobeLights;

    public ObjectPowerUnit[] navLightPowerUnits;

    public FormationGlowController formationGlowController;

    [Header("Flight Assist")]
    [Header("Default is based on the F/A-26B PIDs")]
    public bool overrideFlightAssist;

    public float pitchAngVel = 0.75f;

    public float yawAngVel = 0.75f;
    
    public float rollAngVel = 5;
    
    [Tooltip("Get these from the F/A-26B FlightAssist, I cannot default these!")]
    public AnimationCurve pitchInputCurve;
    
    [Tooltip("Get these from the F/A-26B FlightAssist, I cannot default these!")]
    public AnimationCurve yawInputCurve;
    
    [Tooltip("Get these from the F/A-26B FlightAssist, I cannot default these!")]
    public AnimationCurve rollInputCurve;




    public PID pitchPID = new PID(1.5f, 3f, 0f, -0.06f, 0.06f);

    public PID yawPID = new PID(1f, 0f, -0.05f, -0.5f, 0.5f);

    public PID rollPID = new PID(0.75f, 0f, 0f, -2f, 2f);


    public PIDFixer pidFixer;


    [NonSerialized] 
    public FlightInfo flightInfo = null;

    [NonSerialized] 
    public Battery battery;


    private float _vanillaPitchAngVel;

    private float _vanillaYawAngVel;

    private float _vanillaRollAngVel;


    private AnimationCurve _vanillaPitchInputCurve;

    private AnimationCurve _vanillaYawInputCurve;

    private AnimationCurve _vanillaRollInputCurve;
    

    private PID _vanillaPitchPID;

    private PID _vanillaYawPID;

    private PID _vanillaRollPID;


    public virtual void SetupLights(bool detach = false)
    {
        Transform startTf = null;
        if (flightInfo)
        {
            startTf = flightInfo.transform;
        }
        else
        {
            Debug.Log($"[Wing Controller (SL)]: No flight info. :~(");
            return;
        }
        
        
        if (!battery)
        {
            Debug.Log($"[Wing Controller (SL)]: Battery gone!");
            return;
        }

        var navSwitch = FindTransform.FindTranny(startTf, navSwitchPath)?.GetComponent<VRLever>();
        var strobeController = startTf.GetComponentInChildren<StrobeLightController>();

        if (VTOLMPUtils.IsMultiplayer())
        {
            var extLightSync = startTf.GetComponentInChildren<ExteriorLightSync>();
            
            if (extLightSync && formationGlowController)
                extLightSync.formationLights = formationGlowController;
        }

        if (navSwitch)
        {
            var navLightController = startTf.GetComponentInChildren<NavLightController>();
            
            // Assigning battery to fix null
            foreach (var navLightPowerUnit in navLightPowerUnits)
            {
                navLightPowerUnit.battery = battery;
            }

            foreach (var formationLight in formationGlowController.lights)
            {
                formationLight.battery = battery;
            }

            // Adding units to list
            if (navLightController)
            {
                var powerUnits = navLightController.powerUnits.ToList(); // Maybe need?
                powerUnits.AddRange(navLightPowerUnits);
                navLightController.powerUnits = powerUnits.ToArray();
            }

            // Toggling formation with switch.
            navSwitch.OnSetState.AddListener(delegate(int state)
            {
                formationGlowController.SetStatus(state);
            });
        }
        
        if (strobeController)
        {
            strobeController.lights.AddRange(strobeLights);
        }
    }

    public virtual void SetupWingFold()
    {
        Transform startTf = null;
        if (flightInfo)
        {
            startTf = flightInfo.transform;
        }
        else
        {
            Debug.Log($"[Wing Controller (WF)]: flightInfo null!");
            return;
        }

        var wfSwitch = FindTransform.FindTranny(startTf, wingFoldPath)?.GetComponent<VRLever>();

        if (!wfSwitch)
        {
            Debug.Log($"Can't find wing fold switch");
            var wfSwitchv2 = RecursiveFindChild(startTf, "WingSwitchInteractable");
            if (wfSwitchv2)
                Debug.Log($"Though there is another wfSwitch at {GetTfPath(wfSwitchv2)}");
            return;
        }
        
        if (battery)
            wingFoldToggle.battery = battery;
        
        wfSwitch.OnSetState.AddListener(delegate(int state)
        {
            wingFoldToggle.SetState(state);
        });
    }
    
    public static Transform RecursiveFindChild(Transform parent, string childName)
    {
        Transform result = null;

        foreach (Transform child in parent)
        {
            if (child.name == childName)
                result = child.transform;
            else
                result = RecursiveFindChild(child, childName);

            if (result != null) break;
        }

        return result;
    }

    public static string GetTfPath(Transform child)
    {
        var str = child.gameObject.name;
        Transform tf = child;
        while (tf != null)
        {
            tf = tf.parent;
            str.Insert(0, $"{tf.gameObject.name}/");
        }

        return str;
    }

    public virtual void SetupPIDs(FlightAssist assist)
    {
        if (!overrideFlightAssist || !assist)
            return;

        // Assigning the old values to return to
        _vanillaPitchAngVel = assist.pitchAngVel;
        _vanillaYawAngVel = assist.yawAngVel;
        _vanillaRollAngVel = assist.rollAngVel;

        _vanillaPitchInputCurve = assist.pitchInputCurve;
        _vanillaYawInputCurve = assist.yawInputCurve;
        _vanillaRollInputCurve = assist.rollInputCurve;

        _vanillaPitchPID = assist.pitchPID;
        _vanillaYawPID = assist.yawPID;
        _vanillaRollPID = assist.rollPID;
        
        // Assigning new values
        assist.pitchAngVel = pitchAngVel;
        assist.yawAngVel = yawAngVel;
        assist.rollAngVel = rollAngVel;

        assist.pitchInputCurve = pitchInputCurve;
        assist.yawInputCurve = yawInputCurve;
        assist.rollInputCurve = rollInputCurve;

        assist.pitchPID = pitchPID;
        assist.yawPID = yawPID;
        assist.rollPID = rollPID;

        if (pidFixer)
            pidFixer.assist = assist;
    }

    public virtual void ResetPIDs(FlightAssist assist)
    {
        if (!overrideFlightAssist || !assist)
            return;
        
        
        assist.pitchAngVel = _vanillaPitchAngVel;
        assist.yawAngVel = _vanillaYawAngVel;
        assist.rollAngVel = _vanillaRollAngVel;

        assist.pitchInputCurve = _vanillaPitchInputCurve;
        assist.yawInputCurve = _vanillaYawInputCurve;
        assist.rollInputCurve = _vanillaRollInputCurve;

        assist.pitchPID = _vanillaPitchPID;
        assist.yawPID = _vanillaYawPID;
        assist.rollPID = _vanillaRollPID;
    }
}