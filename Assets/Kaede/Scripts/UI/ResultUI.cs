using Kaede.Scripts.Managers;
using MadDuck.Scripts.Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede.Scripts.UI
{
    public class ResultUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button restartButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        [Title("Text")]
        [SerializeField] private TMP_Text scoreText;
    
        private void Awake()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDestroy()
        {
            if (restartButton != null)
                restartButton.onClick.RemoveAllListeners();
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveAllListeners();
                
            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }
    
        public void SetResultScore(float score)
        {
            if (scoreText != null)
            {
                scoreText.text = score.ToString("N");
            }
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
