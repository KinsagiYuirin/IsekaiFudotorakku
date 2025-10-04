using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using Kaede.Scripts.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Animation
{
    [DisallowMultipleComponent]
    public class DefaultComboButtonVisual : MonoBehaviour, IComboButtonVisual
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text labelText;

        private void Awake()
        {
            iconImage ??= GetComponent<Image>();
            labelText ??= GetComponentInChildren<TMP_Text>();
        }

        public void Initialize(ComboKeySetting comboSetting, string displayKey)
        {
            _ = comboSetting;

            if (labelText != null)
            {
                labelText.text = displayKey;
            }
        }

        public void SetState(KeyState state, int? index, float? indexFloat)
        {
            _ = state;
        }

        public void SetColor(Color color)
        {
            if (iconImage != null)
            {
                iconImage.color = color;
            }
        }
        
        public void SetSprite(Sprite sprite)
        {
            if (!iconImage)
            {
                iconImage.sprite = sprite;
            }
        }
    }
}

