using UnityEngine;

namespace DDArmory.Weapons.Wings.CV_Wings
{
    public class HPEquipCVWing : HPEquipWing
    {
        [Tooltip("Path to canopy switch separated by '/'")]
        public string canopySwitchPath;


        private TiltController _tiltController;

        private CanopyAnimator _canopyAnimator;

        public override void OnConfigDetach(LoadoutConfigurator configurator)
        {
            base.OnConfigDetach(configurator);
            
            var wm = configurator ? configurator.wm : weaponManager;
            if (!wm)
                return;

            
            
            var vehicleInputManager = wm.GetComponent<VehicleInputManager>();
            vehicleInputManager.tiltController = _tiltController;
        }

        public override void OnUnequip()
        {
            base.OnUnequip();

            var vehicleInputManager = weaponManager.GetComponent<VehicleInputManager>();
            vehicleInputManager.tiltController = _tiltController;
        }
        

        public override void Initialize(LoadoutConfigurator configurator = null)
        {
            base.Initialize(configurator);
            
            var wm = configurator ? configurator.wm : weaponManager;
            if (!wm)
                return;

            
            var vehicleInputManager = wm.GetComponent<VehicleInputManager>();
            
            if (vehicleInputManager.tiltController)
            {
                _tiltController = vehicleInputManager.tiltController;
                bool reversed = _tiltController.reverseAnimDirection;
                _tiltController.SetTiltImmediate(reversed ? 1 : 0);
                
                vehicleInputManager.tiltController = null;
            }
        }
    }
}