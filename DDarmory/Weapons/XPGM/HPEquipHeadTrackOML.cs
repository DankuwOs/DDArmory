using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using VTOLVR.Multiplayer;

public class HPEquipHeadTrackOML : HPEquipOpticalML
{
	public bool useHeadTrackDefault;

	public VisualTargetFinder visualTargetFinder;
	
	public UnityEvent OnFire = new();
	
	
	private bool _useHeadTrack;
	private string HeadTrack => _useHeadTrack ? "TRUE" : "FALSE";

	private bool _firing;

	private Transform _headTf;
	
	public override void OnEquip()
	{
		base.OnEquip();

		var eqFunctions = equipFunctions.ToList();

		var headTrackEqFunction = new EquipFunction();
		headTrackEqFunction.optionName = "Enable Head Track";
		headTrackEqFunction.optionReturnLabel = HeadTrack;
		headTrackEqFunction.optionEvent += HeadTrackChanged;
		
		eqFunctions.Add(headTrackEqFunction);

		foreach (var equipFunction in eqFunctions.ToArray())
		{
			if (equipFunction.optionName == s_uncageMode)
			{
				eqFunctions.Remove(equipFunction);
			}
		}

		autoUncage = false;
		autoUncageFraction = 0f;
		equipFunctions = eqFunctions.ToArray();
	}

	private string HeadTrackChanged()
	{
		_useHeadTrack = !_useHeadTrack;
		return HeadTrack;
	}

	public override void OnStartFire()
	{
		base.OnStartFire();

		_firing = true;
		
		if (!_useHeadTrack)
			return;
		StartCoroutine(HeadTrackRoutine());
	}

	public override void OnStopFire()
	{
		base.OnStopFire();

		_firing = false;
	}

	public IEnumerator HeadTrackRoutine()
	{
		if (visualTargetFinder == null || weaponManager?.opticalTargeter == null || _headTf == null)
			yield break;

		while (_firing)
		{
			if (visualTargetFinder.attackingTarget)
				weaponManager.opticalTargeter.Lock(visualTargetFinder.attackingTarget.position);
			
			yield return null;
		}
	}
}
