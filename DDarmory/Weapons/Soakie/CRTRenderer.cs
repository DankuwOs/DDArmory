using UnityEngine;


    public class CRTRenderer : MonoBehaviour
    {
        public Camera cam;
    
        public Renderer targetRenderer;

        public IntVector2 resolution;

        [HideInInspector] public bool crtEnabled;

        private CustomRenderTexture _rt;

        private Vector2 _crtScaling;

        private float _defaultBrightness;
    
        private void Start()
        {
            cam.forceIntoRenderTexture = true; // do i need?
            _rt = new CustomRenderTexture(resolution.x, resolution.y);
            _rt.Create();
            cam.targetTexture = _rt;
        
            _crtScaling = new Vector2(resolution.x, resolution.y);
        
            targetRenderer.material.SetTextureScale("_CRT", _crtScaling);
            targetRenderer.material.SetTexture("_Albedo", _rt);

            _defaultBrightness = targetRenderer.material.GetFloat("_Brightness");
        }

        public void SetDisplay(bool toggle, bool changeState = true)
        {
            targetRenderer.material.SetFloat("_Brightness", toggle ? _defaultBrightness : 0f);
            if (changeState)
            {
                crtEnabled = toggle;
            }
        }
    }
