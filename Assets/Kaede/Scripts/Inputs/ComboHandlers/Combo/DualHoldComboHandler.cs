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
        private bool _isHolding;
        private readonly ComboKey _secondKey;
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
            var primaryDown = input.IsKeyDown(expectedKey);
            var secondaryDown = input.IsKeyDown(_secondKey);
            var primaryHeld = input.IsKeyHeld(expectedKey);
            var secondaryHeld = input.IsKeyHeld(_secondKey);

            // ต้องกดพร้อมกัน ถ้ากดปุ่มใดปุ่มหนึ่งก่อนถือว่าผิด
            if (!_isHolding && primaryDown != secondaryDown)
            {
                ResetHold();
                return ComboInputResult.Wrong;
            }

            if ((primaryHeld || primaryDown) && (secondaryHeld || secondaryDown))
            {
                if (!_isHolding)
                {
                    _isHolding = true;
                    _elapsed = 0f;
                }

                _elapsed += Time.deltaTime;
                if (_elapsed > _requiredTimeRange.y)
                {
                    ResetHold();
                    return ComboInputResult.Wrong;
                }

                visual.SetState(KeyState.Active, null, _elapsed);
                return ComboInputResult.Holding;
            }

            if (_isHolding && (input.IsKeyUp(expectedKey) || input.IsKeyUp(_secondKey)))
            {
                var duration = _elapsed;
                var bothReleased = input.IsKeyUp(expectedKey) && input.IsKeyUp(_secondKey);
                ResetHold();

                if (bothReleased && duration >= _requiredTimeRange.x && duration <= _requiredTimeRange.y)
                {
                    return ComboInputResult.Correct;
                }

                return ComboInputResult.Wrong;
            }

            if (input.AnyOtherKeyDown(expectedKey, _secondKey))
            {
                ResetHold();
                return ComboInputResult.Wrong;
            }

            return ComboInputResult.None;
        }

        private void ResetHold()
        {
            _isHolding = false;
            _elapsed = 0f;
        }
    }
}
