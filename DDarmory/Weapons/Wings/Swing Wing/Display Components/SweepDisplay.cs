using UnityEngine;

public class SweepDisplay : MonoBehaviour
{
    public SweepBar currentSweep;
    public SweepBar targetSweep;

    public bool barEmission;
    
    public SweepMode manualObj;
    public SweepMode autoObj;

    private void Start()
    {
        Debug.Log($"[SweepDisplay]: Start!");
        if (barEmission)
        {
            var currentSweepMat = currentSweep.bar.GetComponent<MeshRenderer>().material;
            var targetSweepMat = targetSweep.bar.GetComponent<MeshRenderer>().material;
            if (currentSweepMat)
            {
                currentSweepMat.SetColor("_Color", currentSweep.albedoColor);
                currentSweepMat.SetColor("_EmissionColor", currentSweep.emissiveColor);
            }

            if (targetSweepMat)
            {
                targetSweepMat.SetColor("_Color", targetSweep.albedoColor);
                targetSweepMat.SetColor("_EmissionColor", targetSweep.emissiveColor);
            }
        }

        var manualObjMat = manualObj.modeTf.GetComponent<MeshRenderer>().material;
        var autoObjMat = autoObj.modeTf.GetComponent<MeshRenderer>().material;

        if (manualObjMat)
        {
            manualObjMat.SetColor("_Color", manualObj.albedoColor);
            manualObjMat.SetColor("_EmissionColor", manualObj.emissiveColor);
        }

        if (autoObjMat)
        {
            autoObjMat.SetColor("_Color", autoObj.albedoColor);
            autoObjMat.SetColor("_EmissionColor", autoObj.emissiveColor);
        }
    }
}