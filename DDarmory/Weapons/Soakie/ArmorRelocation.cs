using System;
using UnityEngine;

namespace DDArmory
{
	[Serializable]
	public class ArmorRelocation
	{
		public Transform armorTf;

		[Tooltip("Positon / Rotation relative to the targeted tf.")]
		public Vector3 localPosition;

		public Vector3 localRotation;

		[Tooltip("Path to the transform in the aircraft prefab.")]
		public string path;
	}
}
