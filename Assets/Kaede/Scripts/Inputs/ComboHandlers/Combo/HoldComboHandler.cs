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
                if (input.IsKeyUp(expectedKey))
                {
                    switch (_elapsed)
                    {
                        case var t when t >= _requiredTimeRange.x && t <= _requiredTimeRange.y:
                            _elapsed = 0f;
                            return ComboInputResult.Correct;
                        
                        case var t when t < _requiredTimeRange.x:
                            _elapsed = 0f;
                            return ComboInputResult.Wrong;
                        
                        case var t when t > _requiredTimeRange.y:
                            _elapsed = 0f;
                            return ComboInputResult.Wrong;
                    }
                }
            }
            else if (input.AnyOtherKeyDown(expectedKey))
            {
                _elapsed = 0f;
                return ComboInputResult.Wrong;
            }
            else
            {
                _elapsed = 0f;
            }
            return ComboInputResult.None;
        }
    }
}
