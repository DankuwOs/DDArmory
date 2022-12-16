﻿using Harmony;

// Token: 0x02000011 RID: 17
[HarmonyPatch(typeof(Hitbox), "Damage")]
public class Patch_Health_Damage
{
    // Token: 0x0600004C RID: 76 RVA: 0x00003A8C File Offset: 0x00001C8C
    public static void Prefix(Hitbox __instance, float damage)
    {
        var flag = !__instance.actor.weaponManager;
        if (!flag)
            for (var i = 0; i < __instance.actor.weaponManager.equipCount; i++)
            {
                var equip = __instance.actor.weaponManager.GetEquip(i);
                var flag2 = equip is HPEquipSoakieSystem;
                if (flag2)
                {
                    var hpequipSoakieSystem = equip as HPEquipSoakieSystem;
                    hpequipSoakieSystem.OnDamage(__instance, damage);
                }
            }
    }
}