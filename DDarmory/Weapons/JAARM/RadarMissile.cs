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
        Radar.OnDetectedActor += delegate(Radar.DetectedUnit detectedUnit)
        {
            actor.DetectActor(actor.team, detectedUnit.detIdentity, TacticalSituationController.DataSources.Radar);
        };
    }
}