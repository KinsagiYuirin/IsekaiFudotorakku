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
        [SerializeField] private float currentIndexPercent = 1f;
        [Header("Details")]
        [SerializeField] private float firstUpdateSpeed;
        [SerializeField] private bool needSmoothFill = true;
        private CancellationTokenSource _slideCts;

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
            if (!iconImage)
            {
                iconImage.color = color;
            }

            if (!fillImage)
            {
                var progressColor = color;
                progressColor.a = Mathf.Clamp01(color.a * 0.6f);
                fillImage.color = progressColor;
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
            StartHoldAnimationAsync().Forget();
        }

        private async UniTaskVoid StartHoldAnimationAsync()
        {
            StopHoldAnimation();
            _slideCts = new CancellationTokenSource();
            await SmoothFillAsync(_currentProgress, _holdDuration, _slideCts.Token);
        }

        private async UniTask SmoothFillAsync(float index, float maxIndex, CancellationToken token)
        {
            var targetPercent = Mathf.Clamp01(index / maxIndex);
            var initialPercent = currentIndexPercent;
            var timer = 0f;
            while (timer < updateSpeed)
            {
                timer += Time.deltaTime;
                currentIndexPercent = Mathf.Lerp(initialPercent, targetPercent, timer / updateSpeed);
                if (fillImage != null)
                    fillImage.fillAmount = currentIndexPercent;
                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            currentIndexPercent = targetPercent;
            if (fillImage != null)
                fillImage.fillAmount = targetPercent;
        }
        
        private void StopHoldAnimation()
        {
            if (_slideCts != null)
            {
                _slideCts.Cancel();
                _slideCts.Dispose();
                _slideCts = null;
            }
        }

        private void ResetProgress()
        {
            StopHoldAnimation();
            currentIndexPercent = 0f;
            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
            }
        }
    }
}
