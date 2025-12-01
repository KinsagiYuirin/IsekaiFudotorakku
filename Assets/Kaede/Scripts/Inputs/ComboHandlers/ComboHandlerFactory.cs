using Kaede.Scripts.Inputs.ComboHandlers.Combo;
using Kaede.Scripts.Item;

namespace Kaede.Scripts.Inputs.ComboHandlers
{
    public static class ComboHandlerFactory
    {
        public static IComboHandler Create(ComboKeySetting setting)
        {
            return setting.type switch
            {
                ComboType.Single => new SingleComboHandler(),
                ComboType.StackTimer => new StackTimerComboHandler(setting.buttonDuration, setting.pressCount),
                ComboType.Hold   => new HoldComboHandler(setting.holdTime),
                ComboType.Stack  => new StackComboHandler(setting.stackCount),
                ComboType.DualKey => new DualComboHandler(setting.secondKey),
                ComboType.DualKeyHold => new DualHoldComboHandler(setting.dualHoldTime, setting.secondKey),
                _                => new SingleComboHandler(),
            };
        }
    }
}
