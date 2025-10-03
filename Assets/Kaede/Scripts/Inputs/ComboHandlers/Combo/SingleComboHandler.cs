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
        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct, IComboButtonVisual visual)
        {
            if (input.IsKeyDown(expectedKey))
                return ComboInputResult.Correct;

            if (input.AnyOtherKeyDown(expectedKey))
            {
                return ComboInputResult.Wrong;
            }
            
            return ComboInputResult.None;
        }
    }
}