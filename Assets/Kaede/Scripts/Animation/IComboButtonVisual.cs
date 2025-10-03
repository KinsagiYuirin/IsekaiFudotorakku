using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using UnityEngine;

namespace Kaede.Scripts.Animation
{
    public interface IComboButtonVisual
    {
        void Initialize(ComboKeySetting comboSetting, string displayKey);
        void SetState(KeyState state, Sprite sprite);
        void SetColor(Color color);
    }
}
