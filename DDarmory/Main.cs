using System;
using System.Collections;
using DDArmory.Weapons.Utils;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine;

namespace DDArmory.Mainer
{
    [ItemId("danku-ddarmory")]
    public class Main : VtolMod
    {
        private void Awake()
        {
        }

        /*public void Awake()
        {
            Debug.Log($"[DDA]: Waiting for me to be loaded...");
            StartCoroutine(OnAwake());
        }

        private IEnumerator OnAwake()
        {
            yield return new WaitUntil(IsItemLoaded);
            Debug.Log($"[DDA]: Trying to load my pack");
            CustomWeaponBase.Main.instance.LoadPackForName("DD Armory");
        }

        private bool IsItemLoaded()
        {
            bool itemLoaded = ModLoader.ModLoader.Instance.IsItemLoaded(CustomWeaponBase.Main.instance.GetDirectoryForName("DD Armory"));
            return itemLoaded;
        }*/

        public override void UnLoad()
        {
            CustomWeaponBase.Main.instance.UnloadPackForName("DD Armory");
        }
    }
}