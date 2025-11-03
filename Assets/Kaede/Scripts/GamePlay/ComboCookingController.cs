using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kaede.Art.TA_Works.Scripts;
using Kaede.Scripts.Animation;
using Kaede.Scripts.Audios;
using Kaede.Scripts.Item;
using Kaede.Scripts.Managers;
using Kaede.Scripts.Utils;
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
        Progress,
        Correct,
        Wrong,
        Holding
    }
    
    public class ComboCookingController : MonoSingleton<ComboCookingController>
    {
        [Title("Game Settings")]
        [SerializeField] private ComboTimerService timer = new ComboTimerService();
        [SerializeField] private bool needSpacebar;
        
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
        private VFXControl _vfxControl;
        
        [SerializeField] private ComboStepAnimationPlayer _animationPlayer;
        [SerializeField] private ComboCharacterEmotionPlayer _characterEmotionPlayer;
        [SerializeField] private SfxManagerDemo _sfxManager;
        [SerializeField] private SendFood sendFood;
        
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
            Initialized();
        }

        /// <summary>
        /// If it not working, check GameManager Update method
        /// </summary>
        private void Update()
        { 
            if (_model == null) return;
            var gameManager = GameManager.Instance;
            if (gameManager != null)
            {
                gameManager.UpdateReadyCountdown();
                if (gameManager.ReadyTime > 0) return;
            }
            timer.Tick(Time.deltaTime);
            if (_model.GameState == CookingState.Resting) return;

            _inputProcessor?.Process(_model);
        }

        private void Initialized()
        {
            if (_menuManager.Initialize(false))
            {
                MenuDatasList = new List<MenuData>(_menuManager.CurrentMenus);
            }
            _model = new ComboCookingModel(MenuDatasList, timer.MaxTimePerCombo);
            _view  = GetComponent<ComboCookingView>();
            
            _animationPlayer ??= GetComponent<ComboStepAnimationPlayer>();
            _animationPlayer ??= GetComponentInChildren<ComboStepAnimationPlayer>();
            
            _characterEmotionPlayer ??= GetComponent<ComboCharacterEmotionPlayer>();
            _characterEmotionPlayer ??= GetComponentInChildren<ComboCharacterEmotionPlayer>();
            
            _inventoryController = GetComponent<InventoryController>();
            _vfxControl =  GetComponent<VFXControl>();
            
            _model.ScoreManager.SetPendingStepScore(0);
            if (_inputHandler == null)
            {
                _inputHandler = FindObjectOfType<PlayerInputHandler>();
            }

            if (_view == null || _inputHandler == null)
            {
                Debug.LogError("ComboCookingController is missing dependencies required to initialize input processing.");
            }
            else
            {
                _inputProcessor = new ComboInputProcessor(_inputHandler, _view, scorePerButton, 
                    _animationPlayer, _characterEmotionPlayer, _sfxManager);
            }
            
            timer.Initialize(_view);
            timer.TimedOut     += HandleComboTimeout;
            timer.RestEntered  += HandleRestEntered;
            timer.RestFinished += HandleRestFinished;
            
            ShowCurrentCombo();
        }
        #endregion

        #region OnEnable, OnDisable
        private void OnEnable()
        {
            _confirmSubscription = _inputHandler.ConfirmButton.Subscribe(button =>
            {
                if (!needSpacebar) return;
                if (button.isDown)
                {
                    NextStep().Forget();
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
            
            MenuDatasList = new List<MenuData>(_menuManager.CurrentMenus);
            
            var scoreManager = _model != null ? _model.ScoreManager : new ScoreManager();
            _model = new ComboCookingModel(MenuDatasList, timer.MaxTimePerCombo, scoreManager);
            _model.ScoreManager.SetPendingStepScore(0);
            _model.ResetCombo();
            _model.ResetStep();
            timer.ResetTimer();
            _view.ResetCombo();
            _inputProcessor?.ResetState();
            ShowCurrentCombo();

            if (_inventoryController != null)
            {
                _inventoryController.SetInventoryData(MenuDatasList);
            }
            return true;
        }
        #endregion

        #region Combo Features
        private async UniTask NextStep()
        {
            if (_model is { GameState: CookingState.Resting }) return;
            if (_inputProcessor == null || !_inputProcessor.IsStepComplete) return;
            _inputProcessor?.ResetState();
            
            _model.ScoreManager.CommitPendingStepScore();
            var hasNext = _model.NextStep();
            if (!hasNext)
            {
                sendFood.SetToStartPosition(_model.MenuDatas[_model.CurrentMenuIndex].menuSprite);
                await timer.PauseTimerForSeconds(timer.DelayAfterFinishMenu);
                NextMenu().Forget();;
                return;
            }

            timer.AddTimeNextCombo();
            _model.ResetCombo();
            ShowCurrentCombo();
            Debug.Log($"Step Score: {_model.ScoreManager.CurrentStepScores[^1]}");
            Debug.Log("Next Step");
        }
        
        private async UniTask NextMenu()
        {
            _inputProcessor?.ResetState();

            var multiplier = MathF.Round(timer.DividerTimeToMultiply(), 1);
            
            if (!_model.HasNextMenu())
            {
                _inventoryController.ReloadMenus();
                _model.CompleteMenu(multiplier);
                _view.CompleteCombo();
                Debug.Log($"Grand Total Score: {_model.ScoreManager.GrandTotalScore}");
                Debug.Log($"multiplier: {multiplier}");

                if (!TryAdvanceMenuType())
                {
                    await timer.PauseTimerForSeconds(timer.DelayBeforeGameOver);
                    GameManager.Instance.GameOver(_model.ScoreManager.GrandTotalScore);
                    return;
                }
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
                _characterEmotionPlayer?.ResetToIdle();

                var menu = _model.MenuDatas[_model.CurrentMenuIndex];
                if (menu?.steps != null && _model.CurrentStepIndex < menu.steps.Count)
                {
                    var step = menu.steps[_model.CurrentStepIndex];
                    var sprite = step?.preset != null ? step.preset.cookingSprite : null;
                    _view.SetCookingImage(sprite);
                    var animationDefinition = step?.ResolveAnimation() ?? ComboStepAnimationDefinition.None;
                    if (animationDefinition.HasAnimation)
                    {
                        _animationPlayer?.SetAnimation(animationDefinition, false);
                    }
                    else
                    {
                        _animationPlayer?.ClearAnimation();
                    }
                }
                else
                {
                    _animationPlayer?.ClearAnimation();
                }   
            }
        }
        #endregion

        #region Utill
        private void HandleComboTimeout()
        {
            _model.ResetCombo();
            _view.ResetCombo();
            NextMenu();
            _model.ScoreManager.ResetPendingStepScore();
            _inputProcessor?.ResetState();
            _animationPlayer?.Stop();
            _characterEmotionPlayer?.ResetToIdle();
        }

        private void HandleRestEntered()
        {
            _inventoryController?.SetVisible(false);
            _model.ResetCombo();
            _view.ResetCombo();
            _animationPlayer?.Stop();
            _inputProcessor?.ResetState();
            _characterEmotionPlayer?.ResetToIdle();
        }
        
        private void HandleRestFinished()
        {
            _inventoryController?.SetVisible(true);
            _model.ResetCombo();
            _view.ResetCombo();
            _animationPlayer?.Stop();
            ShowCurrentCombo();
            _characterEmotionPlayer?.ResetToIdle();
        }
        #endregion
    }
}
