using System.Threading;
using Kaede.Scripts.Animation;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;
using UnityEngine;

namespace Kaede.Scripts.Inputs.ComboHandlers.Combo
{
    public class DualComboHandler : IComboHandler
    {
        private float _remainingSimultaneousWindow;
        private bool _waitingForSecondKey;
        private bool _isActionDone;
        private readonly ComboKey _secondKey;
        private const float SimultaneousGraceSeconds = 0.1f;

        public DualComboHandler(ComboKey secondKey)
        {
            _secondKey = secondKey;
        }

        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey,
            CancellationToken ct, IComboButtonVisual visual)
        {
            if (_isActionDone)
            {
                bool expectedUp = input.IsKeyUp(expectedKey) || !input.IsKeyHeld(expectedKey);
                bool secondUp = input.IsKeyUp(_secondKey) || !input.IsKeyHeld(_secondKey);

                if (expectedUp || secondUp)
                {
                    _isActionDone = false;
                    ResetState();
                    return ComboInputResult.Complete;
                }
                return ComboInputResult.None;
            }

            var primaryDown = input.IsKeyDown(expectedKey);
            var secondaryDown = input.IsKeyDown(_secondKey);
            
            var primaryHeld = input.IsKeyHeld(expectedKey);
            var secondaryHeld = input.IsKeyHeld(_secondKey);
            
            var primaryActive = primaryHeld || primaryDown;
            var secondaryActive = secondaryHeld || secondaryDown;

            if (input.AnyOtherKeyDown(expectedKey, _secondKey))
            {
                _isActionDone = true; 
                ResetState();
                return ComboInputResult.Wrong;
            }

            if (primaryDown && secondaryDown)
            {
                _isActionDone = true;
                ResetState();
                return ComboInputResult.Correct;
            }
            
            if (_waitingForSecondKey)
            {
                _remainingSimultaneousWindow -= Time.deltaTime;

                var secondPressedWhileFirstActive = (primaryDown && secondaryActive) || (secondaryDown && primaryActive);
                if (secondPressedWhileFirstActive)
                {
                    _isActionDone = true;
                    ResetState();
                    return ComboInputResult.Correct;
                }
                
                if ((!primaryActive && !secondaryActive) || _remainingSimultaneousWindow <= 0f)
                {
                    _isActionDone = true;
                    ResetState();
                    return ComboInputResult.Wrong;
                }
            }

            if (!_waitingForSecondKey && !_isActionDone && (primaryDown ^ secondaryDown))
            {
                 _waitingForSecondKey = true;
                 _remainingSimultaneousWindow = SimultaneousGraceSeconds;
            }
            
            return ComboInputResult.None;
        }
        
        private void ResetState()
        {
            _waitingForSecondKey = false;
            _remainingSimultaneousWindow = 0f;
        }
    }
}