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
        [SerializeField] private List<MenuSlot> menuSlots;
        [SerializeReference] private InventoryModule inventoryModule = new();
        
        private void Start()
        {
            CheckSlotCount();
            //inventoryEvent.InitializeInventory();
        }

        private void CheckSlotCount()
        {
            if (menuSlots.Count < 0) return;
            
            LoadInventory();
        }

        private void LoadInventory()
        {
            for (int i = 0; i < InventoryModule.InventoryDataList.Count; i++)
            {
                var inventoryData = InventoryModule.InventoryDataList[i];
                var menuSlot      = menuSlots[i];
                menuSlot.Initialize(inventoryData);
            }
        }
    }
}