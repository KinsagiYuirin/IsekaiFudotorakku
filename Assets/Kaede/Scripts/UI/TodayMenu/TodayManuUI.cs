using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.Item;
using Kaede.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.UI.TodayMenu
{
    public class TodayManuUI : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private float timeFadeIn = 1f;
        [SerializeField] private float timeFadeOut = 1f;
        [SerializeField] private Image bgImage;

        [SerializeField] private TodayMenuPrefab menuPrefab;
        [SerializeField] private Transform menuParent;

        [Title("References")]
        [SerializeField] private RandomSystem randomSystem;
        [SerializeField] private FadeFromRight fadeFromRight;

        private readonly List<TodayMenuPrefab> _activeMenus = new();

        private void Start()
        {
            SetFood();
            WaitForAnyPress(true).Forget();
        }
        
        private void SetFood()
        {
            foreach (var m in _activeMenus)
            {
                if (m) Destroy(m.gameObject);
            }
            _activeMenus.Clear();

            foreach (var t in randomSystem.AllMenuInLevel)
            {
                var menuObj = Instantiate(menuPrefab, menuParent);
                menuObj.SetFoodDeta(t);
                _activeMenus.Add(menuObj);
            }
        }

        private async UniTaskVoid WaitForAnyPress(bool activeAnimation)
        {
            await UniTask.WaitUntil(() =>
                Input.anyKeyDown ||
                Input.GetMouseButtonDown(0) ||
                Input.GetMouseButtonDown(1) ||
                Input.GetMouseButtonDown(2));

            Debug.Log("Press detected!");
            
            if (activeAnimation)
            {
                await TodayMenuAnimation();
                WaitForAnyPress(false).Forget();
            }
            else
            {
                fadeFromRight.StartFade(timeFadeIn);
                await UniTask.Delay(TimeSpan.FromSeconds(timeFadeIn));
                
                gameObject.SetActive(false);

                fadeFromRight.FadeOut(timeFadeOut);
                await UniTask.Delay(TimeSpan.FromSeconds(timeFadeOut));
                
                GameManager.Instance.ResumeGame();
            }
        }

        private async UniTask TodayMenuAnimation()
        {
            foreach (var menu in _activeMenus)
            {
                if (menu) menu.PlayCoverAnimation();
            }

            await UniTask.Delay(TimeSpan.FromSeconds(0.5f));
        }
    }
}
