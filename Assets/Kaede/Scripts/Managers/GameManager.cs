using MadDuck.Scripts.Inputs;
using R3;
using System;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.UI;
using Kaede.Scripts.UI.TodayMenu;
using Kaede.Scripts.Utils;
using Sirenix.OdinInspector;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede.Scripts.Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] public bool test;
        
        [Title("Game Settings")]
        [SerializeField] private float readyTime = 3f;
        public float ReadyTime => _isReadyCountdownActive ? _readyTimer : 0f;
        [SerializeField] private TMP_Text readyText;
        [SerializeField] private ControllerCheck controllerCheck;
        
        [Title("Today Menu")]
        [SerializeField] private TodayManuUI todayManuUI;
        
        [Title("Pause Settings")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private PauseUI pauseMenuUI;
        [SerializeField] private bool isPaused = false;
        
        [Title("ResultUI")]
        [SerializeField] private ResultUI resultUI;

        private bool _interactable = true;
        private PlayerInputHandler _inputHandler;
        private IDisposable _pauseButtonSubscription;
        private float _readyTimer;
        private bool  _isReadyCountdownActive;
        private InputMode _currentInputMode = InputMode.KeyboardMouse;
        
        public bool IsPaused
        {
            get => isPaused;
            set => isPaused = value;
        }

        protected override void Awake()
        {
            base.Awake();
            _inputHandler = FindObjectOfType<PlayerInputHandler>();
            SetupButtons();
            
            _readyTimer = 0f;
            _isReadyCountdownActive = false;

            SetPauseMenuVisibility(false);
            SetResultUIVisibility(false);
        }
        
        private void Start()
        {
            SubscribeToInput();
            InitializeReadyText();
            SetPauseMenuVisibility(false);
            SetInteractable(true);
            PauseGame();
        }
        
        public void Update()
        {
            if (!_isReadyCountdownActive) return;
            CountdownReady();
        }
        
        private void OnEnable()
        {
            if (controllerCheck != null)
            {
                controllerCheck.InputModeChanged += OnInputModeChanged;
            }
        }

        private void OnDisable()
        {
            _pauseButtonSubscription?.Dispose();
            if (controllerCheck != null)
            {
                controllerCheck.InputModeChanged -= OnInputModeChanged;
            }
        }

        private void OnDestroy()
        {
            CleanupButtons();
        }
        
        private void SetupButtons()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(TogglePause);
            }
        }

        private void CleanupButtons()
        {
            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveAllListeners();
            }
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (pauseButton != null)
            {
                pauseButton.interactable = interactable;
            }
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            SetButtonsInteractable(_interactable);
        }
        
        private void CountdownReady()
        {
            _readyTimer -= Time.unscaledDeltaTime;

            if (readyText != null)
            {
                readyText.gameObject.SetActive(true);
                readyText.text = Mathf.CeilToInt(Mathf.Max(_readyTimer, 0f)).ToString();
            }

            if (_readyTimer > 0f) return;

            CompleteReadyCountdown();
        }

        public void StartReadyCountdown()
        {
            if (_isReadyCountdownActive) return;

            _readyTimer = readyTime;

            if (_readyTimer <= 0f)
            {
                if (readyText != null)
                {
                    readyText.gameObject.SetActive(false);
                }

                ResumeGame();
                return;
            }

            PauseGame();
            _isReadyCountdownActive = true;

            if (readyText != null)
            {
                readyText.text = Mathf.CeilToInt(_readyTimer).ToString();
                readyText.gameObject.SetActive(true);
            }
        }

        private void CompleteReadyCountdown()
        {
            _isReadyCountdownActive = false;

            if (readyText != null)
            {
                readyText.gameObject.SetActive(false);
            }

            ResumeGame();
        }
        
        private void SubscribeToInput()
        {
            if (_inputHandler == null) return;
            
            _pauseButtonSubscription = _inputHandler.PauseGameButton.Subscribe(button =>
            {
                if (button.isDown)
                {
                    if (!_interactable) return;
                    TogglePause();
                }
            });
        }

        private void TogglePause()
        {
            if (ReadyTime > 0f) return;
            if (isPaused)
            {
                ResumeGame();
                SetPauseMenuVisibility(false);
                SetInputMode(controllerCheck.CurrentInputMode, null);
            }
            else
            {
                PauseGame();
                SetPauseMenuVisibility(true);
                SetInputMode(controllerCheck.CurrentInputMode, pauseMenuUI.ResumeButton.gameObject);
            }
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            _isReadyCountdownActive = false;
            _readyTimer = 0f;

            if (readyText != null)
            {
                readyText.gameObject.SetActive(false);
            }

            isPaused = false;
            Time.timeScale = 1f;
        }
        
        public void GameOver(float index)
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            SetResultUIVisibility(true);
            resultUI?.SetResultScore(index);
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        
        private void InitializeReadyText()
        {
            if (readyText == null)
                return;

            readyText.gameObject.SetActive(false);
        }

        private void SetPauseMenuVisibility(bool isVisible)
        {
            if (pauseMenuUI == null)
                return;

            pauseMenuUI.gameObject.SetActive(isVisible);
        }

        private void SetResultUIVisibility(bool isVisible)
        {
            if (resultUI == null)
                return;

            resultUI.gameObject.SetActive(isVisible);
            SetInputMode(controllerCheck.CurrentInputMode, resultUI.RestartButton.gameObject);
        }

        #region Controller Check

        private void SetInputMode(InputMode newMode, GameObject button = null)
        {
            var hasEventSystem = EventSystem.current != null;
            
            if (EventSystem.current == null) return;

            if (_currentInputMode != newMode)
            {
                _currentInputMode = newMode;

                if (!hasEventSystem) return;

                if (_currentInputMode == InputMode.Gamepad)
                {
                    if (EventSystem.current.currentSelectedGameObject == null && button != null)
                    {
                        controllerCheck.DelaySelect(button).Forget();
                    }
                    // Cursor.visible = false;
                }
                else
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    // Cursor.visible = true;
                }

                return;
            }
            if (!hasEventSystem) return;

            if (_currentInputMode == InputMode.Gamepad && EventSystem.current.currentSelectedGameObject == null && button != null)
            {
                controllerCheck.DelaySelect(button).Forget();
            }
        }

        private void OnInputModeChanged(InputMode newMode)
        {
            SetInputMode(newMode);
        }
        
        #endregion

    }
}
