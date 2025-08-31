using System.Collections.Generic;
using PrimeTween;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede.Scripts.GamePlay
{
    [System.Serializable]
    public class KeySpriteMapping
    {
        public Key key;
        public Sprite sprite;
    }
    
    public class ComboCookingView : MonoSingleton<ComboCookingView>
    {
        [Title("References")]
        [SerializeField] private Transform comboPanel;
        [SerializeField] private GameObject keyIconPrefab;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private List<KeySpriteMapping> keySprite;
        
        private int _currentMenuIndex = 0;
        private Dictionary<Key, Sprite> _spriteLookup;

        protected override void Awake()
        {
            _spriteLookup = new Dictionary<Key, Sprite>();
            foreach (var mapping in keySprite)
            {
                if (!_spriteLookup.ContainsKey(mapping.key))
                    _spriteLookup.Add(mapping.key, mapping.sprite);
            }
        }

        
        /// <summary>
        /// สร้าง UI แสดงปุ่มที่ต้องกดตาม combo
        /// </summary>
        public void ShowCombo(List<Key> keys)
        {
            foreach (Transform child in comboPanel)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var key in keys)
            {
                var icon = Instantiate(keyIconPrefab, comboPanel);
                var img = icon.GetComponent<Image>();

                img.sprite = _spriteLookup.GetValueOrDefault(key, defaultSprite);
            }
        }
        
        public void OnKeyPress(Key key)
        {
            if (_currentMenuIndex >= comboPanel.childCount) return;

            var currentIcon = comboPanel.GetChild(_currentMenuIndex).GetComponent<Image>();
            var expectedKey = Key.None;
            foreach (var mapping in keySprite)
            {
                if (mapping.sprite == currentIcon.sprite)
                {
                    expectedKey = mapping.key;
                    break;
                }
            }

            if (key == expectedKey)
            {
                currentIcon.color = Color.green;
                _currentMenuIndex++;
            }
            else
            {
                currentIcon.color = Color.red;
                _currentMenuIndex = 0;

                foreach (Transform child in comboPanel)
                {
                    var img = child.GetComponent<Image>();
                    img.color = Color.white;
                }
            }
            
            if (_currentMenuIndex >= comboPanel.childCount)
            {
                Debug.Log("Combo Complete!");
                // รีเซ็ตสำหรับรอบถัดไป
                foreach (Transform child in comboPanel)
                {
                    var img = child.GetComponent<Image>();
                    img.color = Color.yellow;
                }
                
            }
        }

        public void ClearCombo()
        {
            foreach (Transform child in comboPanel)
            {
                Destroy(child.gameObject);
            }
            _currentMenuIndex = 0;
        }
        
        public void ResetCombo()
        {
            _currentMenuIndex = 0;
            foreach (Transform child in comboPanel)
            {
                var img = child.GetComponent<Image>();
                img.color = Color.white;
            }
        }
    }
}
