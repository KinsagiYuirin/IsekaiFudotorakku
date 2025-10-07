using Kaede.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.UI
{
    public class PauseUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button mainMenuButton;
        [SerializeField] private Button quitButton;
        
        private void Awake()
        {
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(ResumeGame);
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(GoToMainMenu);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }
        
        private void OnDestroy()
        {
            // ทำความสะอาด Event Listeners
            if (resumeButton != null)
                resumeButton.onClick.RemoveAllListeners();
                
            if (mainMenuButton != null)
                mainMenuButton.onClick.RemoveAllListeners();
                
            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }
        
        public void ResumeGame()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
        }
        
        private void GoToMainMenu()
        {
            // Resume ก่อนเปลี่ยน Scene
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
            
            // โหลด Main Menu Scene
            // ใช้ LoadSceneManager ที่มีอยู่
            // LoadSceneManager.Instance.LoadScene(SceneType.MainMenu, LoadSceneMode.Single, false);
        }
        
        private void QuitGame()
        {
            // Resume ก่อนออกจากเกม
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResumeGame();
            }
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
}
