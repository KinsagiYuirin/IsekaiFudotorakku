using System;
using System.Collections.Generic;
using Kaede.Scripts.Animation;
using Kaede.Scripts.Item;
using Kaede.Scripts.Utils;
using MadDuck.Scripts.Inputs;
using Sirenix.OdinInspector;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede.Scripts.GamePlay
{
    public enum KeyState
    {
        Active,
        Current,
        Prepare,
        Ideal
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
        [SerializeField] private GameObject cookingLightEffect;
        [field: SerializeField] public TMP_Text ComboText { get; private set; }
         
        [Title("Prefabs")]
        [SerializeField] private GameObject normalIconPrefab;
        [SerializeField] private GameObject holdIconPrefab;
        [SerializeField] private GameObject stackIconPrefab;
        
        [Title("Display Settings")]
        [SerializeField] private bool useStringDisplayKey = false;

        private readonly List<ComboKeySetting> _comboSettings = new();
        private readonly List<IComboButtonVisual> _buttonVisuals = new();
        public List<IComboButtonVisual> ButtonVisuals => _buttonVisuals ;
        
        #region Unity Lifecycle
        protected override void Awake()
        {
            base.Awake();
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
                ComboText.text = $"{current} combo";
            }
        }
        
        public void ShowCombo(List<ComboKeySetting> comboSettings)
        {
            ClearComboPanel();
            CreateKeyIcons(comboSettings);
        }
        
        public void CurrentKeyPressed(int comboIndex)
        {
            Debug.Log($"CurrentKeyPressed called with index: {comboIndex}");
            SetKeySprite(comboIndex, KeyState.Current);
            PrepareNextButton(comboIndex);
        }

        public void NoneKeyPressed(int comboIndex)
        {
            SetKeySprite(comboIndex, KeyState.Ideal);
        }
        
        public void PressCorrectKey(int comboIndex)
        {
            SetKeySprite(comboIndex, KeyState.Active);
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
        }
        
        public void ResetCombo()
        {
            SetAllKeysColor(Color.white);
        }
        
        public void SetRestingMode(bool isResting)
        {
            SetUIElementActive(ComboPanel?.gameObject, !isResting);
            SetUIElementActive(CookingImageObject?.gameObject, !isResting);
            SetUIElementActive(cookingLightEffect?.gameObject, !isResting);
        }

        public void HideShowTimer(float alpha)
        {
            TimerText.alpha = alpha;
        }
        #endregion

        #region Private Methods

        private void ClearComboPanel()
        {
            if (ComboPanel == null) return;
            
            foreach (Transform child in ComboPanel)
            {
                Destroy(child.gameObject);
            }
            _comboSettings.Clear();
            _buttonVisuals.Clear();
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
                            _comboSettings.Add(comboSetting);
                            SetupKeyIcon(singleIcon, comboSetting);
                        }
                        break;
                    case {type: ComboType.Hold}:
                        if (holdIconPrefab != null)
                        {
                            var holdIcon = Instantiate(holdIconPrefab, ComboPanel);
                            _comboSettings.Add(comboSetting);
                            SetupKeyIcon(holdIcon, comboSetting);
                        }
                        break;
                    case {type:ComboType.Stack or ComboType.StackTimer}:
                        if (stackIconPrefab != null)
                        {
                            var stackIcon = Instantiate(stackIconPrefab, ComboPanel);
                            _comboSettings.Add(comboSetting);
                            SetupKeyIcon(stackIcon, comboSetting);
                        }
                        break;
                }
            }
        }
        
        private void SetupKeyIcon(GameObject icon, ComboKeySetting comboSetting)
        {
            var visual = icon.GetComponent<IComboButtonVisual>() ?? icon.AddComponent<DefaultComboButtonVisual>();
            
            var key = comboSetting?.key ?? ComboKey.None;
            var displayKey = GetDisplayKey(key);

            visual.Initialize(comboSetting, displayKey, useStringDisplayKey);
            visual.SetState(KeyState.Ideal, null, null);
            visual.SetColor(Color.white);

            _buttonVisuals.Add(visual);
        }
        
        private string GetDisplayKey(ComboKey key)
        {
            if (key == ComboKey.None)
            {
                return string.Empty;
            }

            return Gamepad.current != null ? ConvertToGamepadKey(key) : key.ToString();
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
            Debug.Log($"PrepareNextButton - Current: {comboIndex}, Next: {nextIndex}, Total buttons: {ComboPanel.childCount}");
            if (nextIndex < ComboPanel.childCount)
            {
                SetKeySprite(nextIndex, KeyState.Prepare);
            }
        }

        private void SetKeySprite(int index, KeyState state)
        {
            Debug.Log($"SetKeySprite called - Index: {index}, State: {state}");
            if (index < 0 || index >= _buttonVisuals.Count)
            {
                Debug.LogWarning($"Invalid index {index}, ButtonVisuals count: {_buttonVisuals.Count}");
                return;
            }
            _buttonVisuals[index]?.SetState(state, null, null);
        }
        
        private void SetKeyColor(int index, Color color)
        {
            if (index < 0 || index >= _buttonVisuals.Count) return;

            _buttonVisuals[index]?.SetColor(color);
        }

        private void SetAllKeysColor(Color color)
        {
            foreach (var visual in _buttonVisuals)
            {
                visual?.SetColor(color);
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