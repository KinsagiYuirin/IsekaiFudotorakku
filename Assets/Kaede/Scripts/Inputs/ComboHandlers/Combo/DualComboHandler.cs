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
        private readonly ComboKey _secondKey;
        private const float SimultaneousGraceSeconds = 0.1f;

        public DualComboHandler(ComboKey secondKey)
        {
            _secondKey = secondKey;
        }

        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey,
            CancellationToken ct, IComboButtonVisual visual)
        {
            var primaryDown = input.IsKeyDown(expectedKey);
            var secondaryDown = input.IsKeyDown(_secondKey);
            
            var primaryHeld = input.IsKeyHeld(expectedKey);
            var secondaryHeld = input.IsKeyHeld(_secondKey);
            
            var primaryUp = input.IsKeyUp(expectedKey);
            var secondaryUp = input.IsKeyUp(_secondKey);
            
            var primaryActive = primaryHeld || primaryDown;
            var secondaryActive = secondaryHeld || secondaryDown;

            if (input.AnyOtherKeyDown(expectedKey, _secondKey))
            {
                ResetState();
                return ComboInputResult.Wrong;
            }
            
            if (primaryDown && secondaryDown)
            {
                ResetState();
                return ComboInputResult.Correct;
            }
            
            if (_waitingForSecondKey)
            {
                _remainingSimultaneousWindow -= Time.deltaTime;

                var secondPressedWhileFirstActive = (primaryDown && secondaryActive) || (secondaryDown && primaryActive);
                if (secondPressedWhileFirstActive)
                {
                    ResetState();
                    return ComboInputResult.Correct;
                }
                
                if (_remainingSimultaneousWindow <= 0f)
                {
                    ResetState();
                    return ComboInputResult.Wrong;
                }
            }
            else if (!primaryActive && !secondaryActive && (primaryUp || secondaryUp))
            {
                _waitingForSecondKey = true;
                _remainingSimultaneousWindow = SimultaneousGraceSeconds;
            }
            
            if (primaryUp || secondaryUp)
            {
                ResetState();
                return ComboInputResult.Complete;
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
