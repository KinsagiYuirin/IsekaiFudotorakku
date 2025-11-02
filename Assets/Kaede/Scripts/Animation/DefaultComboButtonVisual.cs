using System.Collections.Generic;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using Kaede.Scripts.Utils;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Animation
{
    [DisallowMultipleComponent]
    public class DefaultComboButtonVisual : MonoBehaviour, IComboButtonVisual
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
        
        [Title("Object References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image lightImage;
        [SerializeField] private TMP_Text labelText;

        [Title("Animation Setting")] 
        [SerializeField] private float scaleUp;
        [SerializeField] private float scaleDown;
        [SerializeField] private float durationPhase1;
        [SerializeField] private float durationPhase2;
        
        [Title("Sprite Settings")]
        [SerializeField] private Sprite fallbackSprite;
        [SerializeField] private Sprite grayBackGroundSprite;
        [SerializeField] private Sprite lightBackGroundSprite;
        [SerializeField] private SpriteEntry[] spriteEntries;

        private Tween scaleTween;
        private Sequence pressSequence;
        private Vector3 originalScale;
        private readonly Dictionary<(ComboKey key, KeyState state), Sprite> _spriteLookup = new();
        private ComboKey _currentKey = ComboKey.None;
        
        private void Awake()
        {
            iconImage ??= GetComponent<Image>();
            labelText ??= GetComponentInChildren<TMP_Text>();
            originalScale =  iconImage.transform.localScale;
            InitializeSpriteLookup();
        }

        public void Initialize(ComboKeySetting comboSetting, string displayKey, bool isStringKey)
        {
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
            _ = state;
            _ = index;
            _ = indexFloat;

            ApplySprite(state);

            switch (state)
            {
                case KeyState.Current:
                    UpdateLightAlpha(1f);
                    break;
                
                case KeyState.Prepare:
                    UpdateLightAlpha(0.2f, grayBackGroundSprite);
                    break;
                
                case KeyState.Ideal: 
                    UpdateLightAlpha(0f);
                    break;
                case KeyState.Active:
                    UpdateLightAlpha(0f, active: false);
                    OnPointDownAnimation();
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
        
        private void UpdateLightAlpha(float alpha, Sprite sprite = null, bool active = true)
        {
            if (active)
            {
                lightImage.sprite = sprite ?? lightBackGroundSprite;
                if (lightImage != null)
                {
                    Color color = lightImage.color;
                    color.a = alpha;
                    lightImage.color = color;
                }
            }
            else
            {
                lightImage.gameObject.SetActive(false);
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

        private void OnPointDownAnimation()
        {
            pressSequence = Sequence.Create();
            pressSequence.Chain(Tween.LocalScale(iconImage.rectTransform, originalScale, originalScale * scaleUp, durationPhase1));
            pressSequence.Chain(Tween.LocalScale(iconImage.rectTransform, originalScale * scaleUp, originalScale * scaleDown, durationPhase2));
        }
    }
}

