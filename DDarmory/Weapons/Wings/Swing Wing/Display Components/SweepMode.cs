
    using System.Collections;
    using UnityEngine;

    public class SweepMode : MonoBehaviour
    {
        public Vector3 hidden;

        public Vector3 visible;

        public Transform modeTf;
    
        [ColorUsage(false, true)] public Color albedoColor;
    
        [ColorUsage(false, true)] public Color emissiveColor;

        public float lerpSpeed = 1f;

        private Coroutine _coroutine;

        public void SetVisibility(bool hide)
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(ToggleVisibility(hide));
        } 

        private IEnumerator ToggleVisibility(bool hide)
        {
            var t = 0f;
            while (t <= 1)
            {
                modeTf.localPosition = Vector3.Lerp(modeTf.localPosition, hide ? hidden : visible, t);

                t += Time.deltaTime * lerpSpeed;
                yield return null;
            }
            
        }
    }