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
        [Title("Combo Settings")]
        [field: SerializeField] public List<MenuData> MenuDatasList { get; private set; }
        [SerializeField] private float maxTimePerCombo = 5f;
        [field: SerializeField] public float TimeBetweenCombos { get; private set; } = 1f;

        [Title("Score Setting")] 
        [SerializeField] private float scorePerButton;
        
        [Title("Debug")]
        [SerializeField, ReadOnly] private List<MenuData> completedMenus = new List<MenuData>();
        
        private bool _isStepComplete = false;
        private bool _checking;
        
        private InventoryController _inventoryController;
        private GameManager _gameManager;
        
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
            _gameManager = FindObjectOfType<GameManager>();
            ApplyRandomMenusFromGameManager(true);
            
            base.Awake();
        }

        private void Start()
        {
            ApplyRandomMenusFromGameManager(false);
            
            _model = new ComboCookingModel(MenuDatasList, maxTimePerCombo);
            _view  = GetComponent<ComboCookingView>();
            _inventoryController = GetComponent<InventoryController>();
            _model.ScoreManager.SetPendingStepScore(0);
            
            ShowCurrentCombo();
        }

        private void Update()
        { 
            Tick(Time.deltaTime);

            if (_model.CurrentTimer <= 0f)
            {
                _model.ResetCombo();
                _view.ResetCombo();
                _model.ScoreManager.ResetPendingStepScore();
                return;
            }

            GetTimeLeft();
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
                    RetryStep();
                }
            });
        }
        
        private void OnDisable()
        {
            _confirmSubscription?.Dispose();
            _cancelSubscription?.Dispose();
            CancelInputLoop();
        }
        #endregion

        #region Menu Setup
        public void SetMenuDatas(IEnumerable<MenuData> menus)
        {
            MenuDatasList = menus?.Where(menu => menu != null).ToList() ?? new List<MenuData>();
        }

        private void ApplyRandomMenusFromGameManager(bool forceRegenerate)
        {
            if (_gameManager == null)
            {
                _gameManager = FindObjectOfType<GameManager>();
            }

            if (_gameManager == null) return;

            var menus = _gameManager.GetMenuSetForCombo(forceRegenerate);
            if (menus == null || menus.Count == 0) return;

            SetMenuDatas(menus);
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
                        _model.ScoreManager.AddPendingStepScore(scorePerButton);
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
                return;
            }

            _model.NextMenu();
            _model.ResetStep();
            _model.ResetCombo();
            ShowCurrentCombo();
            
            var latestMenuScore = _model.ScoreManager.MenuScores[^1];
            var allScore =  _model.ScoreManager.GrandTotalScore;
            Debug.Log($"Menu {_model.ScoreManager.MenuScores.Count} Score: {latestMenuScore} \n" +
                      $"All Score: {allScore}");
                        
            _inventoryController.CompleteMenu();
            Debug.Log("Next Menu");
        }

        private void RetryStep()
        {
            CancelInputLoop();
            _isStepComplete      = false;
            _currentHandler      = null;
            _currentComboSetting = null;
            
            _model.ResetCombo();
            ShowCurrentCombo();
            Debug.Log("Undo Step");
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

        #region Until
        private void CancelInputLoop()
        {
            _inputCts?.Cancel();
            _inputCts?.Dispose();
            _inputCts = new CancellationTokenSource();
        }
        #endregion
    }
}
