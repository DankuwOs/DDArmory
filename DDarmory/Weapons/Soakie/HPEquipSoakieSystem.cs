using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class HPEquipSoakieSystem : HPEquippable, IMassObject
{
	protected override void OnEquip()
	{
		base.OnEquip();
		if (weaponManager)
		{
			_battery = weaponManager.battery;
			_material = internalMesh.material;
			_brightness = _material.GetFloat(brightnessProp);
			_tintColor = _material.GetColor(tintProp);
			Hitbox[] componentsInChildren = weaponManager.GetComponentsInChildren<Hitbox>(true);
			foreach (Hitbox hitbox in componentsInChildren)
			{
				if (!hitboxes.ContainsKey(hitbox))
				{
					hitboxes.Add(hitbox, hitbox.subtractiveArmor);
					hitbox.subtractiveArmor += additionalSubtractiveArmor;
				}
			}

			

			SetTransforms();
		}
	}

	public void SetTransforms()
	{
		Debug.Log("Setting up transforms for '" + shortName + "'");

		for (int i = 0; i < armorTfs.Length; i++)
		{
			var armorTf = armorTfs[i];
			var path = paths[i];
			var localPosition = localPositions[i];
			var localRotation = localRotations[i];
			
			string[] array = path.Split('/');
			if (!(array.Length > 0))
			{
				break;
			}
			
			Transform transform = weaponManager.transform;
			
			foreach (string n in array)
			{
				if (!transform)
				{
					return;
				}
				
				transform = transform.Find(n);
			}
			
			CustomWeaponsBase.instance.AddObject(armorTf.gameObject);
			armorTf.SetParent(transform);
			armorTf.localPosition = localPosition;
			armorTf.localRotation = Quaternion.Euler(localRotation);
			if (localScales.Length > 0)
				armorTf.localScale = localScales[i];
		}
	}

	public virtual void Update()
	{
		if (isEquipped)
		{
			float num = powerDraw * _brightness * Time.deltaTime;
			_battery.Drain(num);
			bool sufficientCharge = _battery.currentCharge > num  && _battery.connected;
			
			_brightness = Mathf.Clamp(sufficientCharge? _brightness : 0f, minBrightness, maxBrightness);
		}
	}

	public virtual void SetMaterialValues()
	{
		_material.SetFloat(brightnessProp, _brightness);
		_material.SetColor(tintProp, _tintColor);
	}

	public void SetBrightness(float brightness)
	{
		_brightness = brightness * maxBrightness;
		_brightnessEvent.Invoke(brightness);
	}

	public virtual void OnDamage(Hitbox hb, float damage)
	{
		if ((float)new Random().NextDouble() <= 0.2f)
		{
			Debug.Log("Changing color! Whoohoho!!");
			switch (new Random().Next(1, 4))
			{
			case 1:
				_tintColor.r = 0f;
				break;
			case 2:
				_tintColor.g = 0f;
				break;
			case 3:
				_tintColor.b = 0f;
				break;
			}
		}
		damage = Mathf.Abs(damage);
		if (hb)
		{
			float num;
			hitboxes.TryGetValue(hb, out num);
			float num2 = hb.subtractiveArmor - damage;
			bool flag3 = num2 < num;
			if (flag3)
			{
				hb.subtractiveArmor = num;
			}
			else
			{
				hb.subtractiveArmor = num2;
			}
		}
	}

	public float GetMass()
	{
		return mass;
	}

	private Dictionary<Hitbox, float> hitboxes = new Dictionary<Hitbox, float>();

	public Transform[] armorTfs;

	[Tooltip("Positon / Rotation relative to the targeted tf.")]
	public Vector3[] localPositions;

	public Vector3[] localRotations;

	public Vector3[] localScales;

	[Tooltip("Path to the transform in the aircraft prefab.")]
	public string[] paths;

	public Renderer internalMesh;

	public float powerDraw = 20f;

	public float maxBrightness = 3f;

	public float minBrightness;

	public float additionalSubtractiveArmor;

	public float mass;

	public string brightnessProp = "_Brightness";

	public string tintProp = "_LCDTint";

	private Battery _battery;

	private Material _material;

	private float _brightness;

	private Color _tintColor;

	[HideInInspector]
	public FloatEvent _brightnessEvent = new FloatEvent();

}
