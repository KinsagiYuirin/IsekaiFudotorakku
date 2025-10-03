using System.Collections;
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
        [SerializeField] private YuirinSlideBar slideBar;
        
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image holdProgressImage;
        [SerializeField] private TMP_Text labelText;

        private float _currentProgress;
        private float _holdDuration = 1f;
        private Coroutine _fillRoutine;

        private void Awake()
        {
            iconImage ??= GetComponent<Image>();
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

        public void SetState(KeyState state)
        {
            switch (state)
            {
                case KeyState.Current:
                    break;
                case KeyState.Active:
                    StartHoldAnimation();
                    break;
                case KeyState.Ideal:
                    StopHoldAnimation();
                    break;
                default:
                    StopHoldAnimation();
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

            if (holdProgressImage != null)
            {
                var progressColor = color;
                progressColor.a = Mathf.Clamp01(color.a * 0.6f);
                holdProgressImage.color = progressColor;
            }
        }

        public void SetSprite(Sprite sprite)
        {
            if (!iconImage)
            {
                iconImage.sprite = sprite;
            }
        }

        private void StartHoldAnimation()
        {
            Debug.Log("Starting hold animation");
            slideBar.UpdateSlideUI(_currentProgress, _holdDuration);
        }

        private void StopHoldAnimation()
        {
            slideBar.StopSlide();
        }

        private void ResetProgress()
        {
            slideBar.ResetFill();
        }
    }
}
