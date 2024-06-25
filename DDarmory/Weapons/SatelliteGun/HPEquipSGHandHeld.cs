using System;
using UnityEngine;
using UnityEngine.Events;

	public class HPEquipSGHandHeld : HPEquipSG
	{
		private int _previousEqIdx;

		public override void Start()
		{
			base.Start();
			interactable.OnStartInteraction += StartHolding;
			interactable.OnStopInteraction += StopHolding;
		}

		public void StartHolding(VRHandController controller)
		{
			_held = true;
			controller.OnThumbButtonPressed += Ctrlr_OnThumbButtonPressed;
			controller.OnThumbButtonReleased += Ctrlr_OnThumbButtonReleased;
		}

		public void StopHolding(VRHandController controller)
		{
			_held = false;
			controller.OnThumbButtonPressed -= Ctrlr_OnThumbButtonPressed;
			controller.OnThumbButtonReleased -= Ctrlr_OnThumbButtonReleased;
			
			var laserObject = new LaserObjectSync
			{
				enabled = false,
				laserEnd = Vector3D.zero,
				laserStart = Vector3.zero,
				laserLightPos = Vector3D.zero
			};
		}

		protected override Vector3 GetTargetPosition(OpticalTargeter targeter)
		{
			RaycastHit raycastHit;
		
			Vector3 result;
			if (_held && Physics.Raycast(laserTf.position, laserTf.forward, out raycastHit, maxDistance, mask))
			{
				Vector3[] positions = {
					Vector3.zero,
					laserRenderer.transform.InverseTransformPoint(raycastHit.point)
				};
				laserRenderer.SetPositions(positions);
				pointLight.transform.position = raycastHit.point + raycastHit.normal * 0.01f;
				result = raycastHit.point;
				if (OnSetLaser != null)
				{
					var laserObject = new LaserObjectSync
					{
						enabled = true,
						laserEnd = VTMapManager.WorldToGlobalPoint(raycastHit.point),
						laserStart = Vector3.zero,
						laserLightPos = VTMapManager.WorldToGlobalPoint(raycastHit.point + raycastHit.normal * 0.01f)
					};
					OnSetLaser.Invoke(laserObject);
				}
			}
			else
			{
				result = turret.transform.forward * 8000f;
			}
			return result;
		}

		public void Ctrlr_OnThumbButtonPressed(VRHandController controller)
		{
			ThumbButtonDown.Invoke();
		}

		public void Ctrlr_OnThumbButtonReleased(VRHandController controller)
		{
			ThumbButtonUp.Invoke();
		}

		[Header("Handheld Pointer")]
		public Transform laserTf;

		public LineRenderer laserRenderer;

		public LayerMask mask;

		public float maxDistance;

		public VRInteractable interactable;

		public Light pointLight;
		
		public event Action<LaserObjectSync> OnSetLaser;

		public UnityEvent ThumbButtonDown = new UnityEvent();

		public UnityEvent ThumbButtonUp = new UnityEvent();

		private bool _held;
		
		public class LaserObjectSync
		{
			public Vector3 laserStart;
			public Vector3D laserEnd;
			public Vector3D laserLightPos;
			public bool enabled;
		}
	}