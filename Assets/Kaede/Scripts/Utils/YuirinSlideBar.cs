using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Utils
{
    public class YuirinSlideBar : MonoBehaviour
    {
        [Title("Slide Bar")]
        [SerializeField] private Image fillImage;
        [SerializeField] private float currentIndex;
        public float CurrentIndex{get => currentIndex; set => currentIndex = value;}
        [SerializeField] private float updateSpeed = 0.5f;
        [SerializeField] private float currentIndexPercent = 1f;

        [Header("Details")] 
        [SerializeField] private float firstUpdateSpeed;
        [SerializeField] private bool needSmoothFill = true;
        
        private Coroutine _slideCoroutine;
        
        /// <summary>
        /// เรียกเมื่อ HP เปลี่ยน เพื่ออัปเดตหลอดเลือดแบบ Smooth
        /// </summary>
        public void UpdateSlideUI(float index, float maxIndex)
        {
            var targetPercent = Mathf.Clamp01(index / maxIndex);
            
            if (_slideCoroutine != null)
                StopCoroutine(_slideCoroutine);
        
            _slideCoroutine = StartCoroutine(SmoothFill(targetPercent));
        }

        /// <summary>
        /// ค่อยๆ เปลี่ยนค่าหลอดเลือดแบบลื่น
        /// </summary>
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
    }
}
