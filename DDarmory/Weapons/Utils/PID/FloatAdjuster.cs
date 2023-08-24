using System;
using UnityEngine;
using UnityEngine.Events;

namespace DDArmory.Weapons.Utils
{
    public class FloatAdjuster : MonoBehaviour
    {
        public TextMesh textMesh;

        [Tooltip("Adjusts value by this much per second")]
        public float adjustSpeed = 1f;

        public FloatEvent OnSetFloat = new FloatEvent();

        // so you can do default value
        public float value;

        private bool adjusting;

        private bool subtract;

        private void Start()
        {
            if (textMesh)
                textMesh.text = $"{value}";
        }

        public void OnInteractAdd()
        {
            adjusting = true;
            subtract = false;
        }

        public void OnInteractSubtract()
        {
            adjusting = true;
            subtract = true;
        }

        public void Update()
        {
            if (!adjusting)
                return;

            value += (adjustSpeed * Time.deltaTime) * (subtract? -1 : 1);
            if (textMesh)
                textMesh.text = $"{Math.Round(value, 3, MidpointRounding.ToEven)}";
            OnSetFloat.Invoke(value);
        }

        public void OnStopInteract()
        {
            adjusting = false;
        }
    }
}