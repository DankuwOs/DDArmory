using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Token: 0x02000020 RID: 32
public class HPEquipSoakieSystemMulticrew : HPEquippable, IMassObject
{
	protected override void OnEquip()
	{
		bool flag = !weaponManager;
		if (!flag)
		{
			_battery = weaponManager.battery;
			_materials = (from renderer in internalMeshs
			select renderer.materials[0]).ToArray();
			_colorBrightness = _materials[0].GetFloat(colorBrightnessProp);
			_depthBrightness = _materials[0].GetFloat(depthBrightnessProp);
			Hitbox[] componentsInChildren = weaponManager.GetComponentsInChildren<Hitbox>(true);
			foreach (Hitbox hitbox in componentsInChildren)
			{
				_hitBoxes.Add(hitbox, hitbox.subtractiveArmor);
				hitbox.subtractiveArmor += additionalSubtractiveArmor;
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

	public void Update()
	{
		bool flag = !isEquipped;
		if (!flag)
		{
			float num = powerDraw * _colorBrightness + _depthBrightness * Time.deltaTime;
			_battery.Drain(num);
			bool sufficientCharge = _battery.currentCharge > num  && _battery.connected;
			
			_colorBrightness = Mathf.Clamp(sufficientCharge? _colorBrightness : 0f, minBrightness, maxBrightness);
			_depthBrightness = Mathf.Clamp(sufficientCharge? _depthBrightness : 0f, minBrightness, maxBrightness);
			SetMaterialValues();
		}
	}

	public void SetMaterialValues()
	{
		foreach (Material material in _materials)
		{
			material.SetFloat(colorBrightnessProp, _colorBrightness);
			material.SetFloat(depthBrightnessProp, _depthBrightness);
		}
	}

	public void SetColorBrightness(float brightness)
	{
		_colorBrightness = brightness * maxBrightness;
		_colorBrightnessEvent.Invoke(brightness);
	}

	public void SetDepthBrightness(float brightness)
	{
		_depthBrightness = brightness * maxBrightness;
		_depthBrightnessEvent.Invoke(brightness);
	}

	public void OnDamage(Hitbox hb, float damage)
	{
		damage = Mathf.Abs(damage);
		bool flag = !hb;
		if (!flag)
		{
			float num;
			_hitBoxes.TryGetValue(hb, out num);
			float num2 = hb.subtractiveArmor - damage;
			bool flag2 = num2 < num;
			if (flag2)
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

	public Transform[] armorTfs;

	[Tooltip("Positon / Rotation relative to the targeted tf.")]
	public Vector3[] localPositions;

	public Vector3[] localRotations;

	public Vector3[] localScales;

	[Tooltip("Path to the transform in the aircraft prefab.")]
	public string[] paths;

	public float powerDraw = 0.75f;

	public float maxBrightness = 3f;

	public float minBrightness;

	public float additionalSubtractiveArmor;

	public float mass;

	[Header("Multicrew")]
	public Renderer[] internalMeshs;

	public string colorBrightnessProp = "_ColorMult";

	public string depthBrightnessProp = "_Brightness";

	private Material[] _materials;

	private float _colorBrightness;
	
	private float _depthBrightness;

	private Battery _battery;

	private Dictionary<Hitbox, float> _hitBoxes = new Dictionary<Hitbox, float>();

	[HideInInspector]
	public FloatEvent _colorBrightnessEvent = new FloatEvent();

	[HideInInspector]
	public FloatEvent _depthBrightnessEvent = new FloatEvent();
}
