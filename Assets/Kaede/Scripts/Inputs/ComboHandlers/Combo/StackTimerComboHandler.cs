using System.Threading;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;
using UnityEngine;

namespace Kaede.Scripts.Inputs.ComboHandlers.Combo
{
    public class StackTimerComboHandler : IComboHandler
    {
        private float _lastPressTime = -1f;
        private int _currentPressCount;
        private readonly int _requiredPressCount;
        private readonly float _maxDelay;

        public StackTimerComboHandler(float maxDelay = 0.3f, int requiredPressCount = 2)
        {
            _maxDelay = maxDelay;
            _requiredPressCount = requiredPressCount;
        }

        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct)
        {
            if (input.IsKeyDown(expectedKey))
            {
                var time = Time.time;

                if (_currentPressCount == 0 || time - _lastPressTime <= _maxDelay)
                {
                    _currentPressCount++;
                    _lastPressTime = time;

                    if (_currentPressCount >= _requiredPressCount)
                    {
                        _currentPressCount = 0;
                        _lastPressTime = -1f;
                        return ComboInputResult.Correct;
                    }
                }
                else
                {
                    _currentPressCount = 1;
                    _lastPressTime = time;
                }
            }
            else if (input.AnyOtherKeyDown(expectedKey))
            {
                _currentPressCount = 0;
                _lastPressTime = -1f;
                return ComboInputResult.Wrong;
            }

            return ComboInputResult.None;
        }
    }
}
