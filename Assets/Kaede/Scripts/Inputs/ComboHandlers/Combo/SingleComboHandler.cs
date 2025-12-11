using System.Threading;
using Kaede.Scripts.Animation;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;
using UnityEngine;

namespace Kaede.Scripts.Inputs.ComboHandlers.Combo
{
    public class SingleComboHandler : IComboHandler
    {
        private bool _isActionDone; 

        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct, IComboButtonVisual visual)
        {
            if (_isActionDone)
            {
                if (input.IsKeyUp(expectedKey) || !input.IsKeyDown(expectedKey))
                {
                    _isActionDone = false;
                    return ComboInputResult.Complete;
                }
                return ComboInputResult.None;
            }

            if (input.IsKeyDown(expectedKey))
            {
                _isActionDone = true;
                return ComboInputResult.Correct;
            }

            if (input.AnyOtherKeyDown(expectedKey))
            {
                _isActionDone = true;
                return ComboInputResult.Wrong;
            }
            
            return ComboInputResult.None;
        }
    }
}