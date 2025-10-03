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
        [Title("Color Settings")]
        [SerializeField] private Color correctKeyColor = Color.green;
        [SerializeField] private Color wrongKeyColor = Color.red;
        [SerializeField] private Color completeComboColor = Color.yellow;
        
        [Title("UI References")]
        [field: SerializeField] public TMP_Text TimerText { get; private set; }
        [field: SerializeField] public Transform ComboPanel { get; private set; }
        [field: SerializeField] public Sprite CookingImage { get; private set; }
        [field: SerializeField] public GameObject CookingImageObject { get; private set; }
        [field: SerializeField] public TMP_Text ComboText { get; private set; }
        
        [Title("Prefabs")]
        [SerializeField] private GameObject normalIconPrefab;
        [SerializeField] private GameObject holdIconPrefab;
        [SerializeField] private GameObject stackIconPrefab;
        
        [Title("Button Sprites")]
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private ButtonSprite[] buttonSprites;
        
        [Title("Key Mappings")]
        [field: SerializeField] public List<KeySpriteMapping> KeySprite { get; private set; }
        
        [Title("Debug")]
        [field: SerializeField, DisplayAsString] public int CurrentMenuIndex { get; private set; }
        
        private Dictionary<ComboKey, Sprite> _spriteLookup;
        private Dictionary<(ComboType type, KeyState state), Sprite> _buttonSpriteLookup;
        private readonly List<ComboType> _comboTypes = new();
        
        #region Unity Lifecycle
        protected override void Awake()
        {
            InitializeSpriteLookup();
            InitializeButtonSpriteLookup();
            base.Awake();
        }

        private void Start()
        {
            
        }

        #endregion

        #region Public Methods
        public void UpdateComboText(int current)
        {
            if (ComboText == null) return;
            if (current == 0)
            {
                ComboText.alpha = 0;
            }
            else
            {
                ComboText.alpha = 1;
                ComboText.text = current.ToString();
            }
        }
        
        public void ShowCombo(List<ComboKeySetting> comboSettings)
        {
            ClearComboPanel();
            CreateKeyIcons(comboSettings);
        }
        
        public void CurrentKeyPressed(int comboIndex)
        {
            SetKeySprite(comboIndex, KeyState.Current);
            PrepareNextButton(comboIndex);
        }

        public void NoneKeyPressed(int comboIndex)
        {
            SetKeySprite(comboIndex, KeyState.Ideal);
        }
        
        public void PressCorrectKey(int comboIndex)
        {
            SetKeySprite(comboIndex, KeyState.Ideal);
            SetKeyColor(comboIndex, correctKeyColor);
        }

        public void PressWrongKey(int comboIndex)
        {
            SetKeySprite(comboIndex, KeyState.Ideal);
            SetKeyColor(comboIndex, wrongKeyColor);
        }
        
        public void CompleteCombo()
        {
            SetAllKeysColor(completeComboColor);
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
        
        private void InitializeButtonSpriteLookup()
        {
            _buttonSpriteLookup = new Dictionary<(ComboType, KeyState), Sprite>();

            if (buttonSprites == null) return;

            foreach (var buttonSprite in buttonSprites)
            {
                var key = (buttonSprite.comboType, buttonSprite.state);
                if (!_buttonSpriteLookup.ContainsKey(key) && buttonSprite.sprite != null)
                {
                    _buttonSpriteLookup.Add(key, buttonSprite.sprite);
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
            _comboTypes.Clear();
        }

        private void CreateKeyIcons(List<ComboKeySetting> comboSettings)
        {
            if (comboSettings == null) return;
            
            foreach (var comboSetting in comboSettings)
            {
                switch (comboSetting)
                {
                    case {type: ComboType.Single}:
                        if (normalIconPrefab != null)
                        {
                            var singleIcon = Instantiate(normalIconPrefab, ComboPanel);
                            _comboTypes.Add(comboSetting.type);
                            SetupKeyIcon(singleIcon, comboSetting);
                        }
                        break;
                    case {type: ComboType.Hold}:
                        if (holdIconPrefab != null)
                        {
                            var holdIcon = Instantiate(holdIconPrefab, ComboPanel);
                            _comboTypes.Add(comboSetting.type);
                            SetupKeyIcon(holdIcon, comboSetting);
                        }
                        break;
                    case {type:ComboType.Stack or ComboType.StackTimer}:
                        if (stackIconPrefab != null)
                        {
                            var stackIcon = Instantiate(stackIconPrefab, ComboPanel);
                            _comboTypes.Add(comboSetting.type);
                            SetupKeyIcon(stackIcon, comboSetting);
                        }
                        break;
                }
            }
        }

        private void SetupKeyIcon(GameObject icon, ComboKeySetting comboSetting)
        {
            var img = icon.GetComponent<Image>();
            var text = icon.GetComponentInChildren<TMP_Text>();

            if (img != null)
            {
                var comboType = comboSetting?.type ?? ComboType.Single;
                img.sprite = GetButtonSprite(comboType, KeyState.Ideal);
            }

            if (text != null)
            {
                var key = comboSetting?.key ?? ComboKey.None;
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
                SetKeySprite(nextIndex, KeyState.Prepare);
            }
        }

        private void SetKeySprite(int index, KeyState state)
        {
            if (index >= 0 && index < ComboPanel.childCount)
            {
                var icon = ComboPanel.GetChild(index).GetComponent<Image>();
                if (icon != null)
                {
                    var comboType = GetComboType(index);
                    icon.sprite = GetButtonSprite(comboType, state);
                }
            }
        }

        private ComboType GetComboType(int index)
        {
            if (index >= 0 && index < _comboTypes.Count)
            {
                return _comboTypes[index];
            }

            return ComboType.Single;
        }

        private Sprite GetButtonSprite(ComboType comboType, KeyState state)
        {
            if (_buttonSpriteLookup != null &&
                _buttonSpriteLookup.TryGetValue((comboType, state), out var sprite) &&
                sprite != null)
            {
                return sprite;
            }

            return defaultSprite;
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