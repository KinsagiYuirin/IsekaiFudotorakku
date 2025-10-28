using System;
using System.Threading;
using Kaede.Art.TA_Works.Scripts;
using Kaede.Scripts.Animation;
using Kaede.Scripts.Audios;
using Kaede.Scripts.Inputs.ComboHandlers;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;

namespace Kaede.Scripts.GamePlay
{
    internal sealed class ComboInputProcessor : IDisposable
    {
        private readonly PlayerInputHandler _inputHandler;
        private readonly ComboCookingView   _view;
        private readonly float              _scorePerButton;
        private readonly ComboStepAnimationPlayer _animationPlayer;
        private readonly ComboCharacterEmotionPlayer _emotionPlayer;
        private readonly SfxManagerDemo _sfxManager;
        
        private IComboHandler _currentHandler;
        private ComboKeySetting _currentComboSetting;
        private CancellationTokenSource _inputCts;
        private bool _checking;

        public ComboInputProcessor(PlayerInputHandler inputHandler, ComboCookingView view, float scorePerButton, 
            ComboStepAnimationPlayer animationPlayer, ComboCharacterEmotionPlayer emotionPlayer, SfxManagerDemo sfxManager)
        {
            _inputHandler    = inputHandler;
            _view            = view;
            _scorePerButton  = scorePerButton;
            _animationPlayer = animationPlayer;
            _emotionPlayer   = emotionPlayer;
            _sfxManager      = sfxManager;
        }

        public bool IsStepComplete { get; private set; }
        private bool _holdAnimationActive;

        public void Process(ComboCookingModel model)
        {
            if (model == null) return;
            if (model.GameState == CookingState.Resting) return;
            if (_checking) return;
            if (!model.TryGetCurrentSequenceCount(out var count) || count == 0) return;
            if (model.CurrentComboIndex >= count) return;

            _checking = true;
            _inputCts ??= new CancellationTokenSource();

            try
            {
                if (!model.TryGetExpectedCombo(out var expectedCombo)) return;

                if (_currentHandler == null || _currentComboSetting != expectedCombo)
                {
                    _currentHandler      = ComboHandlerFactory.Create(expectedCombo);
                    _currentComboSetting = expectedCombo;
                }

                var result = _currentHandler.CheckInput(_inputHandler, expectedCombo.key, _inputCts.Token, _view.ButtonVisuals[model.CurrentComboIndex]);
                _view.CurrentKeyPressed(model.CurrentComboIndex);
                if (IsStepComplete) return;

                switch (result)
                {
                    case ComboInputResult.Progress:
                        TriggerAnimation(true);
                        PlaySuccessEmotion();
                        model.ScoreManager.AddPendingStepScore(_scorePerButton);
                        break;
                        
                    case ComboInputResult.Correct:
                        _sfxManager.PlaySuccessSound();
                        TriggerAnimation(true);
                        PlaySuccessEmotion();
                        _view.PressCorrectKey(model.CurrentComboIndex);
                        model.ScoreManager.AddPendingStepScore(_scorePerButton);
                        model.ScoreManager.AddCombo();
                        NextCombo(model);
                        _view.UpdateComboText(model.ScoreManager.ComboCount);
                        break;

                    case ComboInputResult.Wrong:
                        _sfxManager.PlayFailureSound();
                        TriggerAnimation(false);
                        PlayFailureEmotion();
                        if (!IsStepComplete)
                        {
                            _view.PressWrongKey(model.CurrentComboIndex);
                            model.ScoreManager.ResetCombo();
                            _view.UpdateComboText(model.ScoreManager.ComboCount);
                        }
                        NextCombo(model);
                        break;
                    case ComboInputResult.Holding:
                        BeginHoldEmotion();
                        break;
                    case ComboInputResult.None:
                    default:
                        break;
                }
            }
            finally
            {
                _checking = false;
            }
        }

        public void ResetState()
        {
            CancelInputLoop();
            IsStepComplete = false;
            ResetHandler();
            ResetHoldEmotion();
            _emotionPlayer?.ResetToIdle();
        }

        public void ResetStateWithCombo(ComboCookingModel model)
        {
            ResetState();
            model?.ResetCombo();
        }

        private void NextCombo(ComboCookingModel model)
        {
            if (!model.TryGetCurrentSequenceCount(out var count) || count == 0) return;

            if (model.CurrentComboIndex + 1 >= count)
            {
                IsStepComplete = true;
                ResetHandler();
                CancelInputLoop();
                ResetHoldEmotion();
                return;
            }

            ResetHandler();
            model.NextCombo();
        }

        private void ResetHandler()
        {
            _currentHandler      = null;
            _currentComboSetting = null;
        }

        private void TriggerAnimation(bool isCorrect)
        {
            if (_animationPlayer == null)
            { return; }

            if (isCorrect)
            {
                _animationPlayer.Play();
            }
            else
            {
                var playedWrong = _animationPlayer.PlayWrongFeedback();
                if (!playedWrong)
                {
                    _animationPlayer.Play();
                }
            }
        }
        
        private void BeginHoldEmotion()
        {
            if (_emotionPlayer == null || _holdAnimationActive)
            { return; }

            _holdAnimationActive = true;
            _emotionPlayer.PlayHoldLoop();
        }

        private void ResetHoldEmotion()
        {
            if (!_holdAnimationActive)
            { return; }

            _holdAnimationActive = false;
        }

        private void PlaySuccessEmotion()
        {
            if (_emotionPlayer == null)
            { return; }

            ResetHoldEmotion();
            _emotionPlayer.PlaySuccess();
        }

        private void PlayFailureEmotion()
        {
            if (_emotionPlayer == null)
            { return; }

            ResetHoldEmotion();
            _emotionPlayer.PlayFailure();
        }
        
        private void CancelInputLoop()
        {
            _inputCts?.Cancel();
            _inputCts?.Dispose();
            _inputCts = null;
        }

        public void Dispose()
        {
            CancelInputLoop();
            ResetHoldEmotion();
            _emotionPlayer?.ResetToIdle();
        }
    }
}