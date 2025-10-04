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
        [Title("References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text labelText;

        [Header("Slide Bar")]
        [SerializeField] private Image fillImage;
        [SerializeField] private float updateSpeed = 1f;
        [SerializeField, DisplayAsString] private float currentIndexPercent = 1f;
        
        [Header("Details")]
        [SerializeField] private bool needSmoothFill = true;

        [SerializeField, DisplayAsString] private float _holdDuration;
        private bool _isHolding;
        
        private void Awake()
        {
            iconImage ??= GetComponent<Image>();
            fillImage ??= GetComponentInChildren<Image>();
            labelText ??= GetComponentInChildren<TMP_Text>();

            ResetProgress();
        }

        public void Initialize(ComboKeySetting comboSetting, string displayKey)
        {
            if (comboSetting is { type: ComboType.Hold })
            {
                _holdDuration = comboSetting.holdTime;
            }
            else
            {
                _holdDuration = 1f;
            }

            if (labelText != null)
            {
                labelText.text = displayKey;
            }

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

            /*if (fillImage != null)
            {
                var progressColor = color;
                progressColor.a = Mathf.Clamp01(color.a * 0.6f);
                fillImage.color = progressColor;
            }*/
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

            var targetPercent = _holdDuration <= 0f 
                ? 1f 
                : Mathf.Clamp01(elapsedTime / _holdDuration);
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
    }
}
