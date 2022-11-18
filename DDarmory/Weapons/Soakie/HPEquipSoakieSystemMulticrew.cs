using System.Collections.Generic;
using System.Linq;
using DDArmory;
using UnityEngine;

// Token: 0x02000020 RID: 32
public class HPEquipSoakieSystemMulticrew : HPEquippable, IMassObject
{
	// Token: 0x06000099 RID: 153 RVA: 0x000053C8 File Offset: 0x000035C8
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

	// Token: 0x0600009A RID: 154 RVA: 0x000054BC File Offset: 0x000036BC
	public void SetTransforms()
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

	// Token: 0x0600009B RID: 155 RVA: 0x000055F4 File Offset: 0x000037F4
	public void Update()
	{
		bool flag = !this.isEquipped;
		if (!flag)
		{
			float num = powerDraw * _colorBrightness + _depthBrightness * Time.deltaTime;
			_battery.Drain(num);
			bool flag2 = _battery.currentCharge < num;
			if (flag2)
			{
				_colorBrightness = 0f;
				_depthBrightness = 0f;
			}
			_colorBrightness = Mathf.Clamp(_colorBrightness, minBrightness, maxBrightness);
			_depthBrightness = Mathf.Clamp(_depthBrightness, minBrightness, maxBrightness);
			SetMaterialValues();
		}
	}

	// Token: 0x0600009C RID: 156 RVA: 0x000056A8 File Offset: 0x000038A8
	public void SetMaterialValues()
	{
		foreach (Material material in _materials)
		{
			material.SetFloat(colorBrightnessProp, _colorBrightness);
			material.SetFloat(depthBrightnessProp, _depthBrightness);
		}
	}

	// Token: 0x0600009D RID: 157 RVA: 0x000056F8 File Offset: 0x000038F8
	public void SetColorBrightness(float brightness)
	{
		_colorBrightness = brightness * maxBrightness;
		_colorBrightnessEvent.Invoke(brightness);
	}

	// Token: 0x0600009E RID: 158 RVA: 0x00005716 File Offset: 0x00003916
	public void SetDepthBrightness(float brightness)
	{
		_depthBrightness = brightness * maxBrightness;
		_depthBrightnessEvent.Invoke(brightness);
	}

	// Token: 0x0600009F RID: 159 RVA: 0x00005734 File Offset: 0x00003934
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

	// Token: 0x060000A0 RID: 160 RVA: 0x0000578C File Offset: 0x0000398C
	public float GetMass()
	{
		return mass;
	}

	// Token: 0x060000A1 RID: 161 RVA: 0x000057A4 File Offset: 0x000039A4
	[ContextMenu("Test Transforms")]
	public void DebugTransforms()
	{
		Debug.Log("Setting up transforms for '" + shortName + "'");
		Debug.Log(string.Format("'{0}'", setTransforms));
		foreach (ArmorRelocation armorRelocation in setTransforms)
		{
			Debug.Log("a");
			string[] array = armorRelocation.path.Split('/');
			Debug.Log("b");
			bool flag = array.Length == 0;
			if (flag)
			{
				break;
			}
			Debug.Log("c");
			Transform transform = baseObject;
			foreach (string n in array)
			{
				bool flag2 = transform == null;
				if (flag2)
				{
					return;
				}
				transform = transform.Find(n);
			}
			Debug.Log("d");
			Transform transform2 = Instantiate(armorRelocation.armorTf.gameObject).transform;
			Debug.Log("e");
			transform2.SetParent(transform);
			transform2.localPosition = armorRelocation.localPosition;
			transform2.localRotation = Quaternion.Euler(armorRelocation.localRotation);
			Destroy(transform2.gameObject, 10f);
		}
	}

	// Token: 0x040000C3 RID: 195
	[SerializeReference]
	public List<ArmorRelocation> setTransforms;

	// Token: 0x040000C4 RID: 196
	public float powerDraw = 0.75f;

	// Token: 0x040000C5 RID: 197
	public float maxBrightness = 3f;

	// Token: 0x040000C6 RID: 198
	public float minBrightness;

	// Token: 0x040000C7 RID: 199
	public float additionalSubtractiveArmor;

	// Token: 0x040000C8 RID: 200
	public float mass;

	// Token: 0x040000C9 RID: 201
	[Header("Multicrew")]
	public Renderer[] internalMeshs;

	// Token: 0x040000CA RID: 202
	public string colorBrightnessProp = "_ColorMult";

	// Token: 0x040000CB RID: 203
	public string depthBrightnessProp = "_Brightness";

	// Token: 0x040000CC RID: 204
	private Material[] _materials;

	// Token: 0x040000CD RID: 205
	private float _colorBrightness;

	// Token: 0x040000CE RID: 206
	private float _depthBrightness;

	// Token: 0x040000CF RID: 207
	private Battery _battery;

	// Token: 0x040000D0 RID: 208
	private Dictionary<Hitbox, float> _hitBoxes = new Dictionary<Hitbox, float>();

	// Token: 0x040000D1 RID: 209
	[HideInInspector]
	public FloatEvent _colorBrightnessEvent = new FloatEvent();

	// Token: 0x040000D2 RID: 210
	[HideInInspector]
	public FloatEvent _depthBrightnessEvent = new FloatEvent();

	// Token: 0x040000D3 RID: 211
	[Header("Debug")]
	public Transform baseObject;
}
