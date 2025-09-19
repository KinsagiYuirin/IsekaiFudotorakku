using System.Collections.Generic;
using Kaede.Scripts.Item;
using Kaede.Scripts.Model;
using UnityEngine;
using Yuirin.Script.Item;

namespace Kaede.Scripts.Managers
{
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private MenuSlot menuSlotPrefab;
        [SerializeField] private List<MenuData> allMenus;
        [SerializeField] private List<Transform> slotPositions;
        [SerializeField] private int maxDisplayCount = 5;
        [SerializeReference] private InventoryModel inventoryModel = new();
        
        private readonly List<MenuSlot> _menuSlots = new();
        private readonly Queue<MenuData> _displayQueue = new();
        private int _nextIndex;
        
        private void Awake()
        {
            InitializeSlots();
        }
        
        private void Start()
        {
            LoadInitialMenus();
        }
        
        private void LoadInitialMenus()
        {
            RefreshMenuSource();
            ResetDisplayQueue();
            RenderMenus();
        }
        
        public void ReloadMenus()
        {
            LoadInitialMenus();
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

        private void InitializeSlots()
        {
            _menuSlots.Clear();

            if (menuSlotPrefab == null)
            {
                Debug.LogWarning($"{nameof(InventoryController)} requires a menu slot prefab.");
                return;
            }

            if (slotPositions == null || slotPositions.Count == 0)
            {
                Debug.LogWarning($"{nameof(InventoryController)} has no slot positions configured.");
                return;
            }

            foreach (var position in slotPositions)
            {
                if (position == null) continue;

                var slot = Instantiate(menuSlotPrefab, position);
                _menuSlots.Add(slot);
            }
        }
        
        private void RefreshMenuSource()
        {
            allMenus = InventoryModel.InventoryDataList ?? new List<MenuData>();
        }
        
        private void ResetDisplayQueue()
        {
            _displayQueue.Clear();
            _nextIndex = 0;
            
            if (allMenus == null || allMenus.Count == 0) return;

            var initialCount = Mathf.Min(maxDisplayCount, allMenus.Count);
            for (var i = 0; i < initialCount; i++)
            {
                _displayQueue.Enqueue(allMenus[i]);
            }

            _nextIndex = initialCount;
        }
        
        private void TryEnqueueNextMenu()
        {
            if (allMenus == null || _nextIndex >= allMenus.Count) return;

            _displayQueue.Enqueue(allMenus[_nextIndex]);
            _nextIndex++;
        }
        
        private void RenderMenus()
        {
            if (_menuSlots.Count == 0) return;
            
            var index = 0;
            foreach (var menu in _displayQueue)
            {
                if (index >= _menuSlots.Count) break;
            
                var slot = _menuSlots[index];
                slot.gameObject.SetActive(true);
                slot.Initialize(menu);
                index++;
            }

            for (; index < _menuSlots.Count; index++)
            {
                _menuSlots[index].gameObject.SetActive(false);
            }
        }
    }
}