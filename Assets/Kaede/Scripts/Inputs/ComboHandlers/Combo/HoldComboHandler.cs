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
        public float Progress => _maxAmount <= 0f ? 0f : Mathf.Clamp01(_elapsed / _maxAmount);

        public HoldComboHandler(float requiredTime)
        {
            _maxAmount = requiredTime;
            _requiredTimeRange = new Vector2(requiredTime - 0.5f, requiredTime + 0.5f);
        }
        
        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct, IComboButtonVisual visual)
        {
            if (input.IsKeyHeld(expectedKey))
            {
                _elapsed += Time.deltaTime;
                visual.SetState(KeyState.Active);
                if (_elapsed > _requiredTimeRange.y)
                {
                    _elapsed = 0f;
                    return ComboInputResult.Wrong;
                }
            }
            if (input.IsKeyUp(expectedKey))
            {
                var t = _elapsed;
                _elapsed = 0f;
                visual.SetState(KeyState.Ideal);

                if (t >= _requiredTimeRange.x && t <= _requiredTimeRange.y)
                {
                    return ComboInputResult.Correct;
                }
                return ComboInputResult.Wrong;
            }

            if (!input.AnyOtherKeyDown(expectedKey)) return ComboInputResult.None;
            _elapsed = 0f;
            return ComboInputResult.Wrong;
        }
    }
}
