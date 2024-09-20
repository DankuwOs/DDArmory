using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using VTNetworking;
using VTOLVR.Multiplayer;

public class HPEquipPolaroid : HPEquipHandheldCamera
    {

        [Header("Polaroid")] 
        public GameObject polaroidObject;

        public Transform polaroidParent;

        public PolaroidSync polaroidSync;

        public PolaroidSyncMulticrew polaroidSyncMulticrew;
        

        private PolaroidObject _lastPolaroid;

        [HideInInspector]
        public bool remote;

        public override IEnumerator Screenshot()
        {
            yield return base.Screenshot();
            
            yield return StartCoroutine(SendPolaroid());
            
            if (remote)
                yield break;
            
            var prevTexture = camera.targetTexture;

            if (!_lastPolaroid)
            {
                Debug.Log($"[HPEquipPolaroid]: ??!? _lastPolaroid null!?!?!? OWO");
            }
            
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
                var polaroidInstantiateRequest =
                    VTNetworkManager.NetInstantiate("DDA/PolaroidObj", Vector3.zero, Quaternion.identity, false);
                while (!polaroidInstantiateRequest.isReady)
                    yield return null;

                newPolaroid = polaroidInstantiateRequest.obj;
                newPolaroid.transform.parent = polaroidParent;

                if (polaroidSync)
                    polaroidSync.SendRPC("RPC_SendPolaroid", newPolaroid.GetComponent<VTNetEntity>().entityID);

                if (polaroidSyncMulticrew)
                    polaroidSyncMulticrew.SendRPC("RPC_SendPolaroid", newPolaroid.GetComponent<VTNetEntity>().entityID);
            }

            var polaroidTransform = newPolaroid.transform;
            polaroidTransform.localPosition = Vector3.zero;
            polaroidTransform.localRotation = Quaternion.identity;

            newPolaroid.SetActive(true);

            _lastPolaroid = newPolaroid.GetComponentInChildren<PolaroidObject>();

            // Create and set the polaroids RT.
            _lastPolaroid.polaroidMaterial = _lastPolaroid.renderer.materials[_lastPolaroid.materialIndex];
            
            _lastPolaroid.renderTexture = new RenderTexture(_lastPolaroid.renderTexture);
            _lastPolaroid.polaroidMaterial.mainTexture = _lastPolaroid.renderTexture;

            // Set rootTf for the polaroid to set to after yoinked and then do the polaroid nyoom out
            _lastPolaroid.rootTf = transform;

            _lastPolaroid.translationToggle.SetDeployed();
        }
    }