using System.Threading;
using Kaede.Scripts.Animation;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using Kaede.Scripts.Utils;
using MadDuck.Scripts.Inputs;
using UnityEngine;

namespace Kaede.Scripts.Inputs.ComboHandlers.Combo
{
    public class HoldComboHandler : IComboHandler
    {
        private float _elapsed;
        private readonly float _maxAmount;
        private readonly Vector2 _requiredTimeRange;
        private bool _isHolding;
        public float Progress => _maxAmount <= 0f ? 0f : Mathf.Clamp01(_elapsed / _maxAmount);

        public HoldComboHandler(float requiredTime)
        {
            _maxAmount = requiredTime;
            _requiredTimeRange = new Vector2(requiredTime - 1f, requiredTime + 1f);
        }
        
        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct, IComboButtonVisual visual)
        {
            if (input.IsKeyHeld(expectedKey))
            {
                _elapsed += Time.deltaTime;
                if (_elapsed > _requiredTimeRange.y)
                {
                    _elapsed = 0f;
                    return ComboInputResult.Wrong;
                }
                visual.SetState(KeyState.Active, null, _elapsed);
                return ComboInputResult.Holding;
            }
            if (input.IsKeyUp(expectedKey))
            {
                var t = _elapsed;
                _elapsed = 0f;

                if (t >= _requiredTimeRange.x && t <= _requiredTimeRange.y)
                {
                    return ComboInputResult.Correct;
                }
                //visual.SetState(KeyState.Ideal);
                return ComboInputResult.Wrong;
            }

            if (!input.AnyOtherKeyDown(expectedKey)) return ComboInputResult.None;
            _elapsed = 0f;
            return ComboInputResult.Wrong;
        }
    }
}
