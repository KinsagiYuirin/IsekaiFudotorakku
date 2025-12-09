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
        private bool _pendingCompletion;
        
        public float Progress => _maxAmount <= 0f ? 0f : Mathf.Clamp01(_elapsed / _maxAmount);

        public HoldComboHandler(float requiredTime)
        {
            _maxAmount = requiredTime;
            _requiredTimeRange = new Vector2(requiredTime - 0.5f, requiredTime + 0.5f);
        }
        
        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct, IComboButtonVisual visual)
        {
            if (_pendingCompletion)
            {
                _pendingCompletion = false;
                return ComboInputResult.Complete;
            }
            
            if (input.IsKeyHeld(expectedKey))
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
                _pendingCompletion = true;
                    
                    return ComboInputResult.Wrong;
                }
                visual.SetState(KeyState.Active, null, _elapsed);
                return ComboInputResult.Holding;
            }
            if (input.IsKeyUp(expectedKey))
            {
                if (!_isHolding)
                { 
                    ResetHold();
                    _pendingCompletion = true;
                    return ComboInputResult.None;
                }
                
                var t = _elapsed;
                ResetHold();
                _pendingCompletion = true;
                

                if (t >= _requiredTimeRange.x && t <= _requiredTimeRange.y)
                {
                    return ComboInputResult.Correct;
                }
                //visual.SetState(KeyState.Ideal);
                return ComboInputResult.Wrong;
            }

            if (input.AnyOtherKeyDown(expectedKey))
            {
                ResetHold();
                _pendingCompletion = true;
                
                return ComboInputResult.Wrong;
            }
            
            return ComboInputResult.None;
        }
        
        private void ResetHold()
        {
            _isHolding = false;
        }
    }
}
