using MadDuck.Scripts.Inputs;
using R3;
using System;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.UI;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Kaede.Scripts.Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [Header("Pause Settings")]
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
            
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromInput();
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
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
        
        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(true);
            
            AudioListener.pause = true;
        }
        
        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
            
            AudioListener.pause = false;
            
            Debug.Log("Game Resumed");
        }
        
        public void GameOver(float index)
        {
            isPaused = true;
            Time.timeScale = 0f;
            
            if (resultUI != null)
                resultUI.gameObject.SetActive(true);
            
            AudioListener.pause = true;
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
