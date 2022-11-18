using Harmony;
using UnityEngine;
using UnityEngine.Events;

public class HPEquipFloatyGunTurret : HPEquipGunTurret, IMassObject
{
	protected override void OnEquip()
	{
		base.OnEquip();
		GrabComponents();
	}

	public void GrabComponents()
	{
		bool flag = weaponManager;
		if (flag)
		{
			_vehicleInputManager = weaponManager.GetComponent<VehicleInputManager>();
			_flightInfo = weaponManager.actor.flightInfo;
		}
		Debug.Log(string.Format("{0} | {1} | {2}", weaponManager.gameObject, _flightInfo, _vehicleInputManager));
		SetDrag(waterBuoyancies, baseDrag);
		bool flag2 = usingWheels;
		if (flag2)
		{
			StartWheels();
		}
	}

	private void FixedUpdate()
	{
		bool flag = _deployed || alwaysDeployed;
		if (flag)
		{
			Debug.Log("Steering..");
			Steer(_vehicleInputManager.outputPYR);
			Traverse traverse = Traverse.Create(_vehicleInputManager);
			bool wheelBrakesBound = _vehicleInputManager.wheelBrakesBound;
			if (wheelBrakesBound)
			{
				float value = traverse.Field("hardwareBrakeL").GetValue<float>();
				float value2 = traverse.Field("hardwareBrakeR").GetValue<float>();
				SetBrakes((value + value2) / 2f);
			}
			else
			{
				float value3 = traverse.Field("virtualBrakes").GetValue<float>();
				SetBrakes(value3);
			}
		}
	}

	// Token: 0x06000030 RID: 48 RVA: 0x00002E94 File Offset: 0x00001094
	public void Steer(Vector3 input)
	{
		float num = input.y;
		bool flag = num >= 0f;
		bool flag2 = Mathf.Abs(num) < 0.2f;
		if (flag2)
		{
			SetDrag(waterBuoyancies, baseDrag);
		}
		else
		{
			num = (Mathf.Abs(num) - 0.2f) * 1.2f;
			float drag = steerForceCurve.Evaluate(num) * steeringForce;
			bool flag3 = flag;
			if (flag3)
			{
				SetDrag(steerRight, drag);
			}
			else
			{
				SetDrag(steerLeft, drag);
			}
		}
	}

	// Token: 0x06000031 RID: 49 RVA: 0x00002F28 File Offset: 0x00001128
	public void SetBrakes(float input)
	{
		bool flag = input != 0f;
		if (flag)
		{
			bool flag2 = input < 0.05f;
			if (flag2)
			{
				SetDrag(brakeBuoyancies, baseDrag);
				return;
			}
		}
		SetDrag(brakeBuoyancies, brakeForceCurve.Evaluate(input) * maxBrakeForce);
	}

	// Token: 0x06000032 RID: 50 RVA: 0x00002F8C File Offset: 0x0000118C
	public void SetDrag(WaterBuoyancy[] list, float drag)
	{
		foreach (WaterBuoyancy waterBuoyancy in list)
		{
			waterBuoyancy.drag = drag;
		}
	}

	// Token: 0x06000033 RID: 51 RVA: 0x00002FB8 File Offset: 0x000011B8
	public void ToggleDeploy()
	{
		bool flag = !_deployed;
		if (flag)
		{
			Deploy();
		}
		else
		{
			Retract();
		}
	}

	// Token: 0x06000034 RID: 52 RVA: 0x00002FE4 File Offset: 0x000011E4
	public void Deploy()
	{
		_deployed = true;
		UnityEvent onDeploy = OnDeploy;
		bool flag = onDeploy != null;
		if (flag)
		{
			onDeploy.Invoke();
		}
	}

	// Token: 0x06000035 RID: 53 RVA: 0x00003014 File Offset: 0x00001214
	public void Retract()
	{
		_deployed = false;
		UnityEvent onRetract = OnRetract;
		bool flag = onRetract != null;
		if (flag)
		{
			onRetract.Invoke();
		}
	}

	// Token: 0x06000036 RID: 54 RVA: 0x00003044 File Offset: 0x00001244
	public void StartWheels()
	{
		_wheelsController = weaponManager.GetComponent<WheelsController>();
		bool flag = !_wheelsController;
		if (flag)
		{
			Debug.Log("FLTY: No wheels controller!");
		}
		else
		{
			weaponManager.transform.Find("LightSwitches").Find("LandingLight").GetComponent<ObjectPowerUnit>().objectToPower = landingLightParent;
			GameObject gameObject = _wheelsController.gearAnimator.GetComponentInParent<TireRollAudio>().gameObject;
			gameObject.SetActive(false);
			animator.battery = _wheelsController.gearAnimator.battery;
			animator.statusLight = _wheelsController.gearAnimator.statusLight;
			animator.dragComponent.rb = _wheelsController.gearAnimator.dragComponent.rb;
			_wheelsController.gearAnimator = animator;
			_wheelsController.suspensions = suspensions;
			_wheelsController.steeringTransform = steerTf;
			_flightInfo.suspensions = suspensions;
		}
	}

	// Token: 0x06000037 RID: 55 RVA: 0x00003174 File Offset: 0x00001374
	public float GetMass()
	{
		return mass;
	}

	// Token: 0x06000038 RID: 56 RVA: 0x0000318C File Offset: 0x0000138C
	private void OnDrawGizmos()
	{
		Gizmos.color = gizmoColor;
		foreach (WaterBuoyancy waterBuoyancy in waterBuoyancies)
		{
			Gizmos.DrawSphere(waterBuoyancy.transform.position, 0.3f);
		}
	}

	// Token: 0x04000029 RID: 41
	[Header("Floatie")]
	public bool alwaysDeployed;

	// Token: 0x0400002A RID: 42
	[Range(0f, 1f)]
	public float maxBrakeForce;

	// Token: 0x0400002B RID: 43
	[Range(0f, 1f)]
	public float steeringForce;

	// Token: 0x0400002C RID: 44
	[Range(0f, 0.01f)]
	public float baseDrag;

	// Token: 0x0400002D RID: 45
	public WaterBuoyancy[] waterBuoyancies;

	// Token: 0x0400002E RID: 46
	public WaterBuoyancy[] steerRight;

	// Token: 0x0400002F RID: 47
	public WaterBuoyancy[] steerLeft;

	// Token: 0x04000030 RID: 48
	public WaterBuoyancy[] brakeBuoyancies;

	// Token: 0x04000031 RID: 49
	public AnimationCurve steerForceCurve;

	// Token: 0x04000032 RID: 50
	public AnimationCurve brakeForceCurve;

	// Token: 0x04000033 RID: 51
	public UnityEvent OnDeploy;

	// Token: 0x04000034 RID: 52
	public UnityEvent OnRetract;

	// Token: 0x04000035 RID: 53
	public float mass;

	// Token: 0x04000036 RID: 54
	[Header("Wheels")]
	public bool usingWheels;

	// Token: 0x04000037 RID: 55
	public GearAnimator animator;

	// Token: 0x04000038 RID: 56
	public RaySpringDamper[] suspensions;

	// Token: 0x04000039 RID: 57
	public GameObject landingLightParent;

	// Token: 0x0400003A RID: 58
	public Transform steerTf;

	// Token: 0x0400003B RID: 59
	private bool _deployed;

	// Token: 0x0400003C RID: 60
	private VehicleInputManager _vehicleInputManager;

	// Token: 0x0400003D RID: 61
	private WheelsController _wheelsController;

	// Token: 0x0400003E RID: 62
	private FlightInfo _flightInfo;

	// Token: 0x0400003F RID: 63
	public Color gizmoColor = new Color(0f, 0f, 255f, 0.4f);
}
