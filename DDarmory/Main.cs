using Harmony;

namespace DDArmory
{
	public class Main : VTOLMOD
	{
		public override void ModLoaded()
		{
			HarmonyInstance harmonyInstance = HarmonyInstance.Create("danku.ddarmory");
			harmonyInstance.PatchAll();
			
			
			base.ModLoaded();
		}
	}
}
