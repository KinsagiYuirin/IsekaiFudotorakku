using System.Threading;
using Kaede.Scripts.Animation;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;
using UnityEngine;

namespace Kaede.Scripts.Inputs.ComboHandlers.Combo
{
    public class StackTimerComboHandler : IComboHandler
    {
        private float _comboStartTime = -1f;
        private int _currentPressCount;
        private bool _hasReachedRequired;
        private readonly int _requiredPressCount;
        private readonly float _maxDuration;

        public StackTimerComboHandler(float maxDuration = 0.3f, int requiredPressCount = 2)
        {
            _maxDuration = maxDuration;
            _requiredPressCount = requiredPressCount;
        }

        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct, IComboButtonVisual visual)
        {
            if (_comboStartTime >= 0f && Time.time > _comboStartTime + _maxDuration)
            {
                var result = _hasReachedRequired ? ComboInputResult.Correct : ComboInputResult.Wrong;
                ResetCombo();
                return result;
            }
            
            if (input.IsKeyDown(expectedKey))
            {
                var time = Time.time;

                if (_comboStartTime < 0f)
                {
                    _comboStartTime = time;
                    _currentPressCount = 0;
                    _hasReachedRequired = false;
                }
                
                _currentPressCount++;

                if (!_hasReachedRequired && _currentPressCount >= _requiredPressCount)
                {
                    _hasReachedRequired = true;
                }
                return ComboInputResult.None;
            }

            if (!input.AnyOtherKeyDown(expectedKey)) return ComboInputResult.None;
            ResetCombo();
            return ComboInputResult.Wrong;
        }
        
        private void ResetCombo()
        {
            _comboStartTime = -1f;
            _currentPressCount = 0;
            _hasReachedRequired = false;
        }
    }
}
