using UnityEngine;
using UnityEngine.Events;

public class HPEquipMDS : HPEquippable, IMassObject
{
	public void Fired()
	{
		if (weaponManager.lockingRadar && weaponManager.lockingRadar.currentLock.actor)
		{
			var actor = weaponManager.actor;
			var actor2 = weaponManager.lockingRadar.currentLock.actor;
			var component = actor.GetComponent<FlightInfo>();
			SwapActors(actor, actor2, component);
		}
	}

	public void SwapActors(Actor player, Actor lockedActor, FlightInfo playerFlightInfo)
	{
		var vesselRB = player.weaponManager.vesselRB;
		var component = lockedActor.GetComponent<Rigidbody>();
		playerFlightInfo.PauseGCalculations();
		var position = vesselRB.position;
		var velocity = vesselRB.velocity;
		var rotation = vesselRB.rotation;
		vesselRB.MovePosition(component.position);
		vesselRB.velocity = component.velocity;
		vesselRB.MoveRotation(component.rotation);
		
		if (component.isKinematic)
		{
			Debug.Log("Attempted to swap places (KINEMATIC)");
			var component2 = component.GetComponent<KinematicPlane>();
			component2.fixedPoint.point = position;
			component2.rb.velocity = velocity;
			component2.transform.rotation = rotation;
		}
		else
		{
			Debug.Log("Attempted to swap places (NOT KINEMATIC)");
			component.MovePosition(position);
			component.velocity = velocity;
			component.MoveRotation(rotation);
		}
		playerFlightInfo.UnpauseGCalculations();
	}

	public float GetMass()
	{
		return 0.0003f;
	}

	public AudioSource _source;

	[Range(0f, 5f)]
	public float pitchFactor;

	[Range(0f, 5f)]
	public float windUpTime;

	[Range(0f, 3f)]
	public float volumeMultiplier;

	public AnimationCurve pitchCurve;

	public AnimationCurve volumeCurve;

	public UnityEvent startFiring;

	public UnityEvent onFired;

	public UnityEvent stopFiring;

	private bool winding;

	private float _windUp;

	private bool isCoroutine;
}
