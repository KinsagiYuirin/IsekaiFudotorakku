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
        [SerializeField] private bool needSmoothFill = true;
        private CancellationTokenSource _slideCts;

        [SerializeField, DisplayAsString] private float _holdDuration;
        private Coroutine _fillRoutine;

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
            switch (state)
            {
                case KeyState.Current:
                    break;
                case KeyState.Active:
                    StartHoldAnimation();
                    Debug.Log("Start Hold Animation");
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
        
        private void StartHoldAnimation()
        {
            StartHoldAnimationAsync().Forget();
        }

        private async UniTaskVoid StartHoldAnimationAsync()
        {
            StopHoldAnimation();
            _slideCts = new CancellationTokenSource();
            var token = _slideCts.Token;

            var elapsed = 0f;

            while (elapsed < _holdDuration)
            {
                token.ThrowIfCancellationRequested();

                elapsed = Mathf.Min(elapsed + Time.deltaTime, _holdDuration);
                await SmoothFillAsync(elapsed, _holdDuration, token);

                await UniTask.Yield(PlayerLoopTiming.Update, token);
            }
            await SmoothFillAsync(_holdDuration, _holdDuration, token);
        }

        private UniTask SmoothFillAsync(float elapsed, float maxIndex, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var targetPercent = maxIndex <= 0f ? 1f : Mathf.Clamp01(elapsed / maxIndex);

            if (needSmoothFill && updateSpeed > 0f)
            {
                var step = Time.deltaTime / updateSpeed;
                currentIndexPercent = Mathf.MoveTowards(currentIndexPercent, targetPercent, step);
            }
            else
            {
                currentIndexPercent = targetPercent;
            }

            if (elapsed >= maxIndex)
            {
                currentIndexPercent = targetPercent;
            }

            if (fillImage != null)
            {
                fillImage.fillAmount = targetPercent;
            }

            return UniTask.CompletedTask;
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
