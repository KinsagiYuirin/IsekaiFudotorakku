using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Animation
{
    public class StackComboButtonVisual : MonoBehaviour, IComboButtonVisual
    {
        [Title("Icon References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text labelText;
        
        [Title("Settings")]
        [SerializeField] private int currentCount = 0;
        [SerializeField] private int maxCount = 5;
        
        private void Awake()
        {
            iconImage ??= GetComponent<Image>();
            labelText ??= GetComponentInChildren<TMP_Text>();
        }
        
        public void Initialize(ComboKeySetting comboSetting, string displayKey)
        {
            maxCount = comboSetting is { type: ComboType.StackTimer} ? comboSetting.stackCount : 5;
            
            if (labelText != null)
            {
                labelText.text = displayKey;
            }
        }

        public void SetState(KeyState state, int? index, float? indexFloat)
        {       
            _ = indexFloat;
            
            switch (state)
            {
                case KeyState.Active:
                    break;
                case KeyState.Current:
                    break;
                case KeyState.Prepare:
                    break;
                case KeyState.Ideal:
                    break;
                default:
                    break;
            }
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
            if (iconImage != null)
            {
                iconImage.sprite = sprite;
            }
        }
    }
}
