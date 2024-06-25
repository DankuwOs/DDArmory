using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public class HPEquipHeadTrackOML : HPEquipOpticalML
{
	public override void OnEquip()
	{
		oml = (OpticalMissileLauncher)ml;
		var joysticks = weaponManager.GetComponent<VehicleControlManifest>().joysticks;
		foreach (var vrjoystick in joysticks)
		{
			vrjoystick.OnThumbstickButtonDown.AddListener(delegate
			{
				if (itemActivated)
				{
					_headTracking = !_headTracking;
				}
			});
		}
	}

	public override void OnStartFire()
	{
		base.OnStartFire();
		_firing = true;
	}

	public override void OnStopFire()
	{
		base.OnStopFire();
		_firing = false;
		_tgt = null;
	}

	private void Update()
	{
		if(!itemActivated || !visualTargetFinder || visualTargetFinder.targetsSeen.Count == 0 || !_firing || !_headTracking || !weaponManager.opticalTargeter.powered || _tgt)
			return;
		
			var source = from a in visualTargetFinder.targetsSeen
			orderby Vector3.Distance(visualTargetFinder.fovReference.position, a.position)
			select a;
			_tgt = source.First();
			weaponManager.opticalTargeter.Lock(_tgt.position);
	}

	[Header("Head Tracking")]
	public VisualTargetFinder visualTargetFinder;

	public UnityEvent OnFire = new UnityEvent();

	[NonSerialized]
	public bool _headTracking = true;

	private bool _firing;

	[NonSerialized]
	public Actor _tgt;
}
