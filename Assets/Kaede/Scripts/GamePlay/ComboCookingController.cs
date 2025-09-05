using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using MadDuck.Scripts.Inputs;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Kaede.Scripts.GamePlay
{
    public class ComboCookingController : MonoSingleton<ComboCookingController>
    {
        [Title("Settings")]
        [SerializeField] private List<MenuData> menuDatasList;
        [SerializeField] private float maxTimePerCombo = 5f;

        private ComboCookingModel _model;
        private ComboCookingView _view;
        private PlayerInputHandler _inputHandler;

        void Start()
        {
            _model = new ComboCookingModel(menuDatasList, maxTimePerCombo);
            _view = GetComponent<ComboCookingView>();
            _inputHandler = FindObjectOfType<PlayerInputHandler>();

            ShowCurrentCombo();
        }

        void Update()
        {
            _model.Tick(Time.deltaTime);

            if (_model.CurrentTimer <= 0f)
            {
                _model.ResetCombo();
                _view.ResetCombo();
                return;
            }

            CheckComboButton();
        }

        private async UniTask CheckComboButton()
        {
            if (_model.MenuDatas == null || _model.CurrentMenuIndex >= _model.MenuDatas.Count) return;

            var currentMenu = _model.MenuDatas[_model.CurrentMenuIndex];
            if (_model.CurrentComboIndex >= currentMenu.ComboKeys.Count) return;

            var expectedKey = currentMenu.ComboKeys[_model.CurrentComboIndex];
            
            var pressedKey = GetPressedComboKey();
            if (pressedKey == ComboKey.None) return; 
            
            if (pressedKey == expectedKey)
            {
                OnKeyPress(pressedKey);

                if (_model.CurrentComboIndex + 1 >= currentMenu.ComboKeys.Count)
                {
                    await UniTask.Delay(200);
                    NextMenu();

                    if (_model.CurrentMenuIndex >= _model.MenuDatas.Count)
                    {
                        _model.CompleteMenu();
                        _view.CompleteCombo();
                    }
                }
                else
                {
                    _model.NextCombo();
                }
            }
            else
            {
                OnKeyPress(pressedKey);
            }
        }
        
        private ComboKey GetPressedComboKey()
        {
            return true switch
            {
                true when _inputHandler.ComboUpButton?.Value.isDown == true => ComboKey.W,
                true when _inputHandler.ComboDownButton?.Value.isDown == true => ComboKey.S,
                true when _inputHandler.ComboLeftButton?.Value.isDown == true => ComboKey.A,
                true when _inputHandler.ComboRightButton?.Value.isDown == true => ComboKey.D,
                _ => ComboKey.None
            };
        }

        /// <summary>
        /// When a key is pressed, check if it matches the expected key in the combo sequence.
        /// </summary>
        /// <param name="key"></param>
        private void OnKeyPress(ComboKey key)
        {
            int comboIndex = _model.CurrentComboIndex;
            if (comboIndex < 0 || comboIndex >= _view.ComboPanel.childCount) return;

            var currentIcon = _view.ComboPanel.GetChild(comboIndex).GetComponent<Image>();
            var expectedKey = ComboKey.None;
            foreach (var mapping in _view.KeySprite)
            {
                if (mapping.sprite == currentIcon.sprite)
                {
                    expectedKey = mapping.key;
                    break;
                }
            }

            if (expectedKey != key)
            {
                Debug.Log("Wrong Key Pressed");
                _view.PressWrongKey(comboIndex);
            }
            else
            {
                _view.PressCorrectKey(comboIndex);
            }
        }

        private void NextMenu()
        {
            
            _model.NextMenu();
            _model.ResetCombo();
            ShowCurrentCombo();
        }
        
        private void ShowCurrentCombo()
        {
            if (_model.MenuDatas == null || _model.CurrentMenuIndex >= _model.MenuDatas.Count) return;

            var currentMenu = _model.MenuDatas[_model.CurrentMenuIndex];
            var keys = currentMenu.ComboKeys;
            _view.ShowCombo(keys);
        }
    }
}
