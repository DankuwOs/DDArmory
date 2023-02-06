using UnityEngine;

    public class HPEquipPolaroid : HPEquipHandheldCamera
    {

        [Header("Polaroid")] 
        public GameObject polaroidObject;

        public Transform polaroidParent;

        private PolaroidObject lastPolaroid;

        public override void Screenshot()
        {
            base.Screenshot();
            
            SendPolaroid();
        
            var prevTexture = camera.targetTexture;
        
            camera.targetTexture = lastPolaroid.renderTexture;
            camera.Render();

            camera.targetTexture = prevTexture;
        }

        private void SendPolaroid()
        {
            var newPolaroid = Instantiate(polaroidObject, polaroidParent);
            var polaroidTransform = newPolaroid.transform;
            polaroidTransform.localPosition = Vector3.zero;
            polaroidTransform.localRotation = Quaternion.identity;

            newPolaroid.SetActive(true);
        
            lastPolaroid = newPolaroid.GetComponentInChildren<PolaroidObject>();
            
            // Create and set the polaroids RT.
            lastPolaroid.polaroidMaterial = lastPolaroid.renderer.materials[lastPolaroid.materialIndex];

            lastPolaroid.renderTexture = new RenderTexture(lastPolaroid.renderTexture);
            lastPolaroid.polaroidMaterial.mainTexture = lastPolaroid.renderTexture;
            
            // Set rootTf for the polaroid to set to after yoinked and then do the polaroid nyoom out
            lastPolaroid.rootTf = transform;
        
            lastPolaroid.translationToggle.SetDeployed();
        }
    }