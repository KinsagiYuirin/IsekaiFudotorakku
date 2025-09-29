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
        [SerializeField] private ComboTimerService _timer = new ComboTimerService();
        
        [Title("Combo Settings")]
        [field: SerializeField] public List<MenuData> MenuDatasList { get; private set; }
        [field: SerializeField] public float TimeBetweenCombos { get; private set; } = 1f;
        
        [Title("Score Setting")] 
        [SerializeField] private float scorePerButton;
        
        [Title("Debug")]
        [SerializeField, ReadOnly] private List<MenuData> completedMenus = new List<MenuData>();
        
        private bool _isStepComplete = false;
        private bool _checking;
        
        private InventoryController _inventoryController;
        private RandomSystem _randomSystem;
        
        private ComboCookingModel _model;
        private ComboCookingView  _view;
        private ComboKeySetting _currentComboSetting;
        private PlayerInputHandler _inputHandler;
        private IDisposable _confirmSubscription;
        private IDisposable _cancelSubscription;
        private IComboHandler _currentHandler;
        private CancellationTokenSource _inputCts;
        
        #region Awake, Start, Update
        
        protected override void Awake()
        {
            _inputHandler = FindObjectOfType<PlayerInputHandler>();
            _randomSystem = FindObjectOfType<RandomSystem>();
            ApplyRandomMenus(true);
            
            base.Awake();
        }

        private void Start()
        {
            ApplyRandomMenus(false);
            
            _model = new ComboCookingModel(MenuDatasList, _timer.MaxTimePerCombo);
            _view  = GetComponent<ComboCookingView>();
            _inventoryController = GetComponent<InventoryController>();
            _model.ScoreManager.SetPendingStepScore(0);
            
            _timer.Initialize(_view);
            _timer.TimedOut     += HandleComboTimeout;
            _timer.RestEntered  += HandleRestEntered;
            _timer.RestFinished += HandleRestFinished;
            
            ShowCurrentCombo();
        }

        private void Update()
        { 
            if (_model == null) return;
            _timer.Tick(Time.deltaTime);
            if (_model.GameState == CookingState.Resting) return;

            CheckComboButton();
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
            CancelInputLoop();
        }

        private void OnDestroy()
        {
            _timer.TimedOut     -= HandleComboTimeout;
            _timer.RestEntered  -= HandleRestEntered;
            _timer.RestFinished -= HandleRestFinished;
        }

        #endregion

        #region Menu Setup
        private void SetMenuDatas(IEnumerable<MenuData> menus)
        {
            MenuDatasList = menus?.Where(menu => menu != null).ToList() ?? new List<MenuData>();
        }

        private bool ApplyRandomMenus(bool forceRegenerate, bool advanceToNextType = false)
        {
            if (_randomSystem == null)
            {
                _randomSystem = FindObjectOfType<RandomSystem>();
            }

            if (_randomSystem == null) return false;

            List<MenuData> menus;

            if (advanceToNextType)
            {
                menus = _randomSystem.MoveToNextMenuSetForCombo();
            }
            else
            {
                menus = _randomSystem.GetMenuSetForCombo(forceRegenerate);
            }

            if (menus == null || menus.Count == 0) return false;

            SetMenuDatas(menus);
            return true;
        }
        
        private bool TryAdvanceMenuType()
        {
            if (!ApplyRandomMenus(false, true))
            {
                return false;
            }

            var scoreManager = _model != null ? _model.ScoreManager : new ScoreManager();
            _model = new ComboCookingModel(MenuDatasList, _timer.MaxTimePerCombo, scoreManager);
            _model.ScoreManager.SetPendingStepScore(0);
            _model.ResetCombo();
            _model.ResetStep();
            _timer.ResetTimer();
            _view.ResetCombo();
            ShowCurrentCombo();

            _inventoryController?.ReloadMenus();

            _isStepComplete = false;
            _currentHandler = null;
            _currentComboSetting = null;
            CancelInputLoop();

            return true;
        }
        #endregion
        
        #region Combo Logic
        private void CheckComboButton()
        {
            if (_model != null && _model.GameState == CookingState.Resting) return;
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
                
                _view.CurrentKeyPressed(_model.CurrentComboIndex);
                
                switch (result)
                {
                    case ComboInputResult.Correct:
                        _view.PressCorrectKey(_model.CurrentComboIndex);
                        _model.ScoreManager.AddPendingStepScore(scorePerButton);
                        NextCombo();
                        break;

                    case ComboInputResult.Wrong:
                        if (!_isStepComplete)
                            _view.PressWrongKey(_model.CurrentComboIndex);
                        NextCombo();
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
        #endregion

        #region Combo Features
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
            if (_model != null && _model.GameState == CookingState.Resting) return;
            if (!_isStepComplete) return;

            CancelInputLoop();
            _isStepComplete      = false;
            _currentHandler      = null;
            _currentComboSetting = null;
            
            _model.ScoreManager.CommitPendingStepScore();
            var hasNext = _model.NextStep();
            if (!hasNext)
            {
                NextMenu();
                return;
            }

            _timer.AddTimeNextCombo();
            _model.ResetCombo();
            ShowCurrentCombo();
            Debug.Log($"Step Score: {_model.ScoreManager.CurrentStepScores[^1]}");
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
                Debug.Log($"Grand Total Score: {_model.ScoreManager.GrandTotalScore}");
                
                if (TryAdvanceMenuType())
                {
                    _model.Resting(_timer.RestingTime);
                    _timer.BeginRestingPhase();
                    Debug.Log("Next Menu Type");
                    return;
                }
                return;
            }

            _model.NextMenu();
            _model.ResetStep();
            _model.ResetCombo();
            _timer.ResetTimer();
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
            if (_model != null && _model.GameState == CookingState.Resting) return;
            
            CancelInputLoop();
            _isStepComplete      = false;
            _currentHandler      = null;
            _currentComboSetting = null;
            
            _model.ResetCombo();
            ShowCurrentCombo();
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
        }

        private void HandleRestEntered()
        {
            CancelInputLoop();
            _checking = false;
            _currentHandler      = null;
            _currentComboSetting = null;
        }
        
        private void HandleRestFinished()
        {
            _inventoryController?.SetVisible(true);
            _model.ResetCombo();
            _view.ResetCombo();
            CancelInputLoop();
            _currentHandler      = null;
            _currentComboSetting = null;
            ShowCurrentCombo();
        }
        
        private void CancelInputLoop()
        {
            _inputCts?.Cancel();
            _inputCts?.Dispose();
            _inputCts = new CancellationTokenSource();
        }
        #endregion
    }
}
