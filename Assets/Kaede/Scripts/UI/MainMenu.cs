using Kaede.Scripts.Managers;
using MadDuck.Scripts.Managers;
using MessagePipe;
using Sherbert.Framework.Generic;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede.Scripts.UI
{
    public enum MainMenuPanelType
    {
        GameStart,
        MainMenu,
        Settings,
    }

    public struct SceneActivateEvent
    {
        public readonly SceneType sceneType;
        public SceneActivateEvent(SceneType sceneType)
        {
            this.sceneType = sceneType;
        }
    }
    
    public class MainMenu : MonoSingleton<MainMenu>
    {
        [Title("Panel GameObjects")]
        [SerializeField] private SerializableDictionary<MainMenuPanelType, GameObject> panelGameObjects = new();
        
        [Title("UI Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        
        [Title("Settings")]
        [SerializeField] private MainMenuPanelType initialPanelType = MainMenuPanelType.GameStart;
        [SerializeField] private bool disableInputDuringTransition = true;

        private MainMenuPanelType _currentPanelType;
        private bool _inputDisabled = false;

        protected override void Awake()
        {
            base.Awake();
            SetupButtons();
            InitializePanels();
        }

        private void Start()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
        }

        private void OnEnable()
        {
            LoadSceneManager.OnFinishFadeIn += OnFinishLoad;
            LoadSceneManager.OnStartFadeOut += OnStartFadeOut;
        }

        private void OnDisable()
        {
            LoadSceneManager.OnFinishFadeIn -= OnFinishLoad;
            LoadSceneManager.OnStartFadeOut -= OnStartFadeOut;
        }

        private void OnDestroy()
        {
            CleanupButtons();
        }

        #region Panel Management

        private void InitializePanels()
        {
            foreach (var panel in panelGameObjects.Values)
            {
                if (panel != null)
                {
                    panel.SetActive(false);
                }
            }
        }

        private void ShowPanel(MainMenuPanelType panelType)
        {
            if (_inputDisabled) return;
            
            if (panelGameObjects.ContainsKey(_currentPanelType) && panelGameObjects[_currentPanelType] != null)
            {
                panelGameObjects[_currentPanelType].SetActive(false);
            }

            if (panelGameObjects.ContainsKey(panelType) && panelGameObjects[panelType] != null)
            {
                panelGameObjects[panelType].SetActive(true);
                _currentPanelType = panelType;
                Debug.Log($"Showing panel: {panelType}");
            }
            else
            {
                Debug.LogWarning($"Panel {panelType} not found or is null!");
            }
        }

        private void ShowFirstPanel()
        {
            ShowPanel(initialPanelType);
        }

        #endregion

        #region Button Setup

        private void SetupButtons()
        {
            if (startButton != null)
                startButton.onClick.AddListener(StartGame);

            if (settingsButton != null)
                settingsButton.onClick.AddListener(ShowSettings);
                
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }
        
        private void CleanupButtons()
        {
            if (startButton != null)
                startButton.onClick.RemoveAllListeners();
                
            if (settingsButton != null)
                settingsButton.onClick.RemoveAllListeners();

            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (startButton != null)
                startButton.interactable = interactable;
            if (settingsButton != null)
                settingsButton.interactable = interactable;
            if (quitButton != null)
                quitButton.interactable = interactable;
        }

        #endregion

        #region Event Handlers

        private void OnStartFadeOut()
        {
            if (disableInputDuringTransition)
            {
                _inputDisabled = true;
                SetButtonsInteractable(false);
            }
        }
        
        private void OnFinishLoad()
        {
            if (LoadSceneManager.FirstSceneLoaded) initialPanelType = MainMenuPanelType.MainMenu;
            _inputDisabled = false;
            SetButtonsInteractable(true);
            ShowFirstPanel();
        }

        #endregion

        public void StartGame()
        {
            if (_inputDisabled) return;
            var nextScene = SceneType.Gameplay;
            LoadSceneManager.Instance.LoadScene(nextScene, LoadSceneMode.Single, false);
        }
        
        public void ShowSettings()
        {
            if (_inputDisabled) return;
            ShowPanel(MainMenuPanelType.Settings);
        }
        
        public void BackToMainMenu()
        {
            if (_inputDisabled) return;
            ShowPanel(MainMenuPanelType.MainMenu);
        }

        public void QuitGame()
        {
            if (_inputDisabled) return;
            
            Debug.Log("Quitting game...");
            
            if (GameManager.Instance != null && GameManager.Instance.IsPaused)
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
