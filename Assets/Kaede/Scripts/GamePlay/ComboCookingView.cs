using System;
using System.Collections.Generic;
using Kaede.Scripts.Item;
using Sirenix.OdinInspector;
using TMPro;
using Unity.VisualScripting;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede.Scripts.GamePlay
{
    public enum KeyState
    {
        Current,
        Prepare,
        Ideal
    }
    
    [Serializable]
    public class ButtonSprite
    {
        public ComboType comboType;
        public KeyState state;
        public Sprite sprite;
    }
    
    [Serializable]
    public class KeySpriteMapping
    {
        public ComboKey key;
        public Sprite sprite;
    }
    
    public class ComboCookingView : MonoSingleton<ComboCookingView>
    {
        [Title("UI References")]
        [field: SerializeField] public TMP_Text TimerText { get; private set; }
        [field: SerializeField] public Transform ComboPanel { get; private set; }
        [field: SerializeField] public Sprite CookingImage { get; private set; }
        [field: SerializeField] public GameObject CookingImageObject { get; private set; }
        
        [Title("Prefabs")]
        [SerializeField] private GameObject keyIconPrefab;
        
        [Title("Button Sprites")]
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private ButtonSprite[] buttonSprites;
        [SerializeField] private Sprite currentButtonSprite;
        [SerializeField] private Sprite prepareButtonSprite;
        [SerializeField] private Sprite idealButtonSprite;
        
        [Title("Key Mappings")]
        [field: SerializeField] public List<KeySpriteMapping> KeySprite { get; private set; }
        
        [Title("Debug")]
        [field: SerializeField, DisplayAsString] public int CurrentMenuIndex { get; private set; }
        
        private Dictionary<ComboKey, Sprite> _spriteLookup;

        #region Unity Lifecycle
        protected override void Awake()
        {
            InitializeSpriteLookup();
            base.Awake();
        }
        #endregion

        #region Public Methods
        public void ShowCombo(List<ComboKey> keys)
        {
            ClearComboPanel();
            CreateKeyIcons(keys);
        }
        
        public void CurrentKeyPressed(int comboIndex)
        {
            SetKeySprite(comboIndex, currentButtonSprite);
            PrepareNextButton(comboIndex);
        }

        public void NoneKeyPressed(int comboIndex)
        {
            SetKeyColor(comboIndex, Color.white);
        }
        
        public void PressCorrectKey(int comboIndex)
        {
            SetKeyColor(comboIndex, Color.green);
        }

        public void PressWrongKey(int comboIndex)
        {
            SetKeyColor(comboIndex, Color.red);
        }
        
        public void CompleteCombo()
        {
            SetAllKeysColor(Color.yellow);
        }

        public void SetCookingImage(Sprite sprite)
        {
            if (sprite == null || CookingImage == null) return;
            CookingImage = sprite;
        }
        
        public void ClearCombo()
        {
            ClearComboPanel();
            CurrentMenuIndex = 0;
        }
        
        public void ResetCombo()
        {
            CurrentMenuIndex = 0;
            SetAllKeysColor(Color.white);
        }

        public void SetRestingMode(bool isResting)
        {
            SetUIElementActive(ComboPanel?.gameObject, !isResting);
            SetUIElementActive(CookingImageObject?.gameObject, !isResting);
        }
        #endregion

        #region Private Methods
        private void InitializeSpriteLookup()
        {
            _spriteLookup = new Dictionary<ComboKey, Sprite>();
            
            if (KeySprite == null) return;
            
            foreach (var mapping in KeySprite)
            {
                if (!_spriteLookup.ContainsKey(mapping.key))
                {
                    _spriteLookup.Add(mapping.key, mapping.sprite);
                }
            }
        }

        private void ClearComboPanel()
        {
            if (ComboPanel == null) return;
            
            foreach (Transform child in ComboPanel)
            {
                Destroy(child.gameObject);
            }
        }

        private void CreateKeyIcons(List<ComboKey> keys)
        {
            if (keys == null || keyIconPrefab == null) return;
            
            foreach (var key in keys)
            {
                var icon = Instantiate(keyIconPrefab, ComboPanel);
                SetupKeyIcon(icon, key);
            }
        }

        private void SetupKeyIcon(GameObject icon, ComboKey key)
        {
            var img = icon.GetComponent<Image>();
            var text = icon.GetComponentInChildren<TMP_Text>();

            if (img != null)
            {
                img.sprite = idealButtonSprite;
            }

            if (text != null)
            {
                text.text = Gamepad.current != null ? ConvertToGamepadKey(key) : key.ToString();
            }
        }

        private string ConvertToGamepadKey(ComboKey key)
        {
            return key switch
            {
                ComboKey.W => "Y",
                ComboKey.A => "X", 
                ComboKey.S => "A",
                ComboKey.D => "B",
                _ => key.ToString()
            };
        }

        private void PrepareNextButton(int comboIndex)
        {
            var nextIndex = comboIndex + 1;
            if (nextIndex < ComboPanel.childCount)
            {
                SetKeySprite(nextIndex, prepareButtonSprite);
            }
        }

        private void SetKeySprite(int index, Sprite sprite)
        {
            if (index >= 0 && index < ComboPanel.childCount)
            {
                var icon = ComboPanel.GetChild(index).GetComponent<Image>();
                if (icon != null)
                {
                    icon.sprite = sprite;
                }
            }
        }

        private void SetKeyColor(int index, Color color)
        {
            if (index >= 0 && index < ComboPanel.childCount)
            {
                var icon = ComboPanel.GetChild(index).GetComponent<Image>();
                if (icon != null)
                {
                    icon.color = color;
                }
            }
        }

        private void SetAllKeysColor(Color color)
        {
            for (int i = 0; i < ComboPanel.childCount; i++)
            {
                SetKeyColor(i, color);
            }
        }

        private void SetUIElementActive(GameObject element, bool active)
        {
            if (element != null)
            {
                element.SetActive(active);
            }
        }
        #endregion
    }
}