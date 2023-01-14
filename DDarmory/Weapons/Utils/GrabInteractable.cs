using System.Collections;
using UnityEngine;

public class GrabInteractable : MonoBehaviour
{
	public virtual void Start()
	{
		if (!interactable)
		{
			interactable = GetComponent<VRInteractable>();
		}
		interactable.OnStartInteraction += Ir_OnStartInteraction;
		interactable.OnStopInteraction += Ir_OnStopInteraction;
	}

	public virtual void Ir_OnStartInteraction(VRHandController controller)
	{
		_grabbed = true;
		_controllerTf = controller.transform;
		_localPos = _controllerTf.InverseTransformPoint(transform.position);
		_localRot = Quaternion.Inverse(_controllerTf.rotation) * transform.rotation;
		StartCoroutine(HeldRoutine());
	}

	private IEnumerator HeldRoutine()
	{
		while (_grabbed)
		{
			transform.position = Vector3.Lerp(transform.position, _controllerTf.TransformPoint(_localPos), lerpSpeed * Time.deltaTime);
			transform.rotation = Quaternion.Lerp(transform.rotation, _controllerTf.rotation * _localRot, rotationLerpSpeed * 2f * Time.deltaTime);
			yield return null;
		}
	}

	public virtual void Ir_OnStopInteraction(VRHandController controller)
	{
		_grabbed = false;
	}

	public float lerpSpeed;

	public float rotationLerpSpeed;

	public VRInteractable interactable;

	private Transform _controllerTf;
	
	private Vector3 _localPos;

	private Quaternion _localRot;

	private bool _grabbed;
}
