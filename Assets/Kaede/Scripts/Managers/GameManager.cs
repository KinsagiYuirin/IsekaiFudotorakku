using MadDuck.Scripts.Inputs;
using R3;
using System;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.UI;
using Kaede.Scripts.UI.TodayMenu;
using Sirenix.OdinInspector;
using TMPro;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kaede.Scripts.Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] public bool test;
        
        [Title("Game Settings")]
        [SerializeField] private float readyTime = 3f;
        public float ReadyTime => _isReadyCountdownActive ? _readyTimer : 0f;
        [SerializeField] private TMP_Text readyText;
        
        //[Title("Tutorial")]
        //[SerializeField] private TutorialDemo tutorialDemo;
        //public bool tutorialCompleted = false;
        
        [Title("Today Menu")]
        [SerializeField] private TodayManuUI todayManuUI;
        
        [Title("Pause Settings")]
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private bool isPaused = false;
        
        [Title("ResultUI")]
        [SerializeField] private ResultUI resultUI;

        private PlayerInputHandler _inputHandler;
        private IDisposable _pauseButtonSubscription;
        private float _readyTimer;
        private bool  _isReadyCountdownActive;
        
        public bool IsPaused
        {
            get => isPaused;
            set => isPaused = value;
        }

        protected override void Awake()
        {
            base.Awake();
            _inputHandler = FindObjectOfType<PlayerInputHandler>();

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
            PauseGame();
        }

        /// <summary>
        /// Have changed a bit, called in Update of UIManager
        /// </summary>
        public void UpdateReadyCountdown()
        {
            if (!_isReadyCountdownActive) return;
            CountdownReady();
        }

        private void OnDestroy()
        {
            UnsubscribeFromInput();
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
                    TogglePause();
                }
            });
        }
        
        private void UnsubscribeFromInput()
        {
            _pauseButtonSubscription?.Dispose();
        }

        private void TogglePause()
        {
            if (ReadyTime > 0f) return;
            if (isPaused)
            {
                ResumeGame();
                SetPauseMenuVisibility(false);
            }
            else
            {
                PauseGame();
                SetPauseMenuVisibility(true);
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

            pauseMenuUI.SetActive(isVisible);
        }

        private void SetResultUIVisibility(bool isVisible)
        {
            if (resultUI == null)
                return;

            resultUI.gameObject.SetActive(isVisible);
        }
    }
}
