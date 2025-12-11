using System.Threading;
using Kaede.Scripts.Animation;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;
using UnityEngine;

namespace Kaede.Scripts.Inputs.ComboHandlers.Combo
{
    public class DualHoldComboHandler : IComboHandler
    {
        private float _elapsed;
        private readonly float _maxAmount;
        private readonly Vector2 _requiredTimeRange;
        private float _remainingSimultaneousWindow;
        private bool _waitingForSecondKey;
        private bool _isHolding;
        private bool _pendingCompletion;
        private readonly ComboKey _secondKey;
        private const float SimultaneousGraceSeconds = 0.1f;
        
        public float Progress => _maxAmount <= 0f ? 0f : Mathf.Clamp01(_elapsed / _maxAmount);

        public DualHoldComboHandler(float requiredTime, ComboKey secondKey)
        {
            _maxAmount = requiredTime;
            _requiredTimeRange = new Vector2(requiredTime - 0.5f, requiredTime + 0.5f);
            _secondKey = secondKey;
        }

        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey,
            CancellationToken ct, IComboButtonVisual visual)
        {
            if (_pendingCompletion)
            {
                _pendingCompletion = false;
                return ComboInputResult.Complete;
            }

            var primaryDown = input.IsKeyDown(expectedKey);
            var secondaryDown = input.IsKeyDown(_secondKey);
            
            var primaryHeld = input.IsKeyHeld(expectedKey);
            var secondaryHeld = input.IsKeyHeld(_secondKey);
            
            var primaryActive = primaryHeld || primaryDown;
            var secondaryActive = secondaryHeld || secondaryDown;

            if (input.AnyOtherKeyDown(expectedKey, _secondKey))
            {
                ResetHold();
                _pendingCompletion = true;
                return ComboInputResult.Wrong;
            }
            
            if (!_isHolding)
            {
                if (primaryActive && secondaryActive)
                {
                    BeginHold();
                }
                else if (primaryActive ^ secondaryActive)
                {
                    if (!_waitingForSecondKey)
                    {
                        _waitingForSecondKey = true;
                        _remainingSimultaneousWindow = SimultaneousGraceSeconds;
                    }
                    else
                    {
                        _remainingSimultaneousWindow -= Time.deltaTime;
                        if (_remainingSimultaneousWindow <= 0f || (!primaryActive && !secondaryActive))
                        {
                            ResetHold();
                            _pendingCompletion = true;
                            return ComboInputResult.Wrong;
                        }
                    }
                }
            }
            
            if (_isHolding)
            {
                if (primaryActive && secondaryActive)
                {
                    _elapsed += Time.deltaTime;
                    
                    if (_elapsed > _requiredTimeRange.y)
                    {
                        ResetHold();
                        _pendingCompletion = true;
                        return ComboInputResult.Wrong;
                    }

                    visual.SetState(KeyState.Active, null, _elapsed);
                    return ComboInputResult.Holding;
                }
                
                if (input.IsKeyUp(expectedKey) || input.IsKeyUp(_secondKey) || !primaryActive || !secondaryActive)
                {
                    var duration = _elapsed;
                    var withinWindow = duration >= _requiredTimeRange.x && duration <= _requiredTimeRange.y;
                    
                    ResetHold();
                    _pendingCompletion = true;
                    return withinWindow ? ComboInputResult.Correct : ComboInputResult.Wrong;
                }
            }

            return ComboInputResult.None;
        }

        private void ResetHold()
        {
            _isHolding = false;
            _waitingForSecondKey = false;
            _remainingSimultaneousWindow = 0f;
        }

        private void BeginHold()
        {
            _isHolding = true;
            _waitingForSecondKey = false;
            _elapsed = 0f;
        }
    }
}
