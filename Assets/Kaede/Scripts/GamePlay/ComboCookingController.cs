using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Kaede.Scripts.Inputs.ComboHandlers;
using Kaede.Scripts.Item;
using Kaede.Scripts.Managers;
using MadDuck.Scripts.Inputs;
using R3;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;

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
        [Title("Game Settings")]
        [SerializeField] private ComboTimerService timer = new ComboTimerService();
        
        [Title("Combo Settings")]
        [field: SerializeField] public List<MenuData> MenuDatasList { get; private set; }
        [field: SerializeField] public float TimeBetweenCombos { get; private set; } = 1f;
        
        [Title("Score Setting")] 
        [SerializeField] private float scorePerButton;
        
        [Title("Debug")]
        [SerializeField, ReadOnly] private List<MenuData> completedMenus = new List<MenuData>();
        
        private InventoryController _inventoryController;
        private RandomSystem _randomSystem;
        
        private ComboCookingView  _view;
        private ComboCookingModel _model;
        private IDisposable _cancelSubscription;
        private IDisposable _confirmSubscription;
        private ComboInputProcessor _inputProcessor;
        private ComboMenuManager    _menuManager;
        private PlayerInputHandler  _inputHandler;
        private VFX_Control _VFX;
        
        #region Awake, Start, Update
        
        protected override void Awake()
        {
            _inputHandler = FindObjectOfType<PlayerInputHandler>();
            _randomSystem = FindObjectOfType<RandomSystem>();
            _menuManager = new ComboMenuManager(() =>
            {
                _randomSystem ??= FindObjectOfType<RandomSystem>();
                return _randomSystem;
            });

            if (_menuManager.Initialize(true))
            {
                MenuDatasList = new List<MenuData>(_menuManager.CurrentMenus);
            }
            base.Awake();
        }

        private void Start()
        {
            if (_menuManager.Initialize(false))
            {
                MenuDatasList = new List<MenuData>(_menuManager.CurrentMenus);
            }
            _model = new ComboCookingModel(MenuDatasList, timer.MaxTimePerCombo);
            _view  = GetComponent<ComboCookingView>();
            _inventoryController = GetComponent<InventoryController>();
            _model.ScoreManager.SetPendingStepScore(0);
            _VFX = GetComponent<VFX_Control>();
            
            timer.Initialize(_view);
            timer.TimedOut     += HandleComboTimeout;
            timer.RestEntered  += HandleRestEntered;
            timer.RestFinished += HandleRestFinished;
            
            ShowCurrentCombo();
        }

        private void Update()
        { 
            if (_model == null) return;
            timer.Tick(Time.deltaTime);
            if (_model.GameState == CookingState.Resting) return;

            _inputProcessor?.Process(_model);
            
        }
        #endregion

        #region OnEnable, OnDisable
        private void OnEnable()
        {
            _confirmSubscription = _inputHandler.ConfirmButton.Subscribe(button =>
            {
                if (button.isDown)
                {
                    NextStep();
                }
            });
            
            _cancelSubscription = _inputHandler.CancelButton.Subscribe(button =>
            {
                if (button.isDown)
                {
                    RedoStep();
                }
            });
        }

        private void OnDisable()
        {
            _confirmSubscription?.Dispose();
            _cancelSubscription?.Dispose();
            _inputProcessor?.ResetState();
            
        }

        private void OnDestroy()
        {
            timer.TimedOut     -= HandleComboTimeout;
            timer.RestEntered  -= HandleRestEntered;
            timer.RestFinished -= HandleRestFinished;
            _inputProcessor?.Dispose();
        }

        #endregion

        #region Menu Setup
        private bool TryAdvanceMenuType()
        {
            if (_menuManager == null)
            {
                return false;
            }

            if (!_menuManager.MoveToNextMenuType())
            {
                return false;
            }

            var scoreManager = _model != null ? _model.ScoreManager : new ScoreManager();
            _model = new ComboCookingModel(MenuDatasList, timer.MaxTimePerCombo, scoreManager);
            _model.ScoreManager.SetPendingStepScore(0);
            _model.ResetCombo();
            _model.ResetStep();
            timer.ResetTimer();
            _view.ResetCombo();
            _inputProcessor?.ResetState();
            ShowCurrentCombo();

            _inventoryController?.ReloadMenus();
            return true;
        }
        #endregion

        #region Combo Features
        private void NextStep()
        {
            if (_model is { GameState: CookingState.Resting }) return;
            if (_inputProcessor == null || !_inputProcessor.IsStepComplete) return;
            _inputProcessor?.ResetState();
            
            _model.ScoreManager.CommitPendingStepScore();
            var hasNext = _model.NextStep();
            if (!hasNext)
            {
                NextMenu();
                return;
            }

            timer.AddTimeNextCombo();
            _model.ResetCombo();
            ShowCurrentCombo();
            Debug.Log($"Step Score: {_model.ScoreManager.CurrentStepScores[^1]}");
            Debug.Log("Next Step");
        }
        
        private void NextMenu()
        {
            _inputProcessor?.ResetState();
            
            if (!_model.HasNextMenu())
            {
                _model.CompleteMenu();
                _view.CompleteCombo();
                Debug.Log($"Grand Total Score: {_model.ScoreManager.GrandTotalScore}");

                if (!TryAdvanceMenuType()) return;
                _model.Resting(timer.RestingTime);
                timer.BeginRestingPhase();
                Debug.Log("Next Menu Type");
                return;
            }

            _model.NextMenu();
            _model.ResetStep();
            _model.ResetCombo();
            timer.ResetTimer();
            ShowCurrentCombo();
            
            var latestMenuScore = _model.ScoreManager.MenuScores[^1];
            var allScore        =  _model.ScoreManager.GrandTotalScore;
            Debug.Log($"Menu {_model.ScoreManager.MenuScores.Count} Score: {latestMenuScore} \n" +
                      $"All Score: {allScore}");
                        
            _inventoryController.CompleteMenu();
            Debug.Log("Next Menu");
        }

        private void RedoStep()
        {
            if (_model is { GameState: CookingState.Resting }) return;
            
            _inputProcessor?.ResetStateWithCombo(_model);
            
            _model.ResetCombo();
            _model.ScoreManager.AddRedoCount();
            Debug.Log("Redo Step");
        }
        
        private void ShowCurrentCombo()
        {
            if (_model.TryGetCurrentComboSettings(out var combos))
            {
                _view.ShowCombo(combos);

                var menu = _model.MenuDatas[_model.CurrentMenuIndex];
                if (menu?.steps != null && _model.CurrentMenuIndex < menu.steps.Count)
                {
                    var step = menu.steps[_model.CurrentStepIndex];
                    var sprite = step?.preset != null ? step.preset.cookingSprite : null;
                    _view.SetCookingImage(sprite);
                }
            }
            
        }
        #endregion

        #region Utill
        private void HandleComboTimeout()
        {
            _model.ResetCombo();
            _view.ResetCombo();
            _model.ScoreManager.ResetPendingStepScore();
            _inputProcessor?.ResetState();
        }

        private void HandleRestEntered()
        {
            _inputProcessor?.ResetState();
        }
        
        private void HandleRestFinished()
        {
            _inventoryController?.SetVisible(true);
            _model.ResetCombo();
            _view.ResetCombo();
            ShowCurrentCombo();
        }
        #endregion
    }
}
