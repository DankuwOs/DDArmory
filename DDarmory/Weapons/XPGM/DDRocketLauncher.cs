using System.Collections;
using UnityEngine;

public class DDRocketLauncher : RocketLauncher
{
	public override bool FireRocket()
	{
		if (!base.FireRocket()) return false;
		
		if (salvoCount == 1) return true;
		
		StartCoroutine(Rotate());
		fireCount++;
		return true;
	}

	public override void OnDisableWeapon()
	{
		base.OnDisableWeapon();
		if (salvoCount == 1)
		{
			StartCoroutine(Rotate());
			fireCount++;
		}
	}

	private IEnumerator Rotate()
	{
		float rotationAmount = rotation;
		if (fireCount >= secondaryRotationNumber && doSecondaryRotation)
		{
			rotationAmount = secondaryRotation;
		}
		
		Quaternion targetRotation = Quaternion.Euler(rotaryTransform.localRotation.eulerAngles + new Vector3(0f, 0f, rotationAmount));
		float rotateTime = 0f;
		while (true)
		{
			rotaryTransform.localRotation = Quaternion.Euler(Vector3.Lerp(rotaryTransform.localRotation.eulerAngles,
				targetRotation.eulerAngles, rotationCurve.Evaluate(rotateTime / rotationTime)));

			if (rotateTime >= rotationTime)
			{
				break;
			}

			rotateTime += Time.deltaTime;
			yield return null;

		}

		yield break;
	}
	
	[Header("Rotary Options")]
	public bool DoRotary;

	public Transform rotaryTransform;

	public float rotation;

	public bool doSecondaryRotation;

	[Tooltip("This and any numbers above will use secondary rotation")]
	public int secondaryRotationNumber;

	public float secondaryRotation;

	public float rotationTime;

	public AnimationCurve rotationCurve;

	private int fireCount = 1;
}
