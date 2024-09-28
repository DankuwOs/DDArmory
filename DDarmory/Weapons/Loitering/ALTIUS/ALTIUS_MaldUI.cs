using System;
using UnityEngine;
using UnityEngine.UI;
using VTOLVR.DLC.EW;

namespace DDArmory.Weapons.Loitering.ALTIUS;

public class ALTIUS_MaldUI : MonoBehaviour
{
    // this is 100% going to break in a future update
    public const string DRFMPath =
        "SettingsArea/ModeSelectTitle/SelectDRFM";
    public const string NoisePath =
        "SettingsArea/ModeSelectTitle/SelectNOISE";

    public MALDControlUI maldControlUI;

    private MALDUIButton drfmButton;
    private MALDUIButton noiseButton;

    public MissileDetector MissileDetector
    {
        get
        {
            if (_missileDetector == null)
                _missileDetector = maldControlUI.wm.GetComponentInChildren<MissileDetector>();
            return _missileDetector;
        }
    }

    private MissileDetector _missileDetector;
    
    private void Start()
    {
        var decoyControlsObj = maldControlUI.decoyControlsObj;
        
        var drfm = decoyControlsObj.transform.Find(DRFMPath);
        var drfmText = drfm.GetComponentInChildren<Text>();

        drfmButton = new MALDUIButton()
        {
            obj = drfm.gameObject,
            text = drfmText,
            origText = drfmText.text
        };
            
        var noise = decoyControlsObj.transform.Find(NoisePath);
        var noiseText = noise.GetComponentInChildren<Text>();

        noiseButton = new MALDUIButton()
        {
            obj = noise.gameObject,
            text = noiseText,
            origText = noiseText.text
        };
    }

    private void Update()
    {
        var currentDecoy = maldControlUI.currentDecoy;
        
        var isAltius = currentDecoy is ALTIUSGuidance;
        
        if (drfmButton != null)
        {
            if (isAltius)
            {
                drfmButton.obj.SetActive(true);
                drfmButton.text.text = "KILL";
            }
            else
            {
                drfmButton.obj.SetActive(false);
                drfmButton.text.text = drfmButton.origText;
            }
        }

        if (noiseButton != null)
        {
            noiseButton.text.text = isAltius ? "INTCP" : noiseButton.origText;
            
        }
    }
}

public class MALDUIButton
{
    public GameObject obj;
    public Text text;
    public string origText;
}