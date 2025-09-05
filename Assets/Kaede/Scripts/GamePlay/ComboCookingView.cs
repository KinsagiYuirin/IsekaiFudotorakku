using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede.Scripts.GamePlay
{
    public enum ComboKey
    {
        None,
        W,
        A,
        S,
        D
    }
    
    [System.Serializable]
    public class KeySpriteMapping
    {
        public ComboKey key;
        public Sprite sprite;
    }
    
    public class ComboCookingView : MonoSingleton<ComboCookingView>
    {
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
        
        public void PressCorrectKey(int comboIndex)
        {
            if (comboIndex < 0 || comboIndex >= ComboPanel.childCount) return;
            var currentIcon = ComboPanel.GetChild(comboIndex).GetComponent<Image>();
            currentIcon.color = Color.green;
        }

        public void PressWrongKey(int comboIndex)
        {
            if (comboIndex < 0 || comboIndex >= ComboPanel.childCount) return;
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
