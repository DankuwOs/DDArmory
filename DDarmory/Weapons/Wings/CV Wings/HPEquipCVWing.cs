using System.Collections;
using System.Linq;
using UnityEngine;

namespace DDArmory.Weapons.Wings.CV_Wings
{
    public class HPEquipCVWing : HPEquipWing
    {
        [Tooltip("Path to canopy switch separated by '/'")]
        public string canopySwitchPath;

        public CanopyAnimator canopyAnimator;

        [Tooltip("There's an issue where if the particles use a render host that is disabled the particles wont update. Put those here")]
        public string[] wingVaporParticles;


        private TiltController _tiltController;

        private VRLever _canopyLever;

        public override void OnConfigDetach(LoadoutConfigurator configurator)
        {
            base.OnConfigDetach(configurator);
            
            var wm = configurator ? configurator.wm : weaponManager;
            if (!wm)
                return;

            var vehicleInputManager = wm.GetComponent<VehicleInputManager>();
            vehicleInputManager.tiltController = _tiltController;
            
            foreach (var vaporObject in wingVaporParticles)
            {
                var tf = wm.transform.Find(vaporObject);

                if (!tf || !tf.gameObject.GetComponent<WingVaporParticles>())
                    continue;

                var vaporParticles = tf.gameObject.GetComponent<WingVaporParticles>();
                vaporParticles.StopCoroutine(vaporParticles.UpdateRoutine());
                OnWillRenderObjectHost.GetHost(vaporParticles.renderHost).RegisterScript(vaporParticles);
            }
            
            var switchTf = wm.transform.Find(canopySwitchPath);

            if (!switchTf)
            {
                return;
            }

            _canopyLever = switchTf.GetComponent<VRLever>();
                
            _canopyLever.OnSetState.RemoveListener(SetCanopyState);
            
            
            AudioController.instance.SetExteriorOpening($"canopy_{GetInstanceID()}", 0f);
        }

        public override void OnUnequip()
        {
            base.OnUnequip();
            var vehicleInputManager = weaponManager.GetComponent<VehicleInputManager>();
            vehicleInputManager.tiltController = _tiltController;
            
            foreach (var vaporObject in wingVaporParticles)
            {
                var tf = weaponManager.transform.Find(vaporObject);
                    
                if (!tf || !tf.gameObject.GetComponent<WingVaporParticles>())
                    continue;

                var vaporParticles = tf.gameObject.GetComponent<WingVaporParticles>();
                vaporParticles.StopCoroutine(vaporParticles.UpdateRoutine());
                OnWillRenderObjectHost.GetHost(vaporParticles.renderHost).RegisterScript(vaporParticles);
            }
            
            var switchTf = weaponManager.transform.Find(canopySwitchPath);

            if (!switchTf)
            {
                Debug.Log($"[CVWings_OnUnequip]: Switch tf null");
                return;
            }

            _canopyLever = switchTf.GetComponent<VRLever>();
            
            _canopyLever.OnSetState.RemoveListener(SetCanopyState);
            
            AudioController.instance.SetExteriorOpening($"canopy_{GetInstanceID()}", 0f);
        }
        

        public override void Initialize(LoadoutConfigurator configurator = null)
        {
            base.Initialize(configurator);
            
            var wm = configurator ? configurator.wm : weaponManager;
            if (!wm)
                return;
            
            // Fixing particles
            foreach (var vaporObject in wingVaporParticles)
            {
                var tf = wm.transform.Find(vaporObject);

                if (!tf || !tf.gameObject.GetComponent<WingVaporParticles>())
                {
                    Debug.Log($"[HPEquipCVWing_Initialize]: Cannae get tf {vaporObject} unfortunately");
                    continue;
                }

                var vaporParticles = tf.gameObject.GetComponent<WingVaporParticles>();
                OnWillRenderObjectHost.GetHost(vaporParticles.renderHost).UnRegisterScript(vaporParticles);
                vaporParticles.StartCoroutine(vaporParticles.UpdateRoutine());
            }
            
            var vehicleInputManager = wm.GetComponent<VehicleInputManager>();

            // Wait a few frames so any vehicle setup stuff should happen before this.
            StartCoroutine(WaitForTilt(vehicleInputManager));

            if (!canopyAnimator)
                return;

            var originalCanopy = wm.GetComponentsInChildren<CanopyAnimator>(true).FirstOrDefault(e => e != canopyAnimator);
            if (originalCanopy != null)
                AudioController.instance.SetExteriorOpening($"canopy_{originalCanopy.GetInstanceID()}", 0f);

            canopyAnimator.flightInfo = wm.GetComponent<FlightInfo>();
            
            if (canopyAnimator.flightInfo == null)
                return;

            var switchTf = wm.transform.Find(canopySwitchPath);

            if (!switchTf)
            {
                Debug.Log($"[CVWings_Initialize]: Switch tf null");
                return;
            }

            _canopyLever = switchTf.GetComponent<VRLever>();
                
            _canopyLever.OnSetState.AddListener(SetCanopyState);

            canopyAnimator.SetCanopyImmediate(_canopyLever.currentState == 1);
        }

        private IEnumerator WaitForTilt(VehicleInputManager vehicleInputManager)
        {
            yield return null;
            yield return null;
            yield return null;
            if (!vehicleInputManager.tiltController)
                yield break;
            
            _tiltController = vehicleInputManager.tiltController;
            bool reversed = _tiltController.reverseAnimDirection;
            _tiltController.SetTiltImmediate(reversed ? 90 : 0);
            
            vehicleInputManager.tiltController = null;
        }

        public void SetCanopyState(int state)
        {
            canopyAnimator.SetCanopyState(state);
        }
    }
}