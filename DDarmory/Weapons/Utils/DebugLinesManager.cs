using UnityEngine;

	public class DebugLinesManager
	{
		// Token: 0x0600002B RID: 43 RVA: 0x00002D00 File Offset: 0x00000F00
		public static LineRenderer LineRendererToPoint(Vector3 origin, Vector3 target, LineRenderer renderer)
		{
			Vector3[] positions = {
				origin,
				target
			};
			renderer.SetPositions(positions);
			return renderer;
		}
	}