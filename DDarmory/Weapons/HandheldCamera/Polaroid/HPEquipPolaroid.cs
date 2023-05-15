using System;
using System.Collections;
using UnityEngine;
using VTNetworking;
using VTOLVR.Multiplayer;

public class HPEquipPolaroid : HPEquipHandheldCamera
    {

        [Header("Polaroid")] 
        public GameObject polaroidObject;

        public Transform polaroidParent;

        public Texture remoteTexture;

        private PolaroidObject _lastPolaroid;

        [HideInInspector]
        public bool remote;

        public override void Screenshot()
        {
            base.Screenshot();
            
            StartCoroutine(SendPolaroid());
            if (remote)
                return;
            
            var prevTexture = camera.targetTexture;
            
            camera.targetTexture = _lastPolaroid.renderTexture;
            camera.Render();
            
            camera.targetTexture = prevTexture;
        }

        public IEnumerator SendPolaroid()
        {
            GameObject newPolaroid;
            if (!VTOLMPUtils.IsMultiplayer())
            {
                newPolaroid = Instantiate(polaroidObject, polaroidParent);
            }
            else
            {
                var polaroidInstantiateRequest = VTNetworkManager.NetInstantiate("DDA/PolaroidObj", Vector3.zero, Quaternion.identity, false);
                while (!polaroidInstantiateRequest.isReady)
                    yield return null;
                
                newPolaroid = polaroidInstantiateRequest.obj;
                newPolaroid.transform.parent = polaroidParent;
            }

            var polaroidTransform = newPolaroid.transform;
            polaroidTransform.localPosition = Vector3.zero;
            polaroidTransform.localRotation = Quaternion.identity;

            newPolaroid.SetActive(true);

            _lastPolaroid = newPolaroid.GetComponentInChildren<PolaroidObject>();

            // Create and set the polaroids RT.
            _lastPolaroid.polaroidMaterial = _lastPolaroid.renderer.materials[_lastPolaroid.materialIndex];
            if (!remote)
            {
                _lastPolaroid.renderTexture = new RenderTexture(_lastPolaroid.renderTexture);
                _lastPolaroid.polaroidMaterial.mainTexture = _lastPolaroid.renderTexture;
            }
            else
            {
                _lastPolaroid.polaroidMaterial.mainTexture = remoteTexture;
            }

            // Set rootTf for the polaroid to set to after yoinked and then do the polaroid nyoom out
            _lastPolaroid.rootTf = transform;

            _lastPolaroid.translationToggle.SetDeployed();
        }
    }