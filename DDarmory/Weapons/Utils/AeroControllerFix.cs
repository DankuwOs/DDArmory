using HarmonyLib;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Jobs;

namespace DDArmory.Weapons.Utils
{
    public class DD_AeroControllerFix : MonoBehaviour
    {
        public static void AeroControllerFix(AeroController aeroController)
        {
            // After the recent 1.7 update the aerocontroller is now multithreaded, i could just do aeroController.OnDisable And OnEnable
            // to do the same thing and just set initial dispatch to true, but i already made this.

            var controllerTraverse = Traverse.Create(aeroController);

            var nativeCstTraverse = controllerTraverse.Field("nativeCST");
            var transformAccessTraverse = controllerTraverse.Field("transformAccess");
            var parallelOutputTraverse = controllerTraverse.Field("parallelOutput");

            if (nativeCstTraverse != null)
            {
                nativeCstTraverse.GetValue<NativeArray<AeroController.NativeCST>>().Dispose();

                nativeCstTraverse.SetValue(new NativeArray<AeroController.NativeCST>(
                    aeroController.controlSurfaces.Length,
                    Allocator.Persistent));
            }

            if (transformAccessTraverse != null)
            {
                transformAccessTraverse.GetValue<TransformAccessArray>().Dispose();

                var transformAccessArray = new TransformAccessArray(aeroController.controlSurfaces.Length);
                foreach (var tf in aeroController.controlSurfaces)
                {
                    transformAccessArray.Add(tf.transform);
                }

                transformAccessTraverse.SetValue(transformAccessArray);
            }

            if (parallelOutputTraverse != null)
            {
                parallelOutputTraverse.GetValue<NativeArray<float>>().Dispose();

                var parallelOutput =
                    new NativeArray<float>(aeroController.controlSurfaces.Length, Allocator.Persistent);
                for (int j = 0; j < parallelOutput.Length; j++)
                {
                    parallelOutput[j] = 0.5f;
                }

                parallelOutputTraverse.SetValue(parallelOutput);
            }

            controllerTraverse.Field("initialDispatch").SetValue(true);
        }
    }
}