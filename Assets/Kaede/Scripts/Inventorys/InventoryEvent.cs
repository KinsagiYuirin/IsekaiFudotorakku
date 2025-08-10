using System;
using System.Collections.Generic;
using UnityEngine;
using Yuirin.Script.Item;

namespace Yuirin.Script.Inventory
{
    [Serializable]
    public struct InventoryData
    {
        public ItemData itemData;
        public int count;
    }
    public class InventoryEvent
    {
        [SerializeField] public List<InventoryData> InventoryDataList = new List<InventoryData>();
        public static event Action<ItemData, int, int> OnItemCountChanged;

        public void InitializeInventory()
        {
            foreach (var inventoryData in InventoryDataList)
            {
                OnItemCountChanged?.Invoke(inventoryData.itemData, inventoryData.count, inventoryData.count);
            }
        }
    }
}