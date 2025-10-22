using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using UnityEngine;

namespace Kaede.Scripts.Animation
{
    public interface IComboButtonVisual
    {
        void Initialize(ComboKeySetting comboSetting, string displayKey, bool isStringKey);
        void SetState(KeyState state, int? index, float? indexFloat);
        void SetColor(Color color);
    }
}
