
    using System.Collections;
    using UnityEngine;
    using UnityEngine.UI;

    public class SavingStatus : MonoBehaviour
    {
        public Text statusText;
        
        public string[] statusAnimation;

        public string defaultText;

        public float animationSpeed;

        [HideInInspector]
        public bool saving;

        public IEnumerator SaveRoutine()
        {
            int i = 0;
            int max = statusAnimation.Length;
            while (saving)
            {
                statusText.text = statusAnimation[i++];

                i %= max;
                yield return new WaitForSeconds(animationSpeed);
            }

            statusText.text = defaultText;
        }
    }