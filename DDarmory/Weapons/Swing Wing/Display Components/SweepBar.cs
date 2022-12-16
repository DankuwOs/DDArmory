
    using UnityEngine;

    public class SweepBar : MonoBehaviour
    {
        public Vector3 axis;

        public Transform bar;

        [ColorUsage(false, true)] public Color albedoColor;
    
        [ColorUsage(false, true)] public Color emissiveColor;
    
        public void SetScale(float scale) => bar.localScale = (Vector3.one - axis * Mathf.Clamp01(scale));
    }