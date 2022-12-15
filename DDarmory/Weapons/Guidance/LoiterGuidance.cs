using DDArmory.Guidance;
using UnityEngine;
using UnityEngine.Events;
public class LoiterGuidance : MissileGuidanceUnit
{
	public override Vector3 GetGuidedPoint()
	{
		Actor opticalTargetFromView = TargetManager.instance.GetOpticalTargetFromView(missile.actor, visualRange, Actor.GetRoleMask(roleMask), 300f, eyeTf.position, eyeTf.forward, fov);
		bool flag = opticalTargetFromView && opticalTargetFromView.alive && !opticalTargetFromView.GetComponent<imtargetedlmao>();
		Vector3 result;
		if (flag)
		{
			opticalTargetFromView.gameObject.AddComponent<imtargetedlmao>();
			opticalTargetFromView.health.OnDeath.AddListener(delegate
			{
				missile.navMode = Missile.NavModes.Custom;
				missile.guidanceMode = Missile.GuidanceModes.GPS;
				OnTargetLost.Invoke();
			});
			missile.navMode = Missile.NavModes.LeadTime;
			missile.guidanceMode = Missile.GuidanceModes.Optical;
			missile.SetOpticalTarget(opticalTargetFromView.transform, opticalTargetFromView);
			OnTargetAcquired.Invoke();
			result = Vector3.zero;
		}
		else
		{
			Vector3 vector = gpsPoint.worldPosition + Vector3.up * height;
			Vector3 normalized = (transform.position - vector).normalized;
			Vector3 b = normalized * loiterRadius;
			Vector3 a = Vector3.Cross(Vector3.ProjectOnPlane(normalized, Vector3.up), Vector3.up);
			Vector3 b2 = a * simDistance;
			Vector3 vector2 = vector + b2 + b;
			vector2.y = gpsPoint.worldPosition.y + height;
			bool flag2 = debugPoint;
			if (flag2)
			{
				_renderer.gameObject.SetActive(true);
				_renderer2.gameObject.SetActive(true);
				_renderer = DebugLinesManager.LineRendererToPoint(transform.position, vector2, _renderer);
				_renderer2 = DebugLinesManager.LineRendererToPoint(vector2, gpsPoint.worldPosition, _renderer2);
			}
			result = vector2;
		}
		return result;
	}

	// Token: 0x0400004C RID: 76
	public GPSTarget gpsPoint;

	// Token: 0x0400004D RID: 77
	public float loiterRadius = 8000f;

	// Token: 0x0400004E RID: 78
	public float height = 3000f;

	// Token: 0x0400004F RID: 79
	public float simDistance = 2000f;

	// Token: 0x04000050 RID: 80
	public Actor.Roles roleMask;

	// Token: 0x04000051 RID: 81
	public Transform eyeTf;

	// Token: 0x04000052 RID: 82
	public float visualRange;

	// Token: 0x04000053 RID: 83
	public float fov;

	// Token: 0x04000054 RID: 84
	public UnityEvent OnTargetAcquired = new UnityEvent();

	// Token: 0x04000055 RID: 85
	public UnityEvent OnTargetLost = new UnityEvent();

	// Token: 0x04000056 RID: 86
	[Header("Debug")]
	public bool debugPoint;

	// Token: 0x04000057 RID: 87
	public LineRenderer _renderer;

	// Token: 0x04000058 RID: 88
	public LineRenderer _renderer2;
}
