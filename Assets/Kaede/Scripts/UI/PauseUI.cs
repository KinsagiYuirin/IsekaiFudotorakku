using Kaede.Scripts.Managers;
using MadDuck.Scripts.Managers;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede.Scripts.UI
{
    public class PauseUI : MonoBehaviour
    {
        [Header("UI References")]
        [field: SerializeField] public Button ResumeButton { get; private set; }
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        private void Awake()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (ResumeButton != null)
                ResumeButton.onClick.AddListener(ResumeGame);
            
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }
        
        private void OnDestroy()
        {
            // ทำความสะอาด Event Listeners
            if (ResumeButton != null)
                ResumeButton.onClick.RemoveAllListeners();
                
            if (restartButton != null)
                restartButton.onClick.RemoveAllListeners();
            
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveAllListeners();
                
            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }

        private void ResumeGame()
        {
            Time.timeScale = 1f;
            gameObject.SetActive(false);
            AudioListener.pause = false;
            GameManager.Instance.IsPaused = false;
        }

        private void RestartGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RestartGame();
            }
        }
        
        private void GoToMainMenu()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
            
            LoadSceneManager.Instance.LoadScene(SceneType.MainMenu, LoadSceneMode.Single, false);
        }
        
        private void QuitGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.QuitGame();
            }
        }
    }
}
