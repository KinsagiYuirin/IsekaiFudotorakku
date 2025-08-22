using System.Collections.Generic;
using UnityEngine;
using Yuirin.Script.Item;


namespace Yuirin.Script.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        [SerializeField] private ItemSlot itemSlotPrefab;
        [SerializeField] private List<ItemSlot> itemSlots;
        [SerializeReference] private InventoryEvent inventoryEvent = new();
        
        void Start()
        {
            CheckSlotCount();
            inventoryEvent.InitializeInventory();
        }

        private void CheckSlotCount()
        {
            if (itemSlots.Count < 0) return;
            
            LoadInventory();
        }

        private void LoadInventory()
        {

            /*foreach (var inventoryData in inventoryEvent.InventoryDataList)
            {
                var itemSlot = itemSlots[0];
                itemSlot.Initialize(inventoryData.itemData);
            }*/
            
            for (int i = 0; i < inventoryEvent.InventoryDataList.Count; i++)
            {
                var inventoryData = inventoryEvent.InventoryDataList[i];
                var itemSlot = itemSlots[i];
                itemSlot.Initialize(inventoryData.itemData);
            }
        }
    }
}