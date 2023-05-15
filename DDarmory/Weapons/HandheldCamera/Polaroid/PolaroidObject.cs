using UnityEngine;

    public class PolaroidObject : MonoBehaviour
    {
        public VRInteractable interactable;

        public MeshRenderer renderer;

        public int materialIndex;

        public string fadePropertyName;

        [Tooltip("Multiplies the velocity magnitude by this amount. Fade is 0-1.")]
        public float fadeMultiplier;

        public string velocityPropertyName;

        public float velocityLerpSpeed;

        public TranslationToggle translationToggle;

        public RenderTexture renderTexture;

        public float lifetime;

        [HideInInspector] public bool isRemote;

        [HideInInspector]
        public Transform rootTf;

        [HideInInspector]
        public Material polaroidMaterial;

        private Vector3 lastPosition;

        private Vector3 lastVelocity;

        private bool yoinked;

        private float fadeValue;

        private float time;

        private void Start()
        {
            interactable.OnStartInteraction += controller => Yoinked();
        }

        private void Update()
        {
            if (!yoinked)
                return;
            
            var position = transform.localPosition;

            var velocity = position - lastPosition;

            var relativeVelocity = transform.InverseTransformDirection(rootTf.TransformDirection(velocity)); // Can't use world space position because reasons.
            
            relativeVelocity = Vector3.Lerp(lastVelocity, relativeVelocity, velocityLerpSpeed * Time.deltaTime);

            foreach (var rendererMaterial in renderer.materials)
            {
                if (!rendererMaterial.HasProperty(velocityPropertyName)) continue;
                
                rendererMaterial.SetVector(velocityPropertyName, relativeVelocity);
            }

            if (fadeValue <= 1)
            {
                fadeValue += velocity.magnitude * fadeMultiplier;
                polaroidMaterial.SetFloat(fadePropertyName, fadeValue);
            }

            time += Time.deltaTime;
            if (time > lifetime)
                Destroy(gameObject);

            lastPosition = position;
            lastVelocity = relativeVelocity;
        }

        public void Yoinked()
        {
            if (yoinked)
                return;

            yoinked = true;
            transform.SetParent(rootTf, true);
            
            lastPosition = transform.localPosition; 
        }
    }