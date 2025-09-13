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
        [SerializeField] private List<MenuSlot> menuSlots;
        [SerializeField] private int maxDisplayCount = 5;
        [SerializeReference] private InventoryModule inventoryModule = new();
        
        private readonly Queue<MenuData> _displayQueue = new();
        private int _nextIndex;
        
        private void Start()
        {
            allMenus = InventoryModule.InventoryDataList;
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
            for (int i = 0; i < menuSlots.Count; i++)
            {
                if (i < menus.Length)
                {
                    menuSlots[i].gameObject.SetActive(true);
                    menuSlots[i].Initialize(menus[i]);
                }
                else
                {
                    menuSlots[i].gameObject.SetActive(false);
                }
            }
        }
    }
}