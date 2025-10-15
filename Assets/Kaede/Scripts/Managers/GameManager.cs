using MadDuck.Scripts.Inputs;
using R3;
using System;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.UI;
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
        [SerializeField] private TMP_Text readyText;
        
        [Title("Tutorial")]
        [SerializeField] private TutorialDemo tutorialDemo;
        public bool tutorialCompleted = false;
        
        [Title("Pause Settings")]
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private bool isPaused = false;
        
        [Title("ResultUI")]
        [SerializeField] private ResultUI resultUI;
        
        private PlayerInputHandler _inputHandler;
        private IDisposable _pauseButtonSubscription;
        
        public bool IsPaused
        {
            get => isPaused;
            set => isPaused = value;
        }

        protected override void Awake()
        {
            base.Awake();
            _inputHandler = FindObjectOfType<PlayerInputHandler>();

            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
            if (resultUI != null)
                resultUI.gameObject.SetActive(false);
        }
        
        private void Start()
        {
            SubscribeToInput();
            if (tutorialDemo && !test)
            {
                PauseGame();
                tutorialCompleted = false;

                if (tutorialDemo != null)
                    tutorialDemo.gameObject.SetActive(true);
            }
            
            readyText.gameObject.SetActive(false);
            
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
        }

        private void Update()
        {
            CountdownReady();
        }

        private void OnDestroy()
        {
            UnsubscribeFromInput();
        }
        
        private void CountdownReady()
        {
            if (!tutorialCompleted) return;
            readyText.gameObject.SetActive(true);
            readyTime -= Time.unscaledDeltaTime;
            readyText.text = readyTime.ToString("N0");
            
            if (!(readyTime <= 0f)) return;
            readyText.gameObject.SetActive(false);
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
            if (!tutorialCompleted) return;
            if (isPaused)
            {
                ResumeGame();
                if (pauseMenuUI != null)
                    pauseMenuUI.SetActive(false);
            }
            else
            {
                PauseGame();
                if (pauseMenuUI != null)
                    pauseMenuUI.SetActive(true);
            }
        }
        
        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
        }
        
        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
        }
        
        public void GameOver(float index)
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            if (resultUI != null)
                resultUI.gameObject.SetActive(true);
            
            resultUI.SetResultScore(index);
        }
        
        public void QuitGame()
        {
            Application.Quit();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
