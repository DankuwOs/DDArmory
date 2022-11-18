using System.Collections.Generic;
using UnityEngine;

// Token: 0x0200000F RID: 15
public class RadarMissile : Missile
{
	// Token: 0x06000045 RID: 69 RVA: 0x000037F4 File Offset: 0x000019F4
	public override void Fire()
	{
		base.Fire();
		bool flag = !Radar;
		if (!flag)
		{
			Radar.radarEnabled = true;
			Radar.myActor = this.actor;
			this.actor.role = Actor.Roles.Air;
			Radar.OnDetectedActor += delegate(Actor detectedActor)
			{
				List<Actor> list = (this.actor.team == Teams.Allied) ? TargetManager.instance.alliedUnits : TargetManager.instance.enemyUnits;
				foreach (Actor actor in list)
				{
					bool flag2 = !actor.weaponManager || !actor.weaponManager.tsc;
					if (flag2)
					{
						break;
					}
					actor.weaponManager.tsc.DataLinkedRadarDetected(detectedActor);
				}
			};
		}
	}

	// Token: 0x04000059 RID: 89
	[Header("JAARM")]
	public Radar Radar;
}
