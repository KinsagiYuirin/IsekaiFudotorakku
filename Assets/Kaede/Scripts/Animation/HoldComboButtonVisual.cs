using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using Kaede.Scripts.Utils;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Animation
{
    [DisallowMultipleComponent]
    public class HoldComboButtonVisual : MonoBehaviour, IComboButtonVisual
    {
        [Title("Icon References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text labelText;

        [Header("Slide Bar")]
        [SerializeField] private Image fillImage;
        [SerializeField] private float updateSpeed = 1f;
        [SerializeField, DisplayAsString] private float currentIndexPercent = 1f;
        
        [Title("Dynamic Sizing")]
        [SerializeField] private float baseWidth = 100f;
        [SerializeField] private float pixelsPerSecond = 40f;
        [SerializeField] private Vector2 sizeRange = new Vector2(80f, 400f);
        [SerializeField] private LayoutElement layoutElement;

        private RectTransform _rectTransform;
        private RectTransform _fillImageRect;

        [Header("Details")]
        [SerializeField] private bool needSmoothFill = true;

        [SerializeField, DisplayAsString] private float holdDuration;
        private bool _isHolding;
        
        private void Awake()
        {
            iconImage ??= GetComponent<Image>();
            fillImage ??= GetComponentInChildren<Image>();
            labelText ??= GetComponentInChildren<TMP_Text>();

            _rectTransform = GetComponent<RectTransform>();
            if (fillImage != null)
                _fillImageRect = fillImage.GetComponent<RectTransform>();

            // สร้าง LayoutElement ถ้ายังไม่มี
            if (layoutElement == null)
            {
                layoutElement = GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = gameObject.AddComponent<LayoutElement>();
                }
            }

            ResetProgress();
        }

        public void Initialize(ComboKeySetting comboSetting, string displayKey, bool isStringKey)
        {
            holdDuration = comboSetting is { type: ComboType.Hold } ? comboSetting.holdTime : 1f;

            if (!isStringKey)
            {
                labelText.alpha = 0f;
            }
            else
            {
                labelText.alpha = 1f;
                if (labelText != null)
                {
                    labelText.text = displayKey;
                }
            }

            UpdateLayoutElementSize();
            ResetProgress();
        }

        public void SetState(KeyState state, int? index, float? indexFloat)
        {
            _ = index;

            switch (state)
            {
                case KeyState.Active:
                    UpdateProgress(indexFloat ?? 0f);
                    break;
                case KeyState.Current:
                case KeyState.Prepare:
                    _isHolding = false;
                    break;
                case KeyState.Ideal:
                default:
                    ResetProgress();
                    break;
            }
        }

        public void SetColor(Color color)
        {
            if (iconImage != null)
            {
                iconImage.color = color;
            }
        }

        public void SetSprite(Sprite sprite)
        {
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
            }
        }
        
        private void UpdateProgress(float elapsedTime)
        {
            if (!_isHolding)
            {
                _isHolding = true;
                ApplyFill(0f);
            }

            var targetPercent = holdDuration <= 0f 
                ? 1f 
                : Mathf.Clamp01(elapsedTime / holdDuration);
            ApplyFill(targetPercent);
        }

        private void ApplyFill(float targetPercent)
        {
            targetPercent = Mathf.Clamp01(targetPercent);

            if (needSmoothFill && updateSpeed > Mathf.Epsilon)
            {
                var t = 1f - Mathf.Exp(-updateSpeed * Time.deltaTime);
                currentIndexPercent = Mathf.Lerp(currentIndexPercent, targetPercent, t);
            }
            else
            {
                currentIndexPercent = targetPercent;
            }

            if (fillImage != null)
            {
                fillImage.fillAmount = currentIndexPercent;
            }
        }
        
        private void ResetProgress()
        {
            _isHolding = false;
            currentIndexPercent = 0f;
            ApplyFill(0f);
        }
            
        private void UpdateLayoutElementSize()
        {
            var calculatedWidth = baseWidth + (holdDuration * pixelsPerSecond);
            var targetWidth = Mathf.Clamp(calculatedWidth, sizeRange.x, sizeRange.y);
            
            Debug.Log($"Calculating size: Duration={holdDuration}s, Target Width={targetWidth}px");
            
            // Method 1: ใช้ LayoutElement
            if (layoutElement != null)
            {
                layoutElement.preferredWidth = targetWidth;
                layoutElement.flexibleWidth = 0f;
                layoutElement.minWidth = targetWidth; // เพิ่มเพื่อบังคับขนาด
            }
            
            // Method 2: ปรับ RectTransform โดยตรงด้วย (สำรอง)
            if (_rectTransform != null)
            {
                var sizeDelta = _rectTransform.sizeDelta;
                sizeDelta.x = targetWidth;
                _rectTransform.sizeDelta = sizeDelta;
                
                Debug.Log($"RectTransform size set to: {_rectTransform.sizeDelta.x}");
            }
            
            SetupFillImageAnchors();
            StartCoroutine(RebuildLayoutNextFrame());
        }

        private void SetupFillImageAnchors()
        {
            if (_fillImageRect == null) return;
            
            Debug.Log("Setting up fillImage anchors...");
            
            // ตั้ง anchor ให้ stretch horizontally และ center vertically
            _fillImageRect.anchorMin = new Vector2(0f, 0.5f);
            _fillImageRect.anchorMax = new Vector2(1f, 0.5f);
            
            _fillImageRect.anchoredPosition = Vector2.zero;
            
            // ให้ width ถูกกำหนดโดย anchor, เซ็ตแค่ height
            var fillSize = _fillImageRect.sizeDelta;
            fillSize.x = 0f; // width จะใช้จาก anchor
            _fillImageRect.sizeDelta = fillSize;
            
            // เพิ่ม margin ถ้าต้องการ (optional)
            _fillImageRect.offsetMin = new Vector2(2f, -fillSize.y/2f); // left margin
            _fillImageRect.offsetMax = new Vector2(-2f, fillSize.y/2f); // right margin
            
            Debug.Log($"FillImage anchor set: {_fillImageRect.anchorMin} to {_fillImageRect.anchorMax}");
        }

        private IEnumerator RebuildLayoutNextFrame()
        {
            yield return null;
            
            Debug.Log("Rebuilding layout...");
            
            // Rebuild parent layout
            if (transform.parent != null)
            {
                var parentRect = transform.parent as RectTransform;
                if (parentRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                    Debug.Log("Parent layout rebuilt");
                }
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);
            Debug.Log($"Final sizes - GameObject: {_rectTransform.sizeDelta.x}, FillImage: {_fillImageRect?.sizeDelta.x}");
        }
    }

}
