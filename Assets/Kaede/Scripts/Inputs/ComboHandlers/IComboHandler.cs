using System.Threading;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;

namespace Kaede.Scripts.Inputs.ComboHandlers
{
    public interface IComboHandler
    {
        ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct);
    }
}