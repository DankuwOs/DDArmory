public class HPEquipJAARM : HPEquipMissileLauncher
{
    protected override void OnEquip()
    {
        base.OnEquip();
        foreach (var internalWeaponBay in weaponManager.GetComponentsInChildren<InternalWeaponBay>(true))
            if (internalWeaponBay.hardpointIdx == hardpointIdx)
            {
                ml.SetInternalWeaponBay(internalWeaponBay);
                ml.openAndCloseBayOnLaunch = true;
                break;
            }
    }

    public override void OnStartFire()
    {
        base.OnStartFire();
        if (ml.missileCount > 0 && weaponManager.gpsSystem.currentGroup.currentTarget != null)
        {
            ((JAARMGuidance)ml.GetNextMissile().guidanceUnit).gpsPoint =
                weaponManager.gpsSystem.currentGroup.currentTarget;
            ml.FireMissile();
        }

        weaponManager.ToggleCombinedWeapon();
    }
}