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
        
        [Title("References")]
        [field: SerializeField] public Transform ComboPanel { get; private set; }
        [SerializeField] private GameObject keyIconPrefab;
        [SerializeField] private Sprite defaultSprite;
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

                img.sprite = _spriteLookup.GetValueOrDefault(key, defaultSprite);
            }
        }
        
        public void NoneKeyPressed(int comboIndex)
        {
            var currentIcon = ComboPanel.GetChild(comboIndex).GetComponent<Image>();
            currentIcon.color = Color.white;
        }
        
        public void PressCorrectKey(int comboIndex)
        {
            var currentIcon = ComboPanel.GetChild(comboIndex).GetComponent<Image>();
            currentIcon.color = Color.green;
        }

        public void PressWrongKey(int comboIndex)
        {
            var currentIcon = ComboPanel.GetChild(comboIndex).GetComponent<Image>();
            currentIcon.color = Color.red;
        }
        
        public void CompleteCombo()
        {
            foreach (Transform child in ComboPanel)
            {
                var img = child.GetComponent<Image>();
                img.color = Color.yellow;
            }
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
                var img = child.GetComponent<Image>();
                img.color = Color.white;
            }
        }
    }
}
