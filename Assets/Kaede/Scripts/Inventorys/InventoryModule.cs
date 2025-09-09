using System;
using System.Collections.Generic;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using UnityEngine;
using Yuirin.Script.Item;

namespace Kaede.Scripts.Inventorys
{
    public class InventoryModule
    {
        [SerializeField] public static List<MenuData> InventoryDataList => ComboCookingController.Instance.MenuDatasList;
        //public static event Action<MenuData> OnItemCountChanged;

        private void UpdateMenuDataList()
        {
            
        }
        
        /*public void InitializeInventory()
        {
            foreach (var inventoryData in InventoryDataList)
            {
                OnItemCountChanged?.Invoke(inventoryData.menuData);
            }
        }*/
    }
}