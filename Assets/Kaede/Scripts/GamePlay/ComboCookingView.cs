using System.Collections.Generic;
using Kaede.Scripts.Animation;
using Kaede.Scripts.Item;
using Sirenix.OdinInspector;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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
        private sealed class DualComboButtonVisualProxy : IComboButtonVisual
        {
            private readonly IComboButtonVisual _primary;
            private readonly IComboButtonVisual _secondary;

            public DualComboButtonVisualProxy(IComboButtonVisual primary, IComboButtonVisual secondary)
            {
                _primary = primary;
                _secondary = secondary;
            }

            public void Initialize(ComboKeySetting comboSetting, string displayKey, bool isStringKey)
            {
                _primary?.Initialize(comboSetting, displayKey, isStringKey);
            }

            public void SetState(KeyState state, int? index, float? indexFloat)
            {
                _primary?.SetState(state, index, indexFloat);
                _secondary?.SetState(state, index, indexFloat);
            }

            public void SetColor(Color color)
            {
                _primary?.SetColor(color);
                _secondary?.SetColor(color);
            }
        }
        
        [Title("Color Settings")]
        [SerializeField] private Color correctKeyColor = Color.green;
        [SerializeField] private Color wrongKeyColor = Color.red;
        [SerializeField] private Color completeComboColor = Color.yellow;
        
        [Title("UI References")]
        [field: SerializeField] public TMP_Text TimerText { get; private set; }
        [field: SerializeField] public Transform ComboPanel { get; private set; }
        [field: SerializeField] public Transform SubComboPanel { get; private set; }
        [field: SerializeField] public Sprite CookingImage { get; private set; }
        [field: SerializeField] public GameObject CookingImageObject { get; private set; }
        [SerializeField] private GameObject cookingLightEffect;
        [field: SerializeField] public TMP_Text ComboText { get; private set; }
        [SerializeField] private GameObject stove;
         
        [Title("Prefabs")]
        [SerializeField] private GameObject normalIconPrefab;
        [SerializeField] private GameObject holdIconPrefab;
        [SerializeField] private GameObject stackIconPrefab;
        [SerializeField] private GameObject dualHoldKeyIconPrefab;
        
        [Title("Dummy")]
        [SerializeField] private GameObject dummyIconPrefab;
        [SerializeField] private GameObject dummyHoldIconPrefab;
        [SerializeField] private GameObject dummyDualHoldIconPrefab;

        
        [Title("Display Settings")]
        [SerializeField] private bool useStringDisplayKey = false;

        private readonly List<ComboKeySetting> _comboSettings = new();
        private readonly List<IComboButtonVisual> _buttonVisuals = new();
        private readonly Dictionary<int, IComboButtonVisual> _subButtonVisuals = new();
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
            SetKeySprite(comboIndex, KeyState.Active);
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
            if (ComboPanel != null)
            {
                foreach (Transform child in ComboPanel)
                {
                    Destroy(child.gameObject);
                }
            }

            if (SubComboPanel != null)
            {
                foreach (Transform child in SubComboPanel)
                {
                    Destroy(child.gameObject);
                }
            }
            _comboSettings.Clear();
            _buttonVisuals.Clear();
            _subButtonVisuals.Clear();
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
                            CreateSubDualButton(comboSetting, _buttonVisuals.Count - 1, dummyIconPrefab);
                        }
                        break;
                    case {type: ComboType.Hold}:
                        if (holdIconPrefab != null)
                        {
                            var holdIcon = Instantiate(holdIconPrefab, ComboPanel);
                            _comboSettings.Add(comboSetting);
                            SetupKeyIcon(holdIcon, comboSetting);
                            CreateSubDualButton(comboSetting, _buttonVisuals.Count - 1, dummyHoldIconPrefab);
                        }
                        break;
                    case {type: ComboType.DualKeyHold}:
                        if (holdIconPrefab != null)
                        {
                            var dualHoldIcon = Instantiate(dualHoldKeyIconPrefab, ComboPanel);
                            _comboSettings.Add(comboSetting);
                            SetupKeyIcon(dualHoldIcon, comboSetting);
                            CreateSubDualButton(comboSetting, _buttonVisuals.Count - 1, dummyDualHoldIconPrefab);
                        }
                        break;
                    case {type: ComboType.DualKey}:
                        if (normalIconPrefab != null)
                        {
                            var dualKeyIcon = Instantiate(normalIconPrefab, ComboPanel);
                            _comboSettings.Add(comboSetting);
                            SetupKeyIcon(dualKeyIcon, comboSetting);
                            CreateSubDualButton(comboSetting, _buttonVisuals.Count - 1, normalIconPrefab);
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
        
        private void SetupKeyIcon(GameObject icon, ComboKeySetting comboSetting, string overrideDisplayKey = null, bool registerVisual = true)
        {
            var visual = icon.GetComponent<IComboButtonVisual>() ?? icon.AddComponent<DefaultComboButtonVisual>();

            var key = comboSetting?.key ?? ComboKey.None;
            var displayKey = overrideDisplayKey ?? (comboSetting?.type is ComboType.DualKeyHold or ComboType.DualKey
                ? GetDualDisplayKey(comboSetting)
                : GetDisplayKey(key));

            visual.Initialize(comboSetting, displayKey, useStringDisplayKey);
            visual.SetState(KeyState.Ideal, null, null);
            visual.SetColor(Color.white);

            if (registerVisual)
            { _buttonVisuals.Add(visual); }
        }

        private void CreateSubDualButton(ComboKeySetting comboSetting, int mainButtonIndex, GameObject iconPrefab)
        {
            if (SubComboPanel == null) return;

            if (iconPrefab  == null) return;
            
            var isDummyIcon = iconPrefab == dummyIconPrefab || iconPrefab == dummyHoldIconPrefab;
            if (isDummyIcon && comboSetting.secondKey == ComboKey.None)
            {
                var dummyIcon = Instantiate(iconPrefab, SubComboPanel);
                HideDummyVisuals(dummyIcon);
                return;
            }

            if (comboSetting.secondKey == ComboKey.None) return;
            
            var subSetting = new ComboKeySetting
            {
                key = isDummyIcon && comboSetting.secondKey == ComboKey.None
                    ? ComboKey.None
                    : comboSetting.secondKey,
                type = comboSetting.type,
                dualHoldTime = comboSetting.dualHoldTime
            };

            var subIcon = Instantiate(iconPrefab, SubComboPanel);
            var subDisplayKey = GetDisplayKey(subSetting.key);
            SetupKeyIcon(subIcon, subSetting, subDisplayKey, false);

            var visual = subIcon.GetComponent<IComboButtonVisual>();
            if (visual != null)
            {
                _subButtonVisuals[mainButtonIndex] = visual;
                if (mainButtonIndex < _buttonVisuals.Count && _buttonVisuals[mainButtonIndex] != null)
                {
                    _buttonVisuals[mainButtonIndex] =
                        new DualComboButtonVisualProxy(_buttonVisuals[mainButtonIndex], visual);
                }
            }
        }
        
        private static void HideDummyVisuals(GameObject dummyIcon)
        {
            foreach (var graphic in dummyIcon.GetComponentsInChildren<Graphic>(true))
            {
                graphic.enabled = false;
            }
        }
        
        private string GetDualDisplayKey(ComboKeySetting comboSetting)
        {
            if (comboSetting == null)
            {
                return string.Empty;
            }

            var firstKey = GetDisplayKey(comboSetting.key);
            var secondKey = GetDisplayKey(comboSetting.secondKey);

            if (string.IsNullOrEmpty(firstKey) && string.IsNullOrEmpty(secondKey))
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(firstKey))
            {
                return secondKey;
            }

            if (string.IsNullOrEmpty(secondKey))
            {
                return firstKey;
            }

            return $"{firstKey}+{secondKey}";
        }
        
        private string GetDisplayKey(ComboKey key)
        {
            if (key == ComboKey.None)
            {
                return string.Empty;
            }

            if (Gamepad.current != null)
                return ConvertToGamepadKey(key);
            else
                return key.ToString();
        }
        
        private string ConvertToGamepadKey(ComboKey key)
        {
            return key switch
            {
                ComboKey.W => "Y",
                ComboKey.A => "X", 
                ComboKey.S => "A",
                ComboKey.D => "B",
                ComboKey.Up => "↑",
                ComboKey.Down => "↓",
                ComboKey.Left => "←",
                ComboKey.Right => "→",
                _ => key.ToString()
            };
        }

        private void PrepareNextButton(int comboIndex)
        {
            var nextIndex = comboIndex + 1;
            Debug.Log($"PrepareNextButton - Current: {comboIndex}, Next: {nextIndex}, Total buttons: {ComboPanel.childCount}");
            if (nextIndex < _buttonVisuals.Count)
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
        
        // ชั่วคราว
        private void SetStove(bool active)
        {
            if (stove != null)
            {
                stove.gameObject.SetActive(active);
            }
        }
        #endregion
    }
}