using System.Collections.Generic;
using System.Linq;
using DDArmory.Weapons.Utils;
using VTOLVR.DLC.EW;

namespace DDArmory.Weapons.Loitering.ALTIUS;

public class HPEquipALTIUS : HPEquipDecoyMissile
{
    // COLD = COLD
    // DRFM = KILL
    // NOISE = INTERCEPT
    // DECOY = DECOY

    public InternalWeaponBayHP iwbHP;

    public override void Awake()
    {
        base.Awake();
        if (iwbHP)
        {
            ml.iwb = iwbHP;
        }
    }

    public override void FireMissile()
    {
        var missile = ml.GetNextMissile();
        if (missile == null)
        {
            base.FireMissile();
            return;
        }

        var guidance = missile.guidanceUnit as ALTIUSGuidance;

        if (guidance.decoyTransmitMode == AirLaunchedDecoyGuidance.TransmitModes.NOISE)
        {
            List<ModuleRWR.RWRContact> contacts = new List<ModuleRWR.RWRContact>();
            foreach (var moduleRwr in weaponManager.actor.rwrs)
            {
                contacts.AddRange(moduleRwr.contacts);
            }

            guidance.rwrContacts = contacts;
        }
        
        base.FireMissile();
    }
}