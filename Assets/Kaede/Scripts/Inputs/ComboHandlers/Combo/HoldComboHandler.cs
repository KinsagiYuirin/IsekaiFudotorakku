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

        public HoldComboHandler(float requiredTime)
        {
            _requiredTime = requiredTime;
        }

        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct)
        {
            if (input.IsKeyHeld(expectedKey))
            {
                _elapsed += Time.deltaTime;
                if (_elapsed >= _requiredTime)
                {
                    _elapsed = 0f;
                    return ComboInputResult.Correct;
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
