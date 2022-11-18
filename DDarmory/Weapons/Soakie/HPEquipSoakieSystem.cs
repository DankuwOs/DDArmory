using System.Collections.Generic;
using DDArmory;
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

	public virtual void SetTransforms()
	{
		Debug.Log("Setting up transforms for '" + shortName + "'");
		Debug.Log(string.Format("'{0}'", setTransforms));
		foreach (ArmorRelocation armorRelocation in setTransforms)
		{
			string[] array = armorRelocation.path.Split('/');
			bool flag = array.Length == 0;
			if (flag)
			{
				break;
			}
			Transform transform = weaponManager.transform;
			foreach (string n in array)
			{
				bool flag2 = transform == null;
				if (flag2)
				{
					return;
				}
				transform = transform.Find(n);
			}
			Transform armorTf = armorRelocation.armorTf;
			CustomWeaponsBase.instance.AddObject(armorTf.gameObject);
			armorTf.SetParent(transform);
			armorTf.localPosition = armorRelocation.localPosition;
			armorTf.localRotation = Quaternion.Euler(armorRelocation.localRotation);
		}
	}

	public virtual void Update()
	{
		if (isEquipped)
		{
			float num = powerDraw * _brightness * Time.deltaTime;
			_battery.Drain(num);
			bool flag2 = _battery.currentCharge < num;
			if (flag2)
			{
				_brightness = 0f;
			}
			_brightness = Mathf.Clamp(_brightness, minBrightness, maxBrightness);
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

	[SerializeReference]
	public List<ArmorRelocation> setTransforms;

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
