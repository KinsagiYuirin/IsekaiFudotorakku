using System.Collections.Generic;
using Kaede.Scripts.Inventorys;
using Kaede.Scripts.Item;
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
        
        private void Start()
        {
            foreach (var pos in slotPositions)
            {
                var slot = Instantiate(menuSlotPrefab, pos);
                _menuSlots.Add(slot);
            }
            
            allMenus = InventoryModel.InventoryDataList;
            int initialCount = Mathf.Min(maxDisplayCount, allMenus.Count);
            for (int i = 0; i < initialCount; i++)
            {
                _displayQueue.Enqueue(allMenus[i]);
            }
            _nextIndex = initialCount;
            RenderMenus();
        }

        public void CompleteMenu()
        {
            if (_displayQueue.Count == 0) return;

            _displayQueue.Dequeue();

            if (_nextIndex < allMenus.Count)
            {
                _displayQueue.Enqueue(allMenus[_nextIndex]);
                _nextIndex++;
            }

            RenderMenus();
        }

        private void RenderMenus()
        {
            var menus = _displayQueue.ToArray();
            for (int i = 0; i < _menuSlots.Count; i++)
            {
                if (i < menus.Length)
                {
                    _menuSlots[i].gameObject.SetActive(true);
                    _menuSlots[i].Initialize(menus[i]);
                }
                else
                {
                    _menuSlots[i].gameObject.SetActive(false);
                }
            }
        }
    }
}