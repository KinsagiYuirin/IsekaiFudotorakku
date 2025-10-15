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
        [SerializeField] private Image lightImage;
        [SerializeField] private TMP_Text labelText;

        private void Awake()
        {
            iconImage ??= GetComponent<Image>();
            labelText ??= GetComponentInChildren<TMP_Text>();
        }

        public void Initialize(ComboKeySetting comboSetting, string displayKey, bool isStringKey)
        {
            _ = comboSetting;

            if (!isStringKey)
            {
                labelText.alpha = 0f;
            }
            else
            {
                labelText.alpha = 1f;
                if (labelText != null)
                {
                    labelText.text = displayKey;
                }
            }
        }

        public void SetState(KeyState state, int? index, float? indexFloat)
        {
            _ = state;

            switch (state)
            {
                case KeyState.Current:
                    UpdateLightAlpha(1f);
                    break;
                
                case KeyState.Prepare:
                    UpdateLightAlpha(0.5f);
                    break;
                
                case KeyState.Ideal: 
                case KeyState.Active:
                    UpdateLightAlpha(0f);
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
        
        private void UpdateLightAlpha(float alpha)
        {
            if (lightImage != null)
            {
                Color color = lightImage.color;
                color.a = alpha;
                lightImage.color = color;
            }
        }
    }
}

