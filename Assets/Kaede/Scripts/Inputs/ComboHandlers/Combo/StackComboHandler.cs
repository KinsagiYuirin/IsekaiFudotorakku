using System.Threading;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;

namespace Kaede.Scripts.Inputs.ComboHandlers.Combo
{
    public class StackComboHandler : IComboHandler
    {
        private int _count;
        private readonly int _requiredCount;

        public StackComboHandler(int requiredCount = 3)
        {
            _requiredCount = requiredCount;
        }

        public ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct)
        {
            if (input.IsKeyDown(expectedKey))
            {
                _count++;
                if (_count >= _requiredCount)
                {
                    _count = 0;
                    return ComboInputResult.Correct;
                }
            }
            else if (input.AnyOtherKeyDown(expectedKey))
            {
                _count = 0;
                return ComboInputResult.Wrong;
            }

            return ComboInputResult.None;
        }
    }
}
