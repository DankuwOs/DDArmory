using UnityEngine;
using UnityEngine.Events;

	public class HPEquipSGHandHeld : HPEquipSG
	{
		private int _previousEqIdx;
		
		protected override void Start()
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
			}
			else
			{
				result = turret.transform.forward * 8000f;
			}
			return result;
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();
			var held = _held;
			if (!held)
			{
				laser.localPosition = Vector3.Lerp(laser.localPosition, returnVector, returnSpeed * Time.deltaTime);
				laser.localRotation = Quaternion.Lerp(laser.localRotation, Quaternion.identity, returnSpeed * 2f * Time.deltaTime);
			}
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

		public float returnSpeed = 10f;

		public Transform laser;

		public Vector3 returnVector;

		public Light pointLight;

		public UnityEvent ThumbButtonDown = new UnityEvent();

		public UnityEvent ThumbButtonUp = new UnityEvent();

		private bool _held;
	}