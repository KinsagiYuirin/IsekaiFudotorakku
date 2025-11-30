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
        public float Progress => _maxAmount <= 0f ? 0f : Mathf.Clamp01(_elapsed / _maxAmount);
        
        public DualHoldComboHandler(float requiredTime)
        {
            _maxAmount = requiredTime;
            _requiredTimeRange = new Vector2(requiredTime - 0.5f, requiredTime + 0.5f);
        }
        
        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, 
            CancellationToken ct, IComboButtonVisual visual)
        {
            throw new System.NotImplementedException();
        }
    }
}
