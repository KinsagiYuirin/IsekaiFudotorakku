using System;
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
        [field: Header("UI References")]
        [field: SerializeField] public Button RestartButton {get ; private set;}
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        [Title("Text")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private TMP_Text niceText;
    
        private void Awake()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            //if (restartButton != null)
                RestartButton.onClick.AddListener(RestartGame);
                
            //if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
                
            //if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }

        private void OnEnable()
        {
            GameManager.Instance.SetInteractable(false);
        }

        private void OnDestroy()
        {
            if (RestartButton != null)
                RestartButton.onClick.RemoveAllListeners();
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveAllListeners();
                
            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }
    
        public void SetResultScore(float score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score:N0}";
                
                if (niceText == null) return;
                switch (score)
                {
                    case > 1000: 
                        niceText.text = "Perfect!";
                        break;
                        
                    case > 500:
                        niceText.text = "Nice!";
                        break;
                    
                    default:
                        niceText.text = "Let's Try Again!";
                        break;
                }
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
