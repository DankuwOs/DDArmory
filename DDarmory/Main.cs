using System.Collections;
using ModLoader.Framework;
using ModLoader.Framework.Attributes;
using UnityEngine;

namespace DDArmory
{
    [ItemId("danku-ddarmory")]
    public class Main : VtolMod
    {
        
        
        public void Awake()
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
        }

        public override void UnLoad()
        {
            CustomWeaponBase.Main.instance.UnloadPackForName("DD Armory");
        }
    }
}