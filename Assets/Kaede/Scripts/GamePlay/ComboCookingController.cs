using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.Inputs.ComboHandlers;
using Kaede.Scripts.Item;
using Kaede.Scripts.Managers;
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
        
        [Title("Debug")]
        [SerializeField, ReadOnly] private List<MenuData> completedMenus = new List<MenuData>();
        
        private bool _isStepComplete = false;
        private bool _checking;
        
        private InventoryController _inventoryController;
        
        private ComboCookingModel _model;
        private ComboCookingView  _view;
        private ComboKeySetting _currentComboSetting;
        private PlayerInputHandler _inputHandler;
        private IDisposable _confirmSub;
        private IComboHandler _currentHandler;
        private CancellationTokenSource _inputCts;

        #region Awake, Start, Update
        
        protected override void Awake()
        {
            _inputHandler = FindObjectOfType<PlayerInputHandler>();
            base.Awake();
        }

        private void Start()
        {
            _model = new ComboCookingModel(MenuDatasList, maxTimePerCombo);
            _view  = GetComponent<ComboCookingView>();
            _inventoryController = GetComponent<InventoryController>();
            
            ShowCurrentCombo();
        }

        private void Update()
        { 
            Tick(Time.deltaTime);

            if (_model.CurrentTimer <= 0f)
            {
                _model.ResetCombo();
                _view.ResetCombo();
                return;
            }

            GetTimeLeft();
            CheckComboButton();
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
        
        private void CheckComboButton()
        {
            if (_checking) return;
            _checking   = true;
            _inputCts ??= new CancellationTokenSource();

            try
            {
                if (!_model.TryGetCurrentSequenceCount(out var count) || count == 0) return;
                if (_model.CurrentComboIndex >= count) return;

                if (!_model.TryGetExpectedCombo(out var expectedCombo)) return;

                if (_currentHandler == null || _currentComboSetting != expectedCombo)
                {
                    _currentHandler      = ComboHandlerFactory.Create(expectedCombo);
                    _currentComboSetting = expectedCombo;
                }

                var result = _currentHandler.CheckInput(_inputHandler, expectedCombo.key, _inputCts.Token);


                if (_isStepComplete) return;
                
                switch (result)
                {
                    case ComboInputResult.Correct:
                        _view.PressCorrectKey(_model.CurrentComboIndex);
                        NextCombo();
                        break;

                    case ComboInputResult.Wrong:
                        if (!_isStepComplete)
                            _view.PressWrongKey(_model.CurrentComboIndex);
                        break;

                    case ComboInputResult.None:
                    default:
                        break;
                }
            }
            finally
            {
                _checking = false;
            }
        }

        private void NextCombo()
        {
            if (!_model.TryGetCurrentSequenceCount(out var count) || count == 0) return;
            
            if (_model.CurrentComboIndex + 1 >= count)
            {
                _isStepComplete       = true;
                _currentHandler       = null;
                _currentComboSetting  = null;
                CancelInputLoop();
                return;
            }

            _currentHandler      = null;
            _currentComboSetting = null;
            _model.NextCombo();
        }
        
        private void NextStep()
        {
            if (!_isStepComplete) return;

            CancelInputLoop();
            _isStepComplete      = false;
            _currentHandler      = null;
            _currentComboSetting = null;
            
            if (!_model.HasNextStep())
            {
                NextMenu();
                return;
            }
            
            _model.NextStep();
            _model.ResetCombo();
            ShowCurrentCombo();
            Debug.Log("Next Step");
        }

        
        private void NextMenu()
        {
            CancelInputLoop();
            _isStepComplete      = false;
            
            _currentHandler      = null;
            _currentComboSetting = null;
            
            if (!_model.HasNextMenu())
            {
                _model.CompleteMenu();
                _view.CompleteCombo();
                return;
            }

            _model.NextMenu();
            _model.ResetStep();
            _model.ResetCombo();
            ShowCurrentCombo();
            
            _inventoryController.CompleteMenu();
            Debug.Log("Next Menu");
        }
        
        private void ShowCurrentCombo()
        {
            if (_model.TryGetCurrentKeys(out var keys))
                _view.ShowCombo(keys);
        }
        #endregion

        #region Timer
        private void GetTimeLeft()
        {
            _view.TimerText.text = _model.CurrentTimer.ToString("N0");
        }

        private void Tick(float deltaTime)
        {
            _model.CurrentTimer -= deltaTime;
        }
        #endregion
        
        private void CancelInputLoop()
        {
            _inputCts?.Cancel();
            _inputCts?.Dispose();
            _inputCts = new CancellationTokenSource();
        }
    }
}
