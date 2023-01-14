using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

public class RamUsage : MonoBehaviour
{
    public Text memDisplay;

    public ParticleSystem.MinMaxGradient memGradient;

    private PerformanceCounter _memCounter;

    private void FixedUpdate()
    {
        double physicalAvailable = GetPhysicalAvailableMemoryInMiB() / 1024;
        var total = (double)SystemInfo.systemMemorySize / 1024;

        var usedPercent = Math.Round(GetPercentUsed() * 100, 1);
            
        memDisplay.text = $"MEM     USG: {(total - physicalAvailable) * 10 :00','0}  /  {total * 10 :00','0}   |   { usedPercent * 10 :00','0}%";
        memDisplay.color = memGradient.Evaluate((float)usedPercent / 100);
    }
    
    private int GetPhysicalAvailableMemoryInMiB()
    {
        _memCounter ??= new PerformanceCounter("Mono Memory", "Available Physical Memory");
        var memory = _memCounter.NextValue() / 1024 / 1024;
        return Mathf.RoundToInt(memory);
    }

    public float GetPercentUsed()
    {
        var total = (float)SystemInfo.systemMemorySize;
        var available = GetPhysicalAvailableMemoryInMiB();
        var used = available - total;

        return Mathf.Abs(used / total);
    }
}