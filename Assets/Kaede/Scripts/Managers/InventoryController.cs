using System;
using System.Collections.Generic;
using System.Linq;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using Kaede.Scripts.Model;
using R3;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Color = System.Drawing.Color;

namespace Kaede.Scripts.Managers
{
    public class InventoryController : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private MenuSlot menuSlotPrefab;
        [SerializeField] private GameObject menuSlotContainer;
        [SerializeField] private CanvasGroup inventoryCanvasGroup;
        [SerializeField] private List<Transform> slotPositions;
        [SerializeField] private TMP_Text restingText;
        
        [Title("Settings")]
        [SerializeField] private int maxDisplayCount = 5;
        [SerializeField] private Image menuImages1;
        [SerializeField] private Image menuImages2;
        [SerializeField] private Image menuImages3;
        
        [Title("Debug")]
        [SerializeField, ReadOnly] private List<MenuData> allMenus;

        private readonly List<MenuSlot> _menuSlots = new();
        private readonly Queue<MenuData> _displayQueue = new();
        private int _nextMenuIndex;
        private InventoryModel _inventoryModel;
        private IDisposable _inventorySubscription;

        #region Unity Lifecycle
        private void Awake()
        {
            InitializeModel();
            InitializeSlots();
        }

        private void Start()
        {
            SubscribeToInventoryChanges();
            LoadInitialMenus();
        }
        
        private void OnDestroy()
        {
            Cleanup();
        }
        #endregion

        #region Public Methods
        public void SetInventoryData(List<MenuData> menuDataList)
        {
            _inventoryModel?.SetInventoryData(menuDataList);
        }
        
        public void ReloadMenus()
        {
            LoadInitialMenus();
        }

        public void SetVisible(bool isVisible)
        {
            SetContainerVisibility(isVisible);
            SetCanvasGroupVisibility(isVisible);
            
            if (isVisible)
            {
                RenderMenus();
                restingText.alpha = 0;
            }
            else
            {
                HideAllSlots();
                restingText.alpha = 1;
            }
        }

        public void CompleteMenu()
        {
            if (_displayQueue.Count == 0)
            {
                RenderMenus();
                return;
            }
            
            _displayQueue.Dequeue();
            TryEnqueueNextMenu();
            RenderMenus();
        }

        public void AddMenu(MenuData menuData)
        {
            _inventoryModel?.AddMenu(menuData);
        }

        public void RemoveMenu(MenuData menuData)
        {
            _inventoryModel?.RemoveMenu(menuData);
        }

        public void ClearInventory()
        {
            _inventoryModel?.ClearInventory();
        }

        public bool HasMenu(MenuData menuData)
        {
            return _inventoryModel?.HasMenu(menuData) ?? false;
        }

        public int GetMenuCount()
        {
            return _inventoryModel?.GetMenuCount() ?? 0;
        }
        #endregion

        #region Initialization
        private void InitializeModel()
        {
            _inventoryModel = new InventoryModel();
        }

        private void InitializeSlots()
        {
            _menuSlots.Clear();

            if (!ValidateRequiredComponents()) return;

            CreateSlotInstances();
        }

        private void CreateSlotInstances()
        {
            foreach (var position in slotPositions)
            {
                if (position == null)
                {
                    Debug.LogWarning($"{nameof(InventoryController)}: Null slot position found.");
                    continue;
                }

                var slot = Instantiate(menuSlotPrefab, position);
                slot.gameObject.SetActive(false); // Start hidden
                _menuSlots.Add(slot);
            }
        }

        private bool ValidateRequiredComponents()
        {
            if (menuSlotPrefab == null)
            {
                Debug.LogError($"{nameof(InventoryController)}: Menu slot prefab is required.");
                return false;
            }

            if (slotPositions == null || slotPositions.Count == 0)
            {
                Debug.LogError($"{nameof(InventoryController)}: Slot positions are required.");
                return false;
            }

            return true;
        }
        #endregion

        #region Event Handling
        private void SubscribeToInventoryChanges()
        {
            if (_inventoryModel == null)
            {
                Debug.LogError($"{nameof(InventoryController)}: InventoryModel is null, cannot subscribe to changes.");
                return;
            }

            _inventorySubscription = _inventoryModel.InventoryDataObservable
                .Subscribe(OnInventoryDataChanged)
                .AddTo(this); // Automatic disposal when GameObject is destroyed
        }

        private void OnInventoryDataChanged(List<MenuData> menuList)
        {
            RefreshMenuSource();
            ResetDisplayQueue();
            RenderMenus();
        }
        #endregion

        #region Data Management
        private void LoadInitialMenus()
        {
            if (!TryGetExternalMenuData(out var externalMenus))
            {
                Debug.LogWarning($"{nameof(InventoryController)}: No external menu data found.");
                return;
            }

            _inventoryModel?.SetInventoryData(externalMenus);
        }

