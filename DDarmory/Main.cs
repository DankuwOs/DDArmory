using Harmony;

namespace DDArmory
{
    public class Main : VTOLMOD
    {
        public override void ModLoaded()
        {
            var harmonyInstance = HarmonyInstance.Create("danku.ddarmory");
            harmonyInstance.PatchAll();

            base.ModLoaded();
        }
    }
}