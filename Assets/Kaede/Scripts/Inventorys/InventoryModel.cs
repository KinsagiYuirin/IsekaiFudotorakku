using System;
using System.Collections.Generic;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using UnityEngine;
using Yuirin.Script.Item;

namespace Kaede.Scripts.Inventorys
{
    public class InventoryModel
    {
        [SerializeField] public static List<MenuData> InventoryDataList => ComboCookingController.Instance.MenuDatasList;
        
        private void UpdateMenuDataList()
        {
            
        }
    }
}