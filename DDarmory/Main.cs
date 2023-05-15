using Harmony;

namespace DDArmory
{
    public class Main : VTOLMOD
    {
        public static Main instance;
        
        public override void ModLoaded()
        {
            var harmonyInstance = HarmonyInstance.Create("danku.ddarmory");
            harmonyInstance.PatchAll();

            base.ModLoaded();

            instance = this;
        }
    }
}