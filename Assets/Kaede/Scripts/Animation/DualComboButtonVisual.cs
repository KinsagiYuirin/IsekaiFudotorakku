using System.Collections;
using System.Collections.Generic;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede.Scripts.Animation
{
    [DisallowMultipleComponent]
    public class DualComboButtonVisual : MonoBehaviour, IComboButtonVisual
    {
        [System.Serializable]
        private class SpriteEntry
        {
            [Title("Keyboard Key")]
            public ComboKey key = ComboKey.None;

            public enum KeySite
            {
                Left,
                Right
            }

            [LabelText("Side")]
            public KeySite site = KeySite.Left;

            public Sprite idealSprite;
            public Sprite prepareSprite;
            public Sprite currentSprite;
            public Sprite activeSprite;
        }
        
        [Title("Icon References")]
        [SerializeField] private Image iconImage1;   // ซ้าย = ปุ่มแรก
        [SerializeField] private Image iconImage2;   // ขวา = ปุ่มสอง
        [SerializeField] private TMP_Text labelText;

        [Title("Animation Setting")] 
        [SerializeField] private float scaleUp = 1.2f;
        [SerializeField] private float scaleDown = 1.0f;
        [SerializeField] private float durationPhase1 = 0.1f;
        [SerializeField] private float durationPhase2 = 0.1f;

        [Title("Sprite Settings")]
        [SerializeField] private Sprite fallbackSprite;
        //[SerializeField] private Sprite grayBackGroundSprite; // ไม่ได้ใช้ใน Logic กดครั้งเดียว
        //[SerializeField] private Sprite lightBackGroundSprite; // ไม่ได้ใช้ใน Logic กดครั้งเดียว
        [SerializeField] private SpriteEntry[] spriteEntries;
        [SerializeField] private SpriteEntry[] gamepadSpriteEntries;

        private Sequence pressSequence;
        private Vector3 originalScale1;
        private Vector3 originalScale2;
            
        [SerializeField, DisplayAsString] private ComboKey _leftKey = ComboKey.None;
        [SerializeField, DisplayAsString] private ComboKey _rightKey = ComboKey.None;
        
        private readonly Dictionary<(ComboKey key, SpriteEntry.KeySite site, KeyState state), Sprite> _spriteLookup = new();
        
        private void Awake()
        {
            iconImage1 ??= GetComponent<Image>();
            iconImage2 ??= GetComponent<Image>();
            labelText ??= GetComponentInChildren<TMP_Text>();
            
            // เก็บขนาดเริ่มต้นของทั้งสองไอคอนไว้สำหรับทำ Animation
            if (iconImage1 != null) originalScale1 = iconImage1.transform.localScale;
            if (iconImage2 != null) originalScale2 = iconImage2.transform.localScale;

            InitializeSpriteLookup();
        }

        public void Initialize(ComboKeySetting comboSetting, string displayKey, bool isStringKey)
        {
            // ไม่มีการคำนวณ Duration แล้ว เพราะเป็นปุ่มกดธรรมดา

            // Map ปุ่ม ซ้าย/ขวา
            if (comboSetting.type == ComboType.DualKeyHold || comboSetting.type == ComboType.DualKey)
            {
                _leftKey  = comboSetting.key;        // ปุ่มแรก (ซ้าย)
                _rightKey = comboSetting.secondKey;  // ปุ่มสอง (ขวา)
            }
            else
            {
                _leftKey  = comboSetting.key;
                _rightKey = comboSetting.key;
            }
            
            ApplySprite(KeyState.Ideal);
            
            if (!isStringKey)
            {
                if (labelText != null)
                    labelText.alpha = 0f;
            }
            else
            {
                if (labelText != null)
                {
                    labelText.alpha = 1f;
                    labelText.text = displayKey;
                }
            }
        }

        public void SetState(KeyState state, int? index, float? indexFloat)
        {
            _ = index;
            _ = indexFloat;
            
            ApplySprite(state);

            switch (state)
            {
                case KeyState.Active:
                    // เมื่อกดสำเร็จ ให้เล่น Animation เด้งดึ๋ง (เหมือน DefaultComboButtonVisual)
                    OnPointDownAnimation();
                    break;
                case KeyState.Current:
                case KeyState.Prepare:
                case KeyState.Ideal:
                default:
                    // สถานะปกติแค่เปลี่ยน Sprite (จัดการใน ApplySprite แล้ว)
                    break;
            }
        }

        public void SetColor(Color color)
        {
            if (iconImage1 != null)
                iconImage1.color = color;
            if (iconImage2 != null)
                iconImage2.color = color;
        }

        private void OnPointDownAnimation()
        {
            // หยุด Sequence เดิมถ้ามี
            if (pressSequence.isAlive) pressSequence.Stop();
            
            pressSequence = Sequence.Create();

            // Animate Icon 1 (Left)
            if (iconImage1 != null)
            {
                pressSequence.Group(Tween.LocalScale(iconImage1.transform, originalScale1, originalScale1 * scaleUp, durationPhase1));
                pressSequence.Group(Tween.LocalScale(iconImage1.transform, originalScale1 * scaleUp, originalScale1 * scaleDown, durationPhase2, startDelay: durationPhase1));
            }

            // Animate Icon 2 (Right)
            if (iconImage2 != null)
            {
                pressSequence.Group(Tween.LocalScale(iconImage2.transform, originalScale2, originalScale2 * scaleUp, durationPhase1));
                pressSequence.Group(Tween.LocalScale(iconImage2.transform, originalScale2 * scaleUp, originalScale2 * scaleDown, durationPhase2, startDelay: durationPhase1));
            }
        }
        
        private void InitializeSpriteLookup()
        {
            _spriteLookup.Clear();

            if (spriteEntries == null)
            {
                return;
            }

            // ถ้ามี gamepad ใช้ gamepadSpriteEntries แทน
            if (Gamepad.current != null && gamepadSpriteEntries != null && gamepadSpriteEntries.Length > 0)
            {
                foreach (var entry in gamepadSpriteEntries)
                {
                    if (entry == null) continue;
                    RegisterEntry(entry);
                }
            }
            else
            {
                foreach (var entry in spriteEntries)
                {
                    if (entry == null) continue;
                    RegisterEntry(entry);
                }
            }
        }

        private void RegisterEntry(SpriteEntry entry)
        {
            TryRegisterSprite(entry.key, entry.site, KeyState.Ideal,   entry.idealSprite);
            TryRegisterSprite(entry.key, entry.site, KeyState.Prepare, entry.prepareSprite);
            TryRegisterSprite(entry.key, entry.site, KeyState.Current, entry.currentSprite);
            TryRegisterSprite(entry.key, entry.site, KeyState.Active,  entry.activeSprite);
        }

        private void TryRegisterSprite(ComboKey key, SpriteEntry.KeySite site, KeyState state, Sprite sprite)
        {
            if (sprite == null) return;
            _spriteLookup[(key, site, state)] = sprite;
        }

        private void ApplySprite(KeyState state)
        {
            if (iconImage1 == null && iconImage2 == null)
            { 
                return;
            }
            
            // ซ้าย = Left, ขวา = Right
            if (iconImage1 != null)
            {
                var leftSprite = ResolveSprite(_leftKey, SpriteEntry.KeySite.Left, state);
                if (leftSprite != null)
                    iconImage1.sprite = leftSprite;
            }

            if (iconImage2 != null)
            {
                var rightSprite = ResolveSprite(_rightKey, SpriteEntry.KeySite.Right, state);
                if (rightSprite != null)
                    iconImage2.sprite = rightSprite;
            }
        }

        private Sprite ResolveSprite(ComboKey key, SpriteEntry.KeySite site, KeyState state)
        {
            if (_spriteLookup.Count == 0)
            {
                InitializeSpriteLookup();
            }

            // ตาม key + site + state ก่อน
            if (_spriteLookup.TryGetValue((key, site, state), out var sprite) && sprite != null)
            {
                return sprite;
            }

            // fallback: ใช้ ComboKey.None แต่ยังคง site (ซ้าย/ขวา)
            if (_spriteLookup.TryGetValue((ComboKey.None, site, state), out var fallback) && fallback != null)
            {
                return fallback;
            }

            return fallbackSprite;
        }
    }
}
