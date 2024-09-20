using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using OC;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

    public class HPEquipHandheldCamera : HPEquippable, IMassObject
    {
        [Header("Camera")] public Vector2 resolution;

        public Camera camera;

        public CWB_GrabInteractable interactable;

        public float timeBetweenShots = 0.2f;

        public MinMax fieldOfView;

        [Header("Text")] public RamUsage ramUsage;

        public DriveUsage driveUsage;

        public PicturesTaken imagesTaken;

        public SavingStatus savingStatus;

        [Header("Events")] public GameObject flashObject;

        public UnityEvent OnCapture = new UnityEvent();
    
        public UnityEvent OnFlashEnabled = new UnityEvent();

        public UnityEvent OnFlashDisabled = new UnityEvent();

        private VRHandController _controller;

        private List<byte[]> _heldBuffers = new List<byte[]>();

        private PerformanceCounter _memCounter;

        private Coroutine _screenshotRoutine;

        private Coroutine _flashRoutine;

        private string _imageDirectory;

        private DirectoryInfo _imageDirectoryInfo;

        private bool _capturing;

        private bool _flashEnabled;

        private bool _saving;

        public override void OnEquip()
        {
            base.OnEquip();

            if (!interactable) return;
            var ocCamera = GetComponentInChildren<OverCloudCamera>();
            if (ocCamera == null)
            {
                PlayerVehicleSetup.SetupOCCam(camera.gameObject, true);
            }
        
            interactable.interactable.OnStartInteraction += controller =>
            {
                _controller = controller;
                controller.OnTriggerClicked += OnTriggerClicked;
                controller.OnTriggerClickReleased += OnTriggerClickReleased;

                controller.OnThumbButtonPressed += ToggleFlash;
            };
            interactable.interactable.OnStopInteraction += _ =>
            {
                OnTriggerClickReleased(null); // Ensure camera doesn't continue to screenshot.
                _controller.OnTriggerClicked -= OnTriggerClicked;
                _controller.OnTriggerClickReleased -= OnTriggerClickReleased;

                _controller.OnThumbButtonPressed -= ToggleFlash;

                _controller = null;
            };

            _imageDirectory = $"{VTResources.gameRootDirectory}/Screenshots/DDArmory Camera";

            if (!Directory.Exists(_imageDirectory))
                Directory.CreateDirectory(_imageDirectory);
            _imageDirectoryInfo = new DirectoryInfo(_imageDirectory);

            imagesTaken.AddToCount(_imageDirectoryInfo.GetFiles().Length);
        }

        #region Controls

        public void OnTriggerClicked(VRHandController useless)
        {
            TriggerClick(); // Creating a new method so you can override it without also needing to overriding start.
        }

        public virtual void TriggerClick()
        {
            if (_saving || _controller == null)
                return;

            if (_screenshotRoutine != null)
                return;

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
        
            if (_flashEnabled)
                OnFlashEnabled.Invoke();
            else
                OnFlashDisabled.Invoke();
        }

        #endregion

        private void FixedUpdate()
        {
            if (!_capturing)
                return;

            if (driveUsage && driveUsage.GetDrivePercentUsed() >= 0.95f) // Prevent using too much storage
            {
                _capturing = false;
                SaveImages();
                return;
            }

            if (ramUsage && ramUsage.GetPercentUsed() >= 0.90f) // Prevent using too much ram.
            {
                _capturing = false;
                SaveImages();
            }
        }

        #region Screenshot Stuff


        private IEnumerator ScreenshotRoutine()
        {
            _heldBuffers = new List<byte[]>();
            while (_capturing && _controller != null)
            {
                OnCapture.Invoke();

                yield return StartCoroutine(Screenshot());

                imagesTaken.AddToCount();

                yield return new WaitForSeconds(timeBetweenShots);
            }

            SaveImages();
            _screenshotRoutine = null;
        }

        public virtual IEnumerator Screenshot()
        {
            if (flashObject && _flashEnabled)
            {
                if (_flashRoutine != null)
                    StopCoroutine(_flashRoutine);
                _flashRoutine = StartCoroutine(FlashRoutine());
            }

            var renderTexture =
                RenderTexture.GetTemporary((int)resolution.x, (int)resolution.y, 0, GraphicsFormat.R8G8B8A8_SRGB);

            var prevTexture = camera.targetTexture;

            camera.targetTexture = renderTexture;
            camera.Render();

            AsyncGPUReadback.Request(renderTexture, 0, TextureFormat.RGBA32,
                OnCompleteReadback); // Using AsyncGPUReadback to get the images because it should be *much* more performant than bahas method.

            camera.targetTexture = prevTexture;

            RenderTexture.ReleaseTemporary(renderTexture);
            
            yield break;
        }


        public void RemoteCapture()
        {
            _screenshotRoutine = StartCoroutine(RemoteCaptureRoutine());
        }
        
        private IEnumerator RemoteCaptureRoutine()
        {
            _capturing = true;

            _heldBuffers = new List<byte[]>();
            OnCapture.Invoke();

            yield return StartCoroutine(Screenshot());

            imagesTaken.AddToCount();

            SaveImages();
            _screenshotRoutine = null;

            _capturing = false;
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
            if (_heldBuffers == null || _saving)
                return;


            _saving = true;

            // Animation
            if (savingStatus)
                StartCoroutine(savingStatus.SaveRoutine());

            // Moved to creating a new thread to do all the work. Drastically improves performance.


            ThreadStart start = WriteImages;
            start += () =>
            {
                _heldBuffers = null;
                GC.Collect();
            };

            var thread = new Thread(start) { IsBackground = true };
            thread.Start();
        }

        private void WriteImages()
        {
            foreach (var png in _heldBuffers.Select(heldBuffer =>
                         ImageConversion.EncodeArrayToPNG(heldBuffer, GraphicsFormat.R8G8B8A8_SRGB, (uint)resolution.x,
                             (uint)resolution.y)))
            {
                File.WriteAllBytes(FlybyCameraMFDPage.GetNewScreenshotFilepath(_imageDirectory), png);
            }

            _saving = false;
            if (savingStatus)
                savingStatus.saving = false;

        }

        #endregion

        public void SetFov(float input)
        {
            camera.fieldOfView = fieldOfView.Lerp(input);
        }

        public void RemoteFlash()
        {
            if (!_flashEnabled || !flashObject)
                return;

            
            if (_flashRoutine != null)
                StopCoroutine(_flashRoutine);
            _flashRoutine = StartCoroutine(FlashRoutine());
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