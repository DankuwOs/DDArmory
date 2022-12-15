
    using UnityEngine;

    public class SweepBar : MonoBehaviour
    {
        public Vector3 axis;

        public Transform bar;
    
        public Color albedoColor;
    
        public Color emissiveColor;
    
        public void SetScale(float scale) => bar.localScale = (Vector3.one - axis * Mathf.Clamp01(scale));
    }