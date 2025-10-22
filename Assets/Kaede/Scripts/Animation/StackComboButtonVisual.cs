using System.Collections.Generic;
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
        [System.Serializable]
        private class SpriteEntry
        {
            public ComboKey key = ComboKey.None;
            public Sprite idealSprite;
            public Sprite prepareSprite;
            public Sprite currentSprite;
            public Sprite activeSprite;
        }
        
        [Title("Icon References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text labelText;
        
        [Title("Settings")]
        [SerializeField] private int currentCount = 0;
        [SerializeField] private int maxCount = 5;
        
        [Title("Sprite Settings")]
        [SerializeField] private Sprite fallbackSprite;
        [SerializeField] private SpriteEntry[] spriteEntries;

        private readonly Dictionary<(ComboKey key, KeyState state), Sprite> _spriteLookup = new();
        private ComboKey _currentKey = ComboKey.None;

        
        private void Awake()
        {
            iconImage ??= GetComponent<Image>();
            labelText ??= GetComponentInChildren<TMP_Text>();
        }
        
        public void Initialize(ComboKeySetting comboSetting, string displayKey, bool isStringKey)
        {
            maxCount = comboSetting is { type: ComboType.StackTimer} ? comboSetting.stackCount : 5;
            _currentKey = comboSetting?.key ?? ComboKey.None;
            ApplySprite(KeyState.Ideal);
            
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
            _ = indexFloat;
            ApplySprite(state);
            
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

        private void InitializeSpriteLookup()
        {
            _spriteLookup.Clear();

            if (spriteEntries == null)
            {
                return;
            }

            foreach (var entry in spriteEntries)
            {
                if (entry == null)
                {
                    continue;
                }

                TryRegisterSprite(entry.key, KeyState.Ideal, entry.idealSprite);
                TryRegisterSprite(entry.key, KeyState.Prepare, entry.prepareSprite);
                TryRegisterSprite(entry.key, KeyState.Current, entry.currentSprite);
                TryRegisterSprite(entry.key, KeyState.Active, entry.activeSprite);
            }
        }

        private void TryRegisterSprite(ComboKey key, KeyState state, Sprite sprite)
        {
            if (sprite == null)
            {
                return;
            }

            _spriteLookup[(key, state)] = sprite;
        }

        private void ApplySprite(KeyState state)
        {
            if (iconImage == null)
            {
                iconImage = GetComponent<Image>();
            }

            if (iconImage == null)
            {
                return;
            }

            var sprite = ResolveSprite(_currentKey, state);
            if (sprite != null)
            {
                iconImage.sprite = sprite;
            }
        }
        
        private Sprite ResolveSprite(ComboKey key, KeyState state)
        {
            if (_spriteLookup.Count == 0)
            {
                InitializeSpriteLookup();
            }

            if (_spriteLookup.TryGetValue((key, state), out var sprite) && sprite != null)
            {
                return sprite;
            }

            if (_spriteLookup.TryGetValue((ComboKey.None, state), out var fallback) && fallback != null)
            {
                return fallback;
            }

            return fallbackSprite;
        }
    }
}
