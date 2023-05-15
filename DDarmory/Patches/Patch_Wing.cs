
/*
using Harmony;
using UnityEngine;
[HarmonyPriority(Priority.First)]
[HarmonyPatch(typeof(Wing), nameof(Wing.BPU_FixedUpdate))]
public class Patch_Wing
{
	public static bool Prefix(Wing __instance, Transform ___wingTransform, Vector3 ___rotorVelocity, ref WindMaster ___windMaster, 
		float ___liftConstant, float ___dragConstant, ref float ___currDynamicSweep, bool ___useOverrideLiftDir,  float ___buffetRand,
		bool ___hasSoundEffects, bool ___dragCamShake, CamRigRotationInterpolator ___camRigShaker, float ___dragShakeFactor, ref int ___shakeDir)
	{
		
		Debug.Log($"[DDA WingPatch]: RB Null? {__instance.rb == null}");
		if (!__instance.rb.isKinematic)
		{
			var wingTraverse = Traverse.Create(__instance);
			var ___wingAirspeed = wingTraverse.Property("wingAirspeed");
			var ___currentDragForce = wingTraverse.Property("currentDragForce");
			var ___currentLiftForce = wingTraverse.Property("currentLiftForce");
			
			Debug.Log($"[DDA WingPatch]: RB not kinematic");
			Vector3 vector = __instance.rb.worldCenterOfMass +
			                 __instance.rb.transform.TransformVector(__instance.manualOffset);
			Debug.Log($"[DDA WingPatch]: Vector {vector}");
			Vector3 up = ___wingTransform.up;
			Debug.Log($"[DDA WingPatch]: Up {up}");
			Quaternion quaternion = Quaternion.identity;
			if (__instance.usePhaseLag)
			{
				Vector3 vector2 = __instance.rb.worldCenterOfMass + (__instance.phaseLagAxis.position -
				                                                     __instance.rb.transform.TransformPoint(
					                                                     __instance.rb.centerOfMass));
				Vector3 vector3 = vector - vector2;
				quaternion = Quaternion.AngleAxis(__instance.phaseLagAngle, __instance.phaseLagAxis.forward);
				vector = vector2 + quaternion * vector3;
			}

			Vector3 vector4;
			if (__instance.usePointVelocity)
			{
				vector4 = __instance.rb.GetPointVelocity(vector);
				Debug.Log($"[DDA WingPatch]: PointVelocity {vector4}");
			}
			else
			{
				vector4 = __instance.rb.velocity;
				Debug.Log($"[DDA WingPatch]: NotPV {vector4}");
			}

			if (__instance.useRotorVelocity)
			{
				vector4 += ___rotorVelocity;
			}

			if (WindVolumes.windEnabled)
			{
				Debug.Log($"[DDA WingPatch]: WindEnabled");
				if (!___windMaster)
				{
					Debug.Log($"[DDA WingPatch]: No WindMaster");
					___windMaster = __instance.rb.gameObject.GetComponent<WindMaster>();
					if (!___windMaster)
					{
						___windMaster = __instance.rb.gameObject.AddComponent<WindMaster>();
						Debug.Log($"[DDA WingPatch]: No WindMaster");
					}
				}

				vector4 -= ___windMaster.wind;
				Debug.Log($"[DDA WingPatch]: Vector4 Wind {vector4}");
			}

			float sqrMagnitude = vector4.sqrMagnitude;
			float num = Mathf.Sqrt(sqrMagnitude);
			___wingAirspeed.SetValue(num);
			Debug.Log($"[DDA WingPatch]: WingAirspeed {___wingAirspeed.GetValue()}");
			if (!__instance.cullAtMinAirspeed || sqrMagnitude > 400f)
			{
				Debug.Log($"[DDA WingPatch]: Doing Wing Stuff");
				float num2 = AerodynamicsController.fetch.AtmosDensityAtPosition(vector);
				Debug.Log($"[DDA WingPatch]: Atmos Pos {num2}");
				float num3 = Vector3.Angle(up, vector4) - 90f;
				__instance.currAoA = num3;
				Debug.Log($"[DDA WingPatch]: Curr AOA {num3}");
				float num4 = Mathf.Abs(num3);
				float num5 = num2 * sqrMagnitude;
				float num6 = (__instance.aeroProfile.mirroredCurves
					? __instance.aeroProfile.liftCurve.Evaluate(num4)
					: __instance.aeroProfile.liftCurve.Evaluate(num3));
				Debug.Log($"[DDA WingPatch]: Lift {num6}");
				float num7 = ___liftConstant * num5 * Mathf.Sign(num3) * num6;
				float num8 = (__instance.aeroProfile.mirroredCurves
					? __instance.aeroProfile.dragCurve.Evaluate(num4)
					: __instance.aeroProfile.dragCurve.Evaluate(num3));
				Debug.Log($"[DDA WingPatch]: Drag {num8}");
				float num9 = ___dragConstant * num5 * num8;
				float num10 = __instance.CalculateSweep(vector4);
				Debug.Log($"[DDA WingPatch]: Sweep {num10}");
				if (__instance.debugSweep)
				{
					___currDynamicSweep = num10;
					Debug.Log($"[DDA WingPatch]: Debug Sweep Thing {___currDynamicSweep}");
				}

				num9 *= AerodynamicsController.fetch.DragMultiplierAtSpeedAndSweep(num,
					WaterPhysics.GetAltitude(vector), num10);
				Debug.Log($"[DDA WingPatch]: DragMult {num9}");
				Vector3 vector5;
				if (___useOverrideLiftDir)
				{
					vector5 = __instance.overrideLiftDir.transform.forward;
				}
				else
				{
					vector5 = (__instance.aeroProfile.perpLiftVector
						? Vector3.Cross(vector4, Vector3.Cross(up, vector4))
						: up);
				}
				Debug.Log($"[DDA WingPatch]: Something");

				__instance.dragVector = num9 * vector4.normalized;
				__instance.liftVector = num7 * vector5.normalized;
				Debug.Log($"[DDA WingPatch]: Drag Lift Stuff");
				Vector3 vector6 = __instance.dragVector + __instance.liftVector;
				if (__instance.usePhaseLag)
				{
					vector6 = quaternion * (__instance.liftVector + __instance.dragVector);
				}
				Debug.Log($"[DDA WingPatch]: AADADADADADA");

				___currentLiftForce.SetValue(num7);
				___currentDragForce.SetValue(num9);
				if (__instance.useBuffet)
				{
					Debug.Log($"[DDA WingPatch]: Buffet Time");
					Vector3 vector7 =
						VectorUtils.FullRangePerlinNoise(___buffetRand,
							Time.time * sqrMagnitude * __instance.aeroProfile.buffetTimeFactor) *
						__instance.aeroProfile.buffetCurve.Evaluate(num3) * __instance.aeroProfile.buffetMagnitude *
						sqrMagnitude * __instance.liftArea * up;
					vector6 += vector7;
					Debug.Log($"[DDA WingPatch]: End Buffet");
				}

				__instance.rb.AddForceAtPosition(vector6, vector);
				Debug.Log($"[DDA WingPatch]: RB AddForce");
				if (___hasSoundEffects)
				{
					Debug.Log($"[DDA WingPatch]: Has Sounds");
					float num11 = num7 + 2f * num9;
					for (int i = 0; i < __instance.soundEffects.Length; i++)
					{
						Debug.Log($"[DDA WingPatch]: Update Sound");
						__instance.soundEffects[i].Update(num11, num);
					}
				}
				Debug.Log($"[DDA WingPatch]: End Sound Stuff");
				if (___dragCamShake && ___camRigShaker)
				{
					Debug.Log($"[DDA WingPatch]: Shake Time");
					Vector3 vector8 = (float)___shakeDir * ___dragShakeFactor * __instance.dragVector;
					___shakeDir *= -1;
					CamRigRotationInterpolator.ShakeAll(vector8);
					return false;
				}
			}
			else
			{
				Debug.Log($"[DDA WingPatch]: Zero time...");
				__instance.dragVector = Vector3.zero;
				__instance.liftVector = Vector3.zero;
			}
		}

		return false;
	}
}*/