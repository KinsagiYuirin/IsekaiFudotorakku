using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.Inputs.ComboHandlers;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;
using R3;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.GamePlay
{
    public enum ComboInputResult
    {
        None,
        Correct,
        Wrong
    }
    
    public class ComboCookingController : MonoSingleton<ComboCookingController>
    {
        [Title("Settings")]
        [field: SerializeField] public List<MenuData> MenuDatasList { get; private set; }
        [SerializeField] private float maxTimePerCombo = 5f;
        [field: SerializeField] public float TimeBetweenCombos { get; private set; } = 1f;
        
        private bool _isStepComplete = false;
        
        private ComboCookingModel _model;
        private ComboCookingView _view;
        private PlayerInputHandler _inputHandler;
        private IDisposable _confirmSub;
        private bool _checking;
        private CancellationTokenSource _inputCts;

        #region Awake, Start, Update
        
        [Obsolete("Obsolete")]
        protected override void Awake()
        {
            _inputHandler = FindObjectOfType<PlayerInputHandler>();
        }

        private void Start()
        {
            _model = new ComboCookingModel(MenuDatasList, maxTimePerCombo);
            _view = GetComponent<ComboCookingView>();
            
            ShowCurrentCombo();
        }

        private void Update()
        {
            _model.Tick(Time.deltaTime);

            if (_model.CurrentTimer <= 0f)
            {
                _model.ResetCombo();
                _view.ResetCombo();
                return;
            }

            GetTimeLeft();
            _ = CheckComboButton();
        }
        #endregion

        #region OnEnable, OnDisable
        private void OnEnable()
        {
            _confirmSub = _inputHandler.ConfirmButton.Subscribe(button =>
            {
                if (button.isDown)
                {
                    NextStep();
                }
            });
        }
        
        private void OnDisable()
        {
            _confirmSub?.Dispose();
        }
        #endregion

        #region Combo Logic
        
        private async UniTask CheckComboButton()
        {
            if (_checking) return;
            _checking = true;
            _inputCts ??= new CancellationTokenSource();

            try
            {
                var currentMenu = _model.MenuDatas[_model.CurrentMenuIndex];
                if (currentMenu.Steps == null || _model.CurrentStepIndex >= currentMenu.Steps.Count) return;

                var stepRef = currentMenu.Steps[_model.CurrentStepIndex];
                var sequence = stepRef.ResolveSequence();
                if (sequence == null || sequence.Count == 0) return;

                if (_model.CurrentComboIndex >= sequence.Count) return;

                var expectedCombo = sequence[_model.CurrentComboIndex];
                var handler = ComboHandlerFactory.Create(expectedCombo);
                
                var result = await handler.CheckInput(_inputHandler, expectedCombo.key, _inputCts.Token);

                switch (result)
                {
                    case ComboInputResult.Correct:
                        _view.PressCorrectKey(_model.CurrentComboIndex);
                        NextCombo(sequence, currentMenu);
                        break;

                    case ComboInputResult.Wrong:
                        _view.PressWrongKey(_model.CurrentComboIndex);
                        break;

                    case ComboInputResult.None:
                    default:
                        
                        break;
                }
            }
            catch (OperationCanceledException) { }
            finally { _checking = false; }
        }

        private void NextCombo(List<ComboKeySetting> sequence, MenuData currentMenu)
        {
            if (_model.CurrentComboIndex + 1 >= sequence.Count)
            {
                _model.ResetCombo();
                if (_model.CurrentStepIndex + 1 < currentMenu.Steps.Count)
                {
                    _isStepComplete = true;
                }
            }
            else
                _model.NextCombo();
        }
        
        private void NextStep()
        {
            var menu = _model.MenuDatas[_model.CurrentMenuIndex];
            if (menu?.Steps == null || _model.CurrentStepIndex >= menu.Steps.Count)
            {
                NextMenu();
                return;
            }
            
            if (!_isStepComplete) return;
            CancelInputLoop();
            _isStepComplete = false;

            _model.NextStep();
            _model.ResetCombo();
            
            ShowCurrentCombo();
        }
        
        private void NextMenu()
        {
            CancelInputLoop();
            Debug.Log("Next Menu"); 
            _isStepComplete = false;
            
            _model.NextMenu();
            _model.ResetStep();
            _model.ResetCombo();
            
            if (_model.CurrentMenuIndex < _model.MenuDatas.Count) return;
            _model.CompleteMenu();
            _view.CompleteCombo();

            ShowCurrentCombo();
        }
        
        private void ShowCurrentCombo()
        {
            if (_model.MenuDatas == null || _model.CurrentMenuIndex >= _model.MenuDatas.Count) return;

            var currentMenu = _model.MenuDatas[_model.CurrentMenuIndex];
            if (currentMenu.Steps == null || _model.CurrentStepIndex >= currentMenu.Steps.Count) return;

            var stepRef = currentMenu.Steps[_model.CurrentStepIndex];
            var sequence = stepRef.ResolveSequence();
            if (sequence == null || sequence.Count == 0)
            {
                // แล้วแต่ดีไซน์: จะเคลียร์ UI หรือปล่อยว่าง
                // _view.ClearCombo();
                return;
            }
            
            var keys = sequence.ConvertAll(c => c.key);
            _view.ShowCombo(keys);
        }

        #endregion

        private void GetTimeLeft()
        {
            _view.TimerText.text = _model.CurrentTimer.ToString("N0");
        }
        
        private void CancelInputLoop()
        {
            _inputCts?.Cancel();
            _inputCts?.Dispose();
            _inputCts = new CancellationTokenSource();
        }
    }
}
