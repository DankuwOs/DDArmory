using System.Collections;
using HarmonyLib;
using UnityEngine;

// Token: 0x0200001A RID: 26
public class OmlAimPoint : OpticalMissileLauncher
{
	// Token: 0x0600006F RID: 111 RVA: 0x00004438 File Offset: 0x00002638
	public IEnumerator NewUpdateRoutine()
	{
		var flag = !fwdObj;
		IEnumerator result;
		if (flag)
		{
			result = base.UpdateRoutine();
		}
		else
		{
			var traverse = Traverse.Create(this);
			while (weaponEnabled && !htOml._tgt && htOml._headTracking)
			{
				var vector = fwdObj.transform.forward * 1000f;
				traverse.Field("fwdLocalAimPos").SetValue(vector);
			}
			result = base.UpdateRoutine();
		}
		return result;
	}

	// Token: 0x06000070 RID: 112 RVA: 0x000044D8 File Offset: 0x000026D8
	public override bool TryFireMissile()
	{
		bool flag = htOml;
		if (flag)
		{
			htOml.OnFire.Invoke();
			htOml._tgt = null;
			Debug.Log("[CWB] HT OML Fired, is tgt null? " + ((htOml._tgt == null) ? "Yes" : "No"));
		}
		return base.TryFireMissile();
	}

	// Token: 0x04000091 RID: 145
	public Transform fwdObj;

	// Token: 0x04000092 RID: 146
	public HPEquipHeadTrackOML htOml;
}
