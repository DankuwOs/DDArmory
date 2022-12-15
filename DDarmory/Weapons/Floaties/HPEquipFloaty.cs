using Harmony;
using UnityEngine;
using UnityEngine.Events;

public class HPEquipFloaty : HPEquippable, IMassObject
{
	protected override void OnEquip()
	{
		base.OnEquip();
		GrabComponents();
	}

	public void GrabComponents()
	{
		if (weaponManager)
		{
			_vehicleInputManager = weaponManager.GetComponent<VehicleInputManager>();
			_flightInfo = weaponManager.actor.flightInfo;
		}
		Debug.Log(string.Format("{0} | {1} | {2}", weaponManager.gameObject, _flightInfo, _vehicleInputManager));
		SetDrag(waterBuoyancies, baseDrag);
		if (usingWheels)
		{
			StartWheels();
		}
	}

	private void FixedUpdate()
	{
		if ((_deployed || alwaysDeployed) && _vehicleInputManager)
		{
			Steer(_vehicleInputManager.outputPYR);
			Traverse traverse = Traverse.Create(_vehicleInputManager);
			if (_vehicleInputManager.wheelBrakesBound)
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

	public void Steer(Vector3 input)
	{
		float num = input.y;
		if (Mathf.Abs(num) < 0.2f)
		{
			SetDrag(waterBuoyancies, baseDrag);
		}
		else
		{
			num = (Mathf.Abs(num) - 0.2f) * 1.2f;
			float drag = steerForceCurve.Evaluate(num) * steeringForce;
			if (num >= 0f)
			{
				SetDrag(steerRight, drag);
			}
			else
			{
				SetDrag(steerLeft, drag);
			}
		}
	}

	public void SetBrakes(float input)
	{
		if (input != 0f)
		{
			if (input < 0.05f)
			{
				SetDrag(brakeBuoyancies, baseDrag);
				return;
			}
		}
		SetDrag(brakeBuoyancies, brakeForceCurve.Evaluate(input) * maxBrakeForce);
	}
	
	public void SetDrag(WaterBuoyancy[] list, float drag)
	{
		foreach (WaterBuoyancy waterBuoyancy in list)
		{
			waterBuoyancy.drag = drag;
		}
	}

	public void ToggleDeploy()
	{
		if (!_deployed)
		{
			Deploy();
		}
		else
		{
			Retract();
		}
	}

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

	public void StartWheels()
	{
		if (weaponManager)
		{
			_wheelsController = weaponManager.GetComponent<WheelsController>();
			weaponManager.transform.Find("LightSwitches").Find("LandingLight").GetComponent<ObjectPowerUnit>().objectToPower = landingLightParent;
		}
		if (!_wheelsController)
		{
			Debug.Log("FLTY: No wheels controller!");
		}
		else
		{
			GameObject gameObject = _wheelsController.gearAnimator.GetComponentInParent<TireRollAudio>().gameObject;
			gameObject.SetActive(false);

			var landingGearLever = weaponManager.GetComponentInChildren<LandingGearLever>();
			if (landingGearLever)
			{
				landingGearLever.gear = new [] { animator };
			}
			
			animator.battery = _wheelsController.gearAnimator.battery;
			animator.statusLight = _wheelsController.gearAnimator.statusLight;
			animator.dragComponent.rb = _wheelsController.gearAnimator.dragComponent.rb;
			_wheelsController.gearAnimator = animator;
			_wheelsController.suspensions = suspensions;
			_wheelsController.steeringTransform = steerTf;

			_wheelsController.leftBrakeIdx = leftBrakeIdx;
			_wheelsController.rightBrakeIdx = RightBrakeIdx;
			
			_flightInfo.suspensions = suspensions;
		}
	}

	public float GetMass()
	{
		return mass;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = gizmoColor;
		foreach (WaterBuoyancy waterBuoyancy in waterBuoyancies)
		{
			Gizmos.DrawSphere(waterBuoyancy.transform.position, 0.3f);
		}
	}

	[Header("Floatie")]
	public bool alwaysDeployed;

	[Range(0f, 0.3f)]
	public float maxBrakeForce;

	[Range(0f, 0.1f)]
	public float steeringForce;

	[Range(0f, 0.01f)]
	public float baseDrag;

	public WaterBuoyancy[] waterBuoyancies;

	public WaterBuoyancy[] steerRight;

	public WaterBuoyancy[] steerLeft;

	public WaterBuoyancy[] brakeBuoyancies;

	public AnimationCurve steerForceCurve;

	public AnimationCurve brakeForceCurve;

	public UnityEvent OnDeploy;

	public UnityEvent OnRetract;

	public float mass;

	[Header("Wheels")]
	public bool usingWheels;

	public GearAnimator animator;

	public RaySpringDamper[] suspensions;

	public GameObject landingLightParent;

	public Transform steerTf;

	[Tooltip("0 Based Index Index of Left Brake")]
	public int leftBrakeIdx = 3;

	[Tooltip("0 Based Index Index of Right Brake")]
	public int RightBrakeIdx = 1;

	private bool _deployed;

	private VehicleInputManager _vehicleInputManager;

	private WheelsController _wheelsController;

	private FlightInfo _flightInfo;

	public Color gizmoColor = new Color(0f, 0f, 255f, 0.4f);
}
