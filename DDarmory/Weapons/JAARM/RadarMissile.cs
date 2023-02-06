using UnityEngine;
using VTOLVR.Multiplayer;

public class RadarMissile : Missile
{
    [Header("JAARM")] public Radar Radar;

    public override void Fire()
    {
        base.Fire();

        if (!Radar) return;

        Radar.radarEnabled = true;
        Radar.myActor = actor;
        Radar.OnDetectedActor += delegate(Actor detectedActor)
        {
            detectedActor.DetectActor(actor.team, actor); // Don't know what this does so bye bye

            if (VTOLMPUtils.IsMultiplayer())
                VTOLMPDataLinkManager.instance.ReportKnownPosition(detectedActor, actor.team);
        };
    }
}