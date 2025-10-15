using System;
using System.Threading;
using Kaede.Scripts.Animation;
using Kaede.Scripts.Inputs.ComboHandlers;
using Kaede.Scripts.Inputs.ComboHandlers.Combo;
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

        private IComboHandler _currentHandler;
        private ComboKeySetting _currentComboSetting;
        private CancellationTokenSource _inputCts;
        private bool _checking;

        public ComboInputProcessor(PlayerInputHandler inputHandler, ComboCookingView view, float scorePerButton, ComboStepAnimationPlayer animationPlayer)
        {
            _inputHandler    = inputHandler;
            _view            = view;
            _scorePerButton  = scorePerButton;
            _animationPlayer = animationPlayer;
        }

        public bool IsStepComplete { get; private set; }

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
                        TriggerAnimation();
                        model.ScoreManager.AddPendingStepScore(_scorePerButton);
                        break;
                        
                    case ComboInputResult.Correct:
                        TriggerAnimation();
                        _view.PressCorrectKey(model.CurrentComboIndex);
                        model.ScoreManager.AddPendingStepScore(_scorePerButton);
                        model.ScoreManager.AddCombo();
                        NextCombo(model);
                        _view.UpdateComboText(model.ScoreManager.ComboCount);
                        break;

                    case ComboInputResult.Wrong:
                        TriggerAnimation();
                        if (!IsStepComplete)
                        {
                            _view.PressWrongKey(model.CurrentComboIndex);
                            model.ScoreManager.ResetCombo();
                            _view.UpdateComboText(model.ScoreManager.ComboCount);
                        }
                        NextCombo(model);
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

        private void TriggerAnimation()
        {
            if (_animationPlayer == null)
            {
                return;
            }

            _animationPlayer.Play();
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
        }
    }
}