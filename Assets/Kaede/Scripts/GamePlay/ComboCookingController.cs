using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede.Scripts.GamePlay
{
    public class ComboCookingController : MonoSingleton<ComboCookingController>
    {
        [Title("Settings")]
        [SerializeField] private List<MenuData> menuDatasList;
        [SerializeField] private float maxTimePerCombo = 5f;

        private ComboCookingModel _model;
        private ComboCookingView _view;

        void Start()
        {
            _model = new ComboCookingModel(menuDatasList, maxTimePerCombo);
            _view = GetComponent<ComboCookingView>();

            ShowCurrentCombo();
        }

        void Update()
        {
            _model.Tick(Time.deltaTime);

            if (_model.CurrentTimer <= 0f)
            {
                _model.ResetCombo();
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
            
            if (Keyboard.current[expectedKey] != null && Keyboard.current[expectedKey].wasPressedThisFrame)
            {
                _model.NextCombo();
                _view.OnKeyPress(expectedKey);

                if (_model.CurrentComboIndex >= currentMenu.ComboKeys.Count)
                {
                    NextMenu();
                    _view.OnKeyPress(expectedKey);

                    if (_model.CurrentMenuIndex >= _model.MenuDatas.Count)
                    {
                        _model.CompleteMenu();
                        _view.OnKeyPress(expectedKey);
                    }
                }
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
