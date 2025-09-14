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
                ComboType.StackTimer => new StackTimerComboHandler(setting.doubleTapDelay, setting.pressCount),
                ComboType.Hold   => new HoldComboHandler(setting.holdTime),
                ComboType.Stack  => new StackComboHandler(setting.stackCount),
                _                => new SingleComboHandler(),
            };
        }
    }
}
