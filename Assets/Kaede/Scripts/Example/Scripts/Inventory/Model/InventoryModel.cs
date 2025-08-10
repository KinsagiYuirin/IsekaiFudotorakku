using System;
using System.Collections.Generic;
using SO;
using UnityEngine;

namespace InventoryEx
{
    [Serializable]
    public struct InventoryData
    {
        public ItemDataEx itemDataEx;
        public int Count;
    }
    
    [Serializable]
    public class InventoryModel
    {
        [SerializeField] public List<InventoryData> InventoryDataList = new List<InventoryData>();
        public static event Action<ItemDataEx, int, int> OnItemCountChanged;

        public void InitializeInventory()
        {
            foreach (var inventoryData in InventoryDataList)
            {
                OnItemCountChanged?.Invoke(inventoryData.itemDataEx, inventoryData.Count, inventoryData.Count);
            }
        }
    }
}