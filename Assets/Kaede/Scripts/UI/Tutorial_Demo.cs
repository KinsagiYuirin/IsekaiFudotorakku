using System;
using Kaede.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.UI
{
    public class TutorialDemo : MonoBehaviour
    {
        [Title("UI References")]
        [SerializeField] private Image[] tutorialImages;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button prevButton;
        [SerializeField] private Button closeButton;
        
        private int _currentIndex = 0;

        private void Awake()
        {
            SetupButtons();
        }

        private void Start()
        {
            if (!GameManager.Instance.test)
                UpdateTutorialDisplay();
        }

        private void SetupButtons()
        {
            if (nextButton != null)
                nextButton.onClick.AddListener(NextPage);
            
            if (prevButton != null)
                prevButton.onClick.AddListener(PrevPage);
            
            if (closeButton != null)
                closeButton.onClick.AddListener(CloseTutorial);
        }

        private void OnDestroy()
        {
            if (nextButton != null)
                nextButton.onClick.RemoveAllListeners();
            
            if (prevButton != null)
                prevButton.onClick.RemoveAllListeners();
            
            if (closeButton != null)
                closeButton.onClick.RemoveAllListeners();
        }

        private void UpdateTutorialDisplay()
        {
            gameObject.SetActive(true);
            if (tutorialImages.Length > 0)
            {
                tutorialImages[_currentIndex].gameObject.SetActive(true);
            }
        }
        
        private void NextPage()
        {
            if (_currentIndex < tutorialImages.Length - 1)
            {
                _currentIndex++;
            }
            else
            {
                _currentIndex = 0;
            }
            UpdateTutorialDisplay();
        }
        
        private void PrevPage()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
            }
            else
            {
                _currentIndex = tutorialImages.Length - 1;
            }
            UpdateTutorialDisplay();
        }

        private void CloseTutorial()
        {
            gameObject.SetActive(false);
            GameManager.Instance.tutorialCompleted = true;
            GameManager.Instance.ResumeGame();
        }
    }
}
