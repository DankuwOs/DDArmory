using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class DriveUsage : MonoBehaviour
{
    public Text driveUsageText;

    public ParticleSystem.MinMaxGradient driveUsageGradient;

    [HideInInspector] public DriveInfo driveInfo;

    private string _driveLabel = "";
    
    private void Start()
    {
        driveInfo = new DriveInfo(VTResources.gameRootDirectory);
        _driveLabel = driveInfo.Name;
    }

    private void FixedUpdate()
    {
        var used = GetDrivePercentUsed();
        driveUsageText.text = $"{_driveLabel}     USG| { used * 10 :00','0%}";
        driveUsageText.color = driveUsageGradient.Evaluate(used);
    }

    public float GetDrivePercentUsed()
    {
        if (driveInfo == null)
            return -1;

        var total = driveInfo.TotalSize;
        var used = driveInfo.TotalSize - driveInfo.AvailableFreeSpace;
        
        var percent = (float)used / total;

        return percent;
    }
}