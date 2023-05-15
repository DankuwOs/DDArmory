public class WingVesselRB : CWB_HPEquipExtension
{
    public override void OnEquip()
    {
        base.OnEquip();

        if (!hpEquip.weaponManager)
            return;
        
        foreach (var wing in GetComponentsInChildren<Wing>(true))
        {
            wing.SetParentRigidbody(hpEquip.weaponManager.vesselRB);
            wing.enabled = true;
        }
    }
}