using System.Collections.Generic;
using UnityEngine;

	public class HPEquipSoakieSystemCRT : HPEquippable, IMassObject
	{
		protected override void OnEquip()
		{
			if (!weaponManager) return;
		
			_battery = weaponManager.battery;

			crts = GetComponentsInChildren<CRTRenderer>();
			
			var componentsInChildren = weaponManager.GetComponentsInChildren<Hitbox>(true);
			foreach (var hitbox in componentsInChildren)
			{
				if (!hitboxes.ContainsKey(hitbox))
				{
					hitboxes.Add(hitbox, hitbox.subtractiveArmor);
					hitbox.subtractiveArmor += additionalSubtractiveArmor;
				}
			}
			SetTransforms();
		}
		
		public override void OnDestroy()
		{
			foreach (var armorTf in armorTfs)
			{
				DestroyImmediate(armorTf.gameObject);
			}
		}

		public override void OnUnequip()
		{
			foreach (var armorTf in armorTfs)
			{
				DestroyImmediate(armorTf.gameObject);
			}
		}

		public void SetTransforms()
		{
			Debug.Log("Setting up transforms for '" + shortName + "'");

			for (var i = 0; i < armorTfs.Length; i++)
			{
				var armorTf = armorTfs[i];
				var path = paths[i];
				var localPosition = localPositions[i];
				var localRotation = localRotations[i];
			
				var array = path.Split('/');
				if (!(array.Length > 0))
				{
					break;
				}
			
				var transform = weaponManager.transform;
			
				foreach (var n in array)
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

		public virtual void FixedUpdate()
		{
			if (!isEquipped) return;
		
			var num = powerDraw * Time.deltaTime;
			_battery.Drain(num);
			var toggle = _battery.currentCharge > num && _battery.connected;
			foreach (var crtRenderer in crts)
			{
				if (crtRenderer.crtEnabled)
					crtRenderer.SetDisplay(!toggle, false);
			}
		}

		public virtual void OnDamage(Hitbox hb, float damage)
		{
			damage = Mathf.Abs(damage);
		
			if (!hb) return;

			hitboxes.TryGetValue(hb, out var num);
			var num2 = hb.subtractiveArmor - damage;
			var flag3 = num2 < num;
			if (flag3)
			{
				hb.subtractiveArmor = num;
			}
			else
			{
				hb.subtractiveArmor = num2;
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

		public float powerDraw = 20f;

		public float additionalSubtractiveArmor;

		public float mass;

		private Battery _battery;

		private CRTRenderer[] crts;
	}