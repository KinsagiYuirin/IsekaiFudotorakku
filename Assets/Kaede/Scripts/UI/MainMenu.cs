using Cysharp.Threading.Tasks;
using Kaede.Scripts.Managers;
using MadDuck.Scripts.Managers;
using MessagePipe;
using Sherbert.Framework.Generic;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Kaede.Scripts.UI
{
    public enum MainMenuPanelType
    {
        GameStart,
        MainMenu,
        Tutorial,
        Settings,
    }

    public struct SceneActivateEvent
    {
        public readonly SceneType SceneType;
        public SceneActivateEvent(SceneType sceneType)
        {
            this.SceneType = sceneType;
        }
    }
    
    public class MainMenu : MonoSingleton<MainMenu>
    {
        [Title("Panel GameObjects")]
        [SerializeField] private SerializableDictionary<MainMenuPanelType, GameObject> panelGameObjects = new();
        
        [Title("UI Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button closeTutorialButton;
        
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
            
            DelaySelect(startButton.gameObject).Forget();
        }

        private void OnEnable()
        {
            LoadSceneManager.OnFinishFadeIn += OnFinishLoad;
            LoadSceneManager.OnStartFadeOut += OnStartFadeOut;
            InputSystem.onAnyButtonPress += OnAnyButton;
        }

        private void OnDisable()
        {
            LoadSceneManager.OnFinishFadeIn -= OnFinishLoad;
            LoadSceneManager.OnStartFadeOut -= OnStartFadeOut;
            InputSystem.onAnyButtonPress -= OnAnyButton;
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

            if (tutorialButton != null)
                tutorialButton.onClick.AddListener(OpenTutorial);
            
            if (closeTutorialButton != null)
                closeTutorialButton.onClick.AddListener(CloseTutorial);
            
            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }
        
        private void CleanupButtons()
        {
            if (startButton != null)
                startButton.onClick.RemoveAllListeners();

            if (tutorialButton != null)
                tutorialButton.onClick.RemoveAllListeners();
            
            if (closeTutorialButton != null)
                closeTutorialButton.onClick.RemoveAllListeners();

            if (quitButton != null)
                quitButton.onClick.RemoveAllListeners();
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (startButton != null)
                startButton.interactable = interactable;

            if ( tutorialButton != null)
                tutorialButton.interactable = interactable;
            
            if (closeTutorialButton != null)
                closeTutorialButton.interactable = interactable;
            
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

        #region Public Methods
        
        public void StartGame()
        {
            if (_inputDisabled) return;
            var nextScene = SceneType.Gameplay;
            LoadSceneManager.Instance.LoadScene(nextScene, LoadSceneMode.Single, false);
        }
        
        public void OpenTutorial()
        {
            if (_inputDisabled) return;
            ShowPanel(MainMenuPanelType.Tutorial);
            DelaySelect(closeTutorialButton.gameObject).Forget();
        }
        
        public void CloseTutorial()
        {
            if (_inputDisabled) return;
            ShowPanel(MainMenuPanelType.MainMenu);
            DelaySelect(tutorialButton.gameObject).Forget();
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
        
        #endregion

        #region Private Methods

        private async UniTask DelaySelect(GameObject toSelect = null)
        {
            if (Gamepad.current == null && !Gamepad.current.wasUpdatedThisFrame) return;
            await UniTask.Yield();
            EventSystem.current.SetSelectedGameObject(toSelect);
        }

        private void OnAnyButton(InputControl control)
        {
            if (control.device is Gamepad)
            {
                Debug.Log("Controller is active");
            }
            else if (control.device is Keyboard || control.device is Mouse)
            {
                Debug.Log("Keyboard/Mouse is active");
            }
        }
        
        #endregion
    }
}
