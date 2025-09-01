using System.Collections.Generic;
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

        private void CheckComboButton()
        {
            if (_model.MenuDatas == null || _model.CurrentMenuIndex >= _model.MenuDatas.Count) return;

            var currentMenu = _model.MenuDatas[_model.CurrentMenuIndex];
            if (_model.CurrentComboIndex >= currentMenu.ComboKeys.Count) return;

            var expectedKey = currentMenu.ComboKeys[_model.CurrentComboIndex];
            if (IsExpectedInputPressed(expectedKey))
            {
                _model.NextCombo();
                OnKeyPress(expectedKey);

                if (_model.CurrentComboIndex >= currentMenu.ComboKeys.Count)
                {
                    NextMenu();
                    OnKeyPress(expectedKey);

                    if (_model.CurrentMenuIndex >= _model.MenuDatas.Count)
                    {
                        _model.CompleteMenu();
                        _view.CompleteCombo();
                    }
                }
            }
        }
        
        private bool IsExpectedInputPressed(ComboKey expectedKey)
        {
            if (_inputHandler == null) return false;

            switch (expectedKey)
            {
                case ComboKey.W:
                    return _inputHandler.ComboUpButton?.Value.isDown == true;

                case ComboKey.S:
                    return _inputHandler.ComboDownButton?.Value.isDown == true;

                case ComboKey.A:
                    return _inputHandler.ComboLeftButton?.Value.isDown == true;

                case ComboKey.D:
                    return _inputHandler.ComboRightButton?.Value.isDown == true;

                default:
                    return false;
            }
        }

        private void OnKeyPress(ComboKey key)
        {
            var currentIcon = _view.ComboPanel.GetChild(_view.CurrentMenuIndex).GetComponent<Image>();
            var expectedKey = ComboKey.None;
            foreach (var mapping in _view.KeySprite)
            {
                if (mapping.sprite == currentIcon.sprite)
                {
                    expectedKey = mapping.key;
                    break;
                }
            }
            
            if (key == expectedKey)
            {
                _view.PressCorrectKey();
            }
            else
            {
                _view.PressWrongKey();
            }
        }
        
        private void NextMenu()
        {
            _model.NextMenu();
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
