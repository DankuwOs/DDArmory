using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling.Experimental;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace DDArmory.Weapons.ScreenshotGun
{
    public class HPEquipScreenshotGun : HPEquippable, IMassObject
    {
        [Header("Camera")]
        public Vector2 resolution;

        public Camera camera;

        public GrabInteractable interactable;

        public float timeBetweenShots = 0.2f;

        public MinMax fieldOfView;
        
        [Header("Text")]
        public RamUsage ramUsage;

        public DriveUsage driveUsage;

        public PicturesTaken imagesTaken;

        public SavingStatus savingStatus;
        
        [Header("Events")]
        public GameObject flashObject;
        
        public UnityEvent OnCapture = new UnityEvent();

        public UnityEvent<bool> OnSetFlash = new UnityEvent<bool>();

        private VRHandController _controller = null;

        private List<byte[]> _heldBuffers = new List<byte[]>();

        private PerformanceCounter _memCounter;

        private Coroutine _screenshotRoutine;
        
        private readonly string _imageDirectory = $"{VTResources.gameRootDirectory}/Screenshots/DDArmory Camera";

        private DirectoryInfo _imageDirectoryInfo;

        private bool _capturing;

        private bool _flashEnabled;

        private bool _saving;

        protected override void OnEquip()
        {
            base.OnEquip();

            if (!interactable) return;
            
            
            interactable.interactable.OnStartInteraction += controller =>
            {
                _controller = controller;
                controller.OnTriggerClicked += OnTriggerClicked;
                controller.OnTriggerClickReleased += OnTriggerClickReleased;

                controller.OnSecondaryThumbButtonPressed += ToggleFlash;
            };
            interactable.interactable.OnStopInteraction += _ =>
            {
                OnTriggerClickReleased(null); // Ensure camera doesn't continue to screenshot.
                _controller.OnTriggerClicked -= OnTriggerClicked;
                _controller.OnTriggerClickReleased -= OnTriggerClickReleased;
                
                _controller.OnSecondaryThumbButtonPressed -= ToggleFlash;
                    
                _controller = null;
            };

            if (!Directory.Exists(_imageDirectory))
                Directory.CreateDirectory(_imageDirectory);
            _imageDirectoryInfo = new DirectoryInfo(_imageDirectory);
            
            imagesTaken.AddToCount(_imageDirectoryInfo.GetFiles().Length);
        }

        #region Controls
        
        public void OnTriggerClicked(VRHandController useless)
        {
            if (_saving || _controller == null)
                return;
            
            if (_screenshotRoutine != null)
                StopCoroutine(_screenshotRoutine);

            _capturing = true;
            
            _screenshotRoutine = StartCoroutine(ScreenshotRoutine());
        }

        public void OnTriggerClickReleased(VRHandController useless)
        {
            if (_screenshotRoutine == null) return;

            _capturing = false;
        }

        public void ToggleFlash(VRHandController useless)
        {
            _flashEnabled = !_flashEnabled;
            OnSetFlash.Invoke(!_flashEnabled);
        }
        
        #endregion

        private void FixedUpdate()
        {
            if (!_capturing)
                return;
            
            if (driveUsage && driveUsage.GetDrivePercentUsed() >= 0.85f) // Prevent using too much storage
            {
                _capturing = false;
                SaveImages();
                return;
            }
            
            if (ramUsage && ramUsage.GetPercentUsed() >= 0.85f) // Prevent using too much ram.
            {
                _capturing = false;
                SaveImages();
                return;
            }
        }

        #region Screenshot Stuff


        private IEnumerator ScreenshotRoutine()
        {
            while (_capturing && _controller != null)
            {
                OnCapture.Invoke();
                
                Screenshot();

                imagesTaken.AddToCount();
                
                yield return new WaitForSeconds(timeBetweenShots);
            }
            
            SaveImages();
        }

        private void Screenshot()
        {
            
            
            if (flashObject && _flashEnabled)
                StartCoroutine(FlashRoutine());

            var renderTexture =
                RenderTexture.GetTemporary((int)resolution.x, (int)resolution.y, 0, GraphicsFormat.R8G8B8A8_SRGB);

            var prevTexture = camera.targetTexture;

            camera.targetTexture = renderTexture;
            camera.Render();

            AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBA32, OnCompleteReadback); // Using AsyncGPUReadback to get the images because it should be *much* more performant than bahas method.

            camera.targetTexture = prevTexture;

            RenderTexture.ReleaseTemporary(renderTexture);
        }
        
        private void OnCompleteReadback(AsyncGPUReadbackRequest readbackRequest)
        {
            if (readbackRequest.hasError)
            {
                return;
            }

            _heldBuffers.Add(readbackRequest.GetData<byte>().ToArray());
        }

        private void SaveImages()
        {
            if (_heldBuffers.Count == 0 || _saving)
                return;
            
            
            _saving = true;
            
            // Animation
            if (savingStatus) 
                StartCoroutine(savingStatus.SaveRoutine());

            // Moved to creating a new thread to do all the work. Drastically improves performance.

            var thread = new Thread(WriteImages);
            thread.Start();
        }

        private void WriteImages()
        {
            foreach (var png in _heldBuffers.Select(heldBuffer => ImageConversion.EncodeArrayToPNG(heldBuffer, GraphicsFormat.R8G8B8A8_SRGB, (uint)resolution.x, (uint)resolution.y)))
            {
                File.WriteAllBytes(FlybyCameraMFDPage.GetNewScreenshotFilepath(_imageDirectory), png);
            }
            
            _heldBuffers.Clear();
            GC.Collect();
            
            _saving = false; 
            if (savingStatus) 
                savingStatus.saving = false;
        }
        
        #endregion

        public void SetFov(float input)
        {
            camera.fieldOfView = fieldOfView.Lerp(input);
        }

        private IEnumerator FlashRoutine()
        {
            flashObject.SetActive(true);
            yield return null;
            yield return null;
            flashObject.SetActive(false);
        }

        public override void OnDestroy()
        {
            SaveImages();
            
            base.OnDestroy();
        }

        public float GetMass()
        {
            return 0;
        }
    }
}