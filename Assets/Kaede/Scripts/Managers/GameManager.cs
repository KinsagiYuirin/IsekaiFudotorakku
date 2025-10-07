using MadDuck.Scripts.Inputs;
using R3;
using System;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace Kaede.Scripts.Managers
{
    public class GameManager : MonoSingleton<GameManager>
    {
        [Header("Pause Settings")]
        [SerializeField] private GameObject pauseMenuUI;
        [SerializeField] private bool isPaused = false;
        
        private PlayerInputHandler _inputHandler;
        private IDisposable _pauseButtonSubscription;
        
        public bool IsPaused => isPaused;
        
        public static event Action<bool> OnPauseStateChanged;
        
        protected override void Awake()
        {
            base.Awake();
            _inputHandler = FindObjectOfType<PlayerInputHandler>();
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
        
        public void TogglePause()
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
            
            OnPauseStateChanged?.Invoke(true);
            Debug.Log("Game Paused");
        }
        
        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            
            if (pauseMenuUI != null)
                pauseMenuUI.SetActive(false);
            
            AudioListener.pause = false;
            
            OnPauseStateChanged?.Invoke(false);
            
            Debug.Log("Game Resumed");
        }
    }
}