        private bool TryGetExternalMenuData(out List<MenuData> menuData)
        {
            menuData = null;

            if (ComboCookingController.Instance == null)
            {
                Debug.LogWarning($"{nameof(InventoryController)}: ComboCookingController instance not found.");
                return false;
            }

            menuData = ComboCookingController.Instance.MenuDatasList;
            return menuData != null && menuData.Count > 0;
        }

        private void RefreshMenuSource()
        {
            allMenus = _inventoryModel?.InventoryDataList?.ToList() ?? new List<MenuData>();
        }
        #endregion

        #region Queue Management
        private void ResetDisplayQueue()
        {
            _displayQueue.Clear();
            _nextMenuIndex = 0;
            
            if (allMenus == null || allMenus.Count == 0)
            {
                return;
            }

            EnqueueInitialMenus();
        }

        private void EnqueueInitialMenus()
        {
            var initialCount = Mathf.Min(maxDisplayCount, allMenus.Count);
            
            for (var i = 0; i < initialCount; i++)
            {
                _displayQueue.Enqueue(allMenus[i]);
            }

            _nextMenuIndex = initialCount;
        }
        
        private void TryEnqueueNextMenu()
        {
            if (!CanEnqueueNextMenu()) return;

            _displayQueue.Enqueue(allMenus[_nextMenuIndex]);
            _nextMenuIndex++;
        }

        private bool CanEnqueueNextMenu()
        {
            return allMenus != null && 
                   _nextMenuIndex < allMenus.Count && 
                   _nextMenuIndex >= 0;
        }
        #endregion

        #region UI Rendering
        private void RenderMenus()
        {
            if (_menuSlots.Count == 0)
            {
                Debug.LogWarning($"{nameof(InventoryController)}: No menu slots available for rendering.");
                return;
            }
            
            RenderActiveMenus();
            HideUnusedSlots();
        }

        private void RenderActiveMenus()
        {
            var slotIndex = 0;
            
            foreach (var menu in _displayQueue)
            {
                if (slotIndex >= _menuSlots.Count) break;

                var slot = _menuSlots[slotIndex];
                slot.gameObject.SetActive(true);
                slot.Initialize(menu);
                slotIndex++;
            }
            
            MenuPaper(slotIndex);
        }

        private void HideUnusedSlots()
        {
            var activeSlots = _displayQueue.Count;
            
            for (var i = activeSlots; i < _menuSlots.Count; i++)
            {
                if (_menuSlots[i] != null)
                {
                    _menuSlots[i].gameObject.SetActive(false);
                }
            }
        }

        private void MenuPaper(int index)
        {
            switch (index)
            {
                case 3:
                    SetMenuPaperAlpha(menuImages1, 1f);
                    SetMenuPaperAlpha(menuImages2, 1f);
                    SetMenuPaperAlpha(menuImages3, 1f);
                    break;
                case 2:
                    SetMenuPaperAlpha(menuImages1, 1f);
                    SetMenuPaperAlpha(menuImages2, 1f);
                    SetMenuPaperAlpha(menuImages3, 0f);
                    break;
                case 1:
                    SetMenuPaperAlpha(menuImages1, 1f);
                    SetMenuPaperAlpha(menuImages2, 0f);
                    SetMenuPaperAlpha(menuImages3, 0f);
                    break;
            }
        }
        
        private void SetMenuPaperAlpha(Image image, float alpha)
        {
            if (image == null) return;
            
            var color = image.color;
            color.a = Mathf.Clamp01(alpha);
            image.color = color;
        }
        #endregion

        #region Visibility Management
        private void SetContainerVisibility(bool isVisible)
        {
            if (menuSlotContainer != null)
            {
                menuSlotContainer.SetActive(isVisible);
            }
            else
            {
                Debug.LogWarning($"{nameof(InventoryController)}: Menu slot container is not assigned.");
            }
        }

        private void SetCanvasGroupVisibility(bool isVisible)
        {
            if (inventoryCanvasGroup == null)
            {
                Debug.LogWarning($"{nameof(InventoryController)}: Inventory canvas group is not assigned.");
                return;
            }

            if (isVisible)
                inventoryCanvasGroup.alpha = 1f;
            else
                inventoryCanvasGroup.alpha = 0f;
            inventoryCanvasGroup.interactable = isVisible;
            inventoryCanvasGroup.blocksRaycasts = isVisible;
        }

        private void HideAllSlots()
        {
            foreach (var slot in _menuSlots)
            {
                if (slot != null)
                {
                    slot.gameObject.SetActive(false);
                }
            }
        }
        #endregion

        #region Cleanup
        private void Cleanup()
        {
            _inventorySubscription?.Dispose();
            _inventoryModel = null;
        }
        #endregion
    }
}