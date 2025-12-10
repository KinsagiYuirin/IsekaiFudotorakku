using System;
using System.Globalization;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.GamePlay
{
    [Serializable]
    public class ComboTimerService
    {
        [SerializeField] private float restingTime = 3f;
        [SerializeField] private float maxTimePerCombo = 5f;
        [SerializeField, DisplayAsString] private float currentTimer;
        [SerializeField] private float dividerTimeScore;
        [SerializeField] private float delayAfterFinishMenu = 2f;
        [SerializeField] private float delayBeforeGameOver = 2f;

        private ComboCookingView _view;
        private bool _isResting;
        private bool _hasTriggered;
        private bool _paused;

        public float CurrentTimer => currentTimer;
        public float RestingTime => restingTime;
        public float MaxTimePerCombo => maxTimePerCombo;
        public float DelayAfterFinishMenu => delayAfterFinishMenu;
        public float DelayBeforeGameOver => delayBeforeGameOver;
        
        public event Action TimedOut;
        public event Action RestEntered;
        public event Action RestFinished;

        public void Initialize(ComboCookingView view)
        {
            _view = view;
            _isResting = false;
            ResetTimer();
            UpdateTimerText();
            _view?.SetRestingMode(false);
            _view?.SetFailEffect(false);
        }

        public void Tick(float deltaTime)
        {
            if (_view == null) return;
            if (_paused) return;
            
            if (currentTimer > 0f)
            {
                currentTimer = Mathf.Max(0f, currentTimer - deltaTime);
                UpdateTimerText();
            }

            if (currentTimer <= 0f && !_hasTriggered)
            {
                _hasTriggered = true;
                if (_isResting)
                {
                    EndRestingPhase();
                }
                else
                {
                    HandleComboTimeout();
                }
            }
        }

        public async UniTask PauseTimerForSeconds(float seconds)
        {
            SetPaused();
            await UniTask.Delay(TimeSpan.FromSeconds(seconds));
            SetResuming();
        }
        
        public void SetPaused()
        { _paused = true; }
        
        public void SetResuming()
        { _paused = false; }
        
        public void ResetTimer()
        {
            currentTimer = maxTimePerCombo;
            _hasTriggered = false;
            _isResting = false;
            UpdateTimerText();
        }

        public void AddTimeNextCombo()
        {
            currentTimer += maxTimePerCombo;
            _hasTriggered = false;
            UpdateTimerText();
        }

        public void HandleComboTimeout()
        {
            _isResting = false;
            currentTimer = maxTimePerCombo;
            UpdateTimerText();
            TimedOut?.Invoke();
            _hasTriggered = false;
        }

        public void BeginRestingPhase()
        {
            _isResting = true;
            currentTimer = restingTime;
            _hasTriggered = false;
            UpdateTimerText();
            _view?.SetRestingMode(true);
            RestEntered?.Invoke();
        }

        public void EndRestingPhase()
        {
            _isResting = false;
            currentTimer = maxTimePerCombo;
            UpdateTimerText();
            _view?.SetRestingMode(false);
            _view?.SetFailEffect(false);
            RestFinished?.Invoke();
            _hasTriggered = false;
        }

        public float DividerTimeToMultiply()
        {
            if (Mathf.Approximately(dividerTimeScore, 0f))
            {
                return 1f;
            }
            
            var newTime = currentTimer / dividerTimeScore;
            return newTime;
        }

        private void UpdateTimerText()
        {
            if (_view == null) return;
            var ts = TimeSpan.FromSeconds(currentTimer);
            _view.TimerText.text = ts.ToString("mm\\:ss");
        }
    }
}