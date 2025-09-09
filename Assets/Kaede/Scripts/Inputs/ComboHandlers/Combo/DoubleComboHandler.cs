using System.Threading;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;
using UnityEngine;

namespace Kaede.Scripts.Inputs.ComboHandlers.Combo
{
    public class DoubleComboHandler : IComboHandler
    {
        private float _lastPressTime = -1f;
        private int _pressCount = 0;
        private readonly float _maxDelay;

        public DoubleComboHandler(float maxDelay = 0.3f, int requiredPressCount = 2)
        {
            _maxDelay = maxDelay;
            _pressCount = requiredPressCount;
        }

        public async UniTask<ComboInputResult> CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct)
        {
            if (input.IsKeyDown(expectedKey))
            {
                if (Time.time - _lastPressTime <= _maxDelay)
                {
                    _lastPressTime = -1f;
                    return ComboInputResult.Correct;
                }
                _lastPressTime = Time.time;
            }
            else if (input.AnyOtherKeyDown(expectedKey))
            {
                return ComboInputResult.Wrong;
            }

            return ComboInputResult.None;
        }
    }
}

