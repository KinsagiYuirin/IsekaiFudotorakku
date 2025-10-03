using System;
using System.Collections;
using Kaede.Scripts.Inputs.ComboHandlers.Combo;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Utils
{
    public class YuirinSlideBar : MonoBehaviour
    {
        [Title("Slide Bar")] [SerializeField] private Image fillImage;
        [SerializeField] private float updateSpeed = 1f;
        [SerializeField] private float currentIndexPercent = 1f;

        [Header("Details")] [SerializeField] private float firstUpdateSpeed;
        [SerializeField] private bool needSmoothFill = true;

        private Coroutine _slideCoroutine;

        private void OnDisable()
        {
            StopSlide();
        }

        public void UpdateSlideUI(float index, float maxIndex)
        {
            var targetPercent = Mathf.Clamp01(index / maxIndex);

            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);

            _slideCoroutine = StartCoroutine(SmoothFill(targetPercent));
        }

        private IEnumerator SmoothFill(float targetPercent)
        {
            var initialPercent = currentIndexPercent;
            var timer = 0f;

            while (timer < updateSpeed)
            {
                timer += Time.deltaTime;
                currentIndexPercent = Mathf.Lerp(initialPercent, targetPercent, timer / updateSpeed);
                fillImage.fillAmount = currentIndexPercent;
                yield return null;
            }

            currentIndexPercent = targetPercent;
            fillImage.fillAmount = targetPercent;
            _slideCoroutine = null;
        }

        public void ResetFill()
        {
            if (_slideCoroutine != null)
            {
                StopCoroutine(_slideCoroutine);
                _slideCoroutine = null;
            }

            currentIndexPercent = 0f;
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
            }
        }

        public void StopSlide()
        {
            if (_slideCoroutine != null)
            {
                StopCoroutine(_slideCoroutine);
            }
            _slideCoroutine = null;
        }
    }
}
