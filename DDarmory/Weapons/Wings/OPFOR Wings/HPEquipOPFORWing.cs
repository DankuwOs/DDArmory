using System.Collections.Generic;
using UnityEngine;

namespace DDArmory.Weapons.Wings.OPFOR_Wings
{
    public class HPEquipOPFORWing : HPEquipWing
    {
        private struct ObjectTransform
        {
            public ObjectTransform(Transform objectTf, Vector3 position, Vector3 rotation)
            {
                this.objectTf = objectTf;
                this.position = position;
                this.rotation = rotation;
            }
            
            public Transform objectTf;

            public Vector3 position;

            public Vector3 rotation;
        }
        
        [Header("OPFOR Stabilizers")]
        public List<string> vertStabPaths;

        public List<Vector3> vertStabPositions;

        public List<Vector3> vertStabRotations;

        private List<ObjectTransform> originalTfs;

        public override void OnConfigDetach(LoadoutConfigurator configurator)
        {
            if (originalTfs == null)
            {
                base.OnConfigDetach(configurator);
                return;
            }
            
            foreach (var objectTransform in originalTfs)
            {
                var tf = objectTransform.objectTf;
                tf.localPosition = objectTransform.position;
                tf.localRotation = Quaternion.Euler(objectTransform.rotation);
            }
            
            base.OnConfigDetach(configurator);
        }

        public override void OnUnequip()
        {
            if (originalTfs == null)
            {
                base.OnUnequip();
                return;
            }
            
            foreach (var objectTransform in originalTfs)
            {
                var tf = objectTransform.objectTf;
                tf.localPosition = objectTransform.position;
                tf.localRotation = Quaternion.Euler(objectTransform.rotation);
            }
            
            base.OnUnequip();
        }

        public override void Initialize(LoadoutConfigurator configurator = null)
        {
            base.Initialize(configurator);
            
            var wm = configurator ? configurator.wm : weaponManager;

            if (!wm)
                return;

            originalTfs = new List<ObjectTransform>();

            for (var i = 0; i < vertStabPaths.Count; i++)
            {
                var vertStabPath = vertStabPaths[i];
                var tf = wm.transform.Find(vertStabPath);
                var objectTfStruct = new ObjectTransform(tf, tf.localPosition, tf.localRotation.eulerAngles);
                originalTfs.Add(objectTfStruct);
                tf.localPosition = vertStabPositions[i];
                tf.localRotation = tf.localRotation = Quaternion.Euler(vertStabRotations[i]);
            }
            
            controller.SetupWingFold();
        }
    }
}