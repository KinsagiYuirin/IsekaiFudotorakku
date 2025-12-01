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
        private float _elapsed;
        private readonly float _maxAmount;
        private readonly Vector2 _requiredTimeRange;
        private float _remainingSimultaneousWindow;
        private bool _waitingForSecondKey;
        private bool _isHolding;
        private readonly ComboKey _secondKey;
        private const float SimultaneousGraceSeconds = 0.1f;
        public float Progress => _maxAmount <= 0f ? 0f : Mathf.Clamp01(_elapsed / _maxAmount);

        public DualComboHandler(float requiredTime, ComboKey secondKey)
        {
            _maxAmount = requiredTime;
            _secondKey = secondKey;
        }

        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey,
            CancellationToken ct, IComboButtonVisual visual)
        {
            var primaryDown = input.IsKeyDown(expectedKey);
            var secondaryDown = input.IsKeyDown(_secondKey);
            
            var primaryHeld = input.IsKeyHeld(expectedKey);
            var secondaryHeld = input.IsKeyHeld(_secondKey);
            
            var primaryActive = primaryHeld || primaryDown;
            var secondaryActive = secondaryHeld || secondaryDown;

            // ต้องกดพร้อมกัน ถ้ากดปุ่มใดปุ่มหนึ่งก่อนถือว่าผิด
            if (!_isHolding)
            {
                if (primaryActive && secondaryActive)
                {
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
                            return ComboInputResult.Wrong;
                        }
                    }
                }
            }

            if (_waitingForSecondKey && !primaryActive && !secondaryActive)
            {
                return ComboInputResult.Wrong;
            }

            if (_isHolding && primaryActive && secondaryActive)
            {
                _elapsed += Time.deltaTime;
                if (_elapsed > _requiredTimeRange.y)
                {
                    return ComboInputResult.Wrong;
                }

                visual.SetState(KeyState.Active, null, _elapsed);
                return ComboInputResult.Holding;
            }

            if (_isHolding && (input.IsKeyUp(expectedKey) || input.IsKeyUp(_secondKey)))
            {
                var duration = _elapsed;
                var withinWindow = duration >= _requiredTimeRange.x && duration <= _requiredTimeRange.y;
                
                return withinWindow ? ComboInputResult.Correct : ComboInputResult.Wrong;
            }

            if (input.AnyOtherKeyDown(expectedKey, _secondKey))
            {
                return ComboInputResult.Wrong;
            }

            return ComboInputResult.None;
        }
    }
}
