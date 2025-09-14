using System.Threading;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;
using UnityEngine;

namespace Kaede.Scripts.Inputs.ComboHandlers.Combo
{
    public class HoldComboHandler : IComboHandler
    {
        private float _elapsed;
        private readonly float _requiredTime;
        private readonly Vector2 _requiredTimeRange;

        
        public HoldComboHandler(float requiredTime)
        {
            _requiredTime = requiredTime;
            _requiredTimeRange = new Vector2(requiredTime - 0.5f, requiredTime + 0.5f);
        }

        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct)
        {
            if (input.IsKeyHeld(expectedKey))
            {
                _elapsed += Time.deltaTime;
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
