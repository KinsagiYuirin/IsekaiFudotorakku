using System;
using System.Collections.Generic;
using Kaede.Scripts.Item;
using PrimeTween;
using Sirenix.OdinInspector;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede.Scripts.GamePlay
{
    public enum CookingAction
    {
        
    }
    
    [Serializable]
    public class KeySpriteMapping
    {
        public ComboKey key;
        public Sprite sprite;
    }
    
    public class ComboCookingView : MonoSingleton<ComboCookingView>
    {
        [Title("Settings")]
        [field: SerializeField] public TMP_Text TimerText { get; private set; }
        [SerializeField] private GameObject keyIconPrefab;
        
        [Title("References")]
        [field: SerializeField] public Transform ComboPanel { get; private set; }
        [field: SerializeField] public Image CookingImage { get; private set; }
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite currentButtonSprite;
        
        [Title("Sprite Container")]
        [SerializeField] private Sprite panSprite;
        
        [field: SerializeField] public List<KeySpriteMapping> KeySprite {get; private set;}
        [field: SerializeField, DisplayAsString] public int CurrentMenuIndex { get; private set; }
        private Dictionary<ComboKey, Sprite> _spriteLookup;

        protected override void Awake()
        {
            _spriteLookup = new Dictionary<ComboKey, Sprite>();
            foreach (var mapping in KeySprite)
            {
                if (!_spriteLookup.ContainsKey(mapping.key))
                    _spriteLookup.Add(mapping.key, mapping.sprite);
            }
            base.Awake();
        }
        
        public void ShowCombo(List<ComboKey> keys)
        {
            foreach (Transform child in ComboPanel)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var key in keys)
            {
                var icon = Instantiate(keyIconPrefab, ComboPanel);
                var img = icon.GetComponent<Image>();
                var text = icon.GetComponentInChildren<TMP_Text>();

                img.sprite = _spriteLookup.GetValueOrDefault(key, defaultSprite);
                if (Gamepad.current != null)
                {
                    ConvertToJoyStick(text, key);
                }
                else
                {
                    text.text = key.ToString();
                }
            }
        }
        
        public void CurrentKeyPressed(int comboIndex)
        {
            var currentIcon   = ComboPanel.GetChild(comboIndex).GetComponent<Image>();
            currentIcon.sprite = currentButtonSprite;
        }
        
        public void NoneKeyPressed(int comboIndex)
        {
            var currentIcon   = ComboPanel.GetChild(comboIndex).GetComponent<Image>();
            currentIcon.color = Color.white;
        }
        
        public void PressCorrectKey(int comboIndex)
        {
            var currentIcon   = ComboPanel.GetChild(comboIndex).GetComponent<Image>();
            currentIcon.color = Color.green;
        }

        public void PressWrongKey(int comboIndex)
        {
            var currentIcon   = ComboPanel.GetChild(comboIndex).GetComponent<Image>();
            currentIcon.color = Color.red;
        }
        
        public void CompleteCombo()
        {
            foreach (Transform child in ComboPanel)
            {
                var img   = child.GetComponent<Image>();
                img.color = Color.yellow;
            }
        }

        public void SetCookingImage(Sprite sprite)
        {
            if (sprite != null)
                CookingImage.sprite = sprite;
        }
        
        public void ClearCombo()
        {
            foreach (Transform child in ComboPanel)
            {
                Destroy(child.gameObject);
            }
            CurrentMenuIndex = 0;
        }
        
        public void ResetCombo()
        {
            CurrentMenuIndex = 0;
            foreach (Transform child in ComboPanel)
            {
                var img   = child.GetComponent<Image>();
                img.color = Color.white;
            }
        }

        private void ConvertToJoyStick(TMP_Text text,ComboKey key)
        {
            text.text = key switch
            {
                ComboKey.W => "Y",
                ComboKey.A => "X",
                ComboKey.S => "A",
                ComboKey.D => "B",
                _ => text.text
            };
        }
    }
}
