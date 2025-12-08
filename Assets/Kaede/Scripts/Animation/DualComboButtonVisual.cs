using System.Collections;
using System.Collections.Generic;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
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

        [Header("Slide Bar")]
        [SerializeField] private Image fillImage;
        [SerializeField] private float updateSpeed = 1f;
        [SerializeField, DisplayAsString] private float currentIndexPercent = 1f;
        
        [Title("Dynamic Sizing")]
        [SerializeField] private bool useDynamicSizing = false;   // สำหรับ Dual ปล่อย false ปุ่มจะไม่ยืด
        [SerializeField] private float baseWidth = 100f;
        [SerializeField] private float pixelsPerSecond = 40f;
        [SerializeField] private Vector2 sizeRange = new Vector2(80f, 400f);
        [SerializeField] private LayoutElement layoutElement;

        [Title("Sprite Settings")]
        [SerializeField] private Sprite fallbackSprite;
        [SerializeField] private SpriteEntry[] spriteEntries;
        [SerializeField] private SpriteEntry[] gamepadSpriteEntries;
        
        private RectTransform _rectTransform;
        private RectTransform _fillImageRect;

        [Header("Details")]
        [SerializeField] private bool needSmoothFill = true;

        [SerializeField, DisplayAsString] private float holdDuration;
        private bool _isHolding;

        // คีย์ซ้าย–ขวา (ปุ่มแรก/ปุ่มสอง)
        [SerializeField, DisplayAsString] private ComboKey _leftKey = ComboKey.None;
        [SerializeField, DisplayAsString] private ComboKey _rightKey = ComboKey.None;
        
        // 🔸 ใช้ KeySite ใน dictionary ด้วย
        private readonly Dictionary<(ComboKey key, SpriteEntry.KeySite site, KeyState state), Sprite> _spriteLookup = new();
        
        private void Awake()
        {
            fillImage ??= GetComponentInChildren<Image>();
            labelText ??= GetComponentInChildren<TMP_Text>();
            
            InitializeSpriteLookup();

            _rectTransform = GetComponent<RectTransform>();
            if (fillImage != null)
                _fillImageRect = fillImage.GetComponent<RectTransform>();

            if (layoutElement == null)
            {
                layoutElement = GetComponent<LayoutElement>();
                if (layoutElement == null)
                {
                    layoutElement = gameObject.AddComponent<LayoutElement>();
                }
            }

            ResetProgress();
        }

        public void Initialize(ComboKeySetting comboSetting, string displayKey, bool isStringKey)
        {
            holdDuration = comboSetting switch
            {
                { type: ComboType.Hold }        => comboSetting.holdTime,
                { type: ComboType.DualKeyHold } => comboSetting.dualHoldTime,
                _                               => 1f
            };

            // ตรงนี้ map ให้ตรงกับ ComboKeySetting ของโปรเจกต์จริง
            if (comboSetting.type == ComboType.DualKeyHold)
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

            UpdateLayoutElementSize();
            ResetProgress();
        }

        public void SetState(KeyState state, int? index, float? indexFloat)
        {
            _ = index;
            ApplySprite(state);

            switch (state)
            {
                case KeyState.Active:
                    UpdateProgress(indexFloat ?? 0f); // indexFloat = elapsedTime
                    break;
                case KeyState.Current:
                case KeyState.Prepare:
                    _isHolding = false;
                    break;
                case KeyState.Ideal:
                default:
                    ResetProgress();
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
        
        private void UpdateProgress(float elapsedTime)
        {
            if (!_isHolding)
            {
                _isHolding = true;
                ApplyFill(0f);
            }

            var targetPercent = holdDuration <= 0f 
                ? 1f 
                : Mathf.Clamp01(elapsedTime / holdDuration);
            ApplyFill(targetPercent);
        }

        private void ApplyFill(float targetPercent)
        {
            targetPercent = Mathf.Clamp01(targetPercent);

            if (needSmoothFill && updateSpeed > Mathf.Epsilon)
            {
                var t = 1f - Mathf.Exp(-updateSpeed * Time.deltaTime);
                currentIndexPercent = Mathf.Lerp(currentIndexPercent, targetPercent, t);
            }
            else
            {
                currentIndexPercent = targetPercent;
            }

            if (fillImage != null)
            {
                fillImage.fillAmount = currentIndexPercent;
            }
        }
        
        private void ResetProgress()
        {
            _isHolding = false;
            currentIndexPercent = 0f;
            ApplyFill(0f);
        }
            
        private void UpdateLayoutElementSize()
        {
            // ถ้าไม่อยากให้ปุ่มยืด ปล่อย useDynamicSizing = false
            if (!useDynamicSizing)
            {
                SetupFillImageAnchors();
                StartCoroutine(RebuildLayoutNextFrame());
                return;
            }

            var calculatedWidth = baseWidth + (holdDuration * pixelsPerSecond);
            var targetWidth = Mathf.Clamp(calculatedWidth, sizeRange.x, sizeRange.y);
            
            Debug.Log($"Calculating size: Duration={holdDuration}s, Target Width={targetWidth}px");
            
            if (layoutElement != null)
            {
                layoutElement.preferredWidth = targetWidth;
                layoutElement.flexibleWidth  = 0f;
                layoutElement.minWidth       = targetWidth;
            }
            
            if (_rectTransform != null)
            {
                var sizeDelta = _rectTransform.sizeDelta;
                sizeDelta.x = targetWidth;
                _rectTransform.sizeDelta = sizeDelta;
                
                Debug.Log($"RectTransform size set to: {_rectTransform.sizeDelta.x}");
            }
            
            SetupFillImageAnchors();
            StartCoroutine(RebuildLayoutNextFrame());
        }

        private void SetupFillImageAnchors()
        {
            if (_fillImageRect == null) return;
            
            Debug.Log("Setting up fillImage anchors...");
            
            _fillImageRect.anchorMin        = new Vector2(0f, 0.5f);
            _fillImageRect.anchorMax        = new Vector2(1f, 0.5f);
            _fillImageRect.anchoredPosition = Vector2.zero;
            
            var fillSize = _fillImageRect.sizeDelta;
            fillSize.y = gameObject.GetComponent<RectTransform>().sizeDelta.y;
            fillSize.x = 0f;
            _fillImageRect.sizeDelta = fillSize;
            
            _fillImageRect.offsetMin = new Vector2(2f, -fillSize.y / 2f);
            _fillImageRect.offsetMax = new Vector2(-2f, fillSize.y / 2f);
            
            Debug.Log($"FillImage anchor set: {_fillImageRect.anchorMin} to {_fillImageRect.anchorMax}");
        }

        private IEnumerator RebuildLayoutNextFrame()
        {
            yield return null;
            
            Debug.Log("Rebuilding layout...");
            
            if (transform.parent != null)
            {
                var parentRect = transform.parent as RectTransform;
                if (parentRect != null)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate(parentRect);
                    Debug.Log("Parent layout rebuilt");
                }
            }
            
            if (_rectTransform != null)
                LayoutRebuilder.ForceRebuildLayoutImmediate(_rectTransform);

            Debug.Log($"Final sizes - GameObject: {_rectTransform.sizeDelta.x}, FillImage: {_fillImageRect?.sizeDelta.x}");
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

                    TryRegisterSprite(entry.key, entry.site, KeyState.Ideal,   entry.idealSprite);
                    TryRegisterSprite(entry.key, entry.site, KeyState.Prepare, entry.prepareSprite);
                    TryRegisterSprite(entry.key, entry.site, KeyState.Current, entry.currentSprite);
                    TryRegisterSprite(entry.key, entry.site, KeyState.Active,  entry.activeSprite);
                }
            }
            else
            {
                foreach (var entry in spriteEntries)
                {
                    if (entry == null) continue;

                    TryRegisterSprite(entry.key, entry.site, KeyState.Ideal,   entry.idealSprite);
                    TryRegisterSprite(entry.key, entry.site, KeyState.Prepare, entry.prepareSprite);
                    TryRegisterSprite(entry.key, entry.site, KeyState.Current, entry.currentSprite);
                    TryRegisterSprite(entry.key, entry.site, KeyState.Active,  entry.activeSprite);
                }
            }
        }

        private void TryRegisterSprite(ComboKey key, SpriteEntry.KeySite site, KeyState state, Sprite sprite)
        {
            if (sprite == null) return;

            _spriteLookup[(key, site, state)] = sprite;
        }

        private void ApplySprite(KeyState state)
        {
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

