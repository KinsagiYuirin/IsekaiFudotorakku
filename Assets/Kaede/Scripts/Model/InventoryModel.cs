using System.Collections.Generic;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using UnityEngine;

namespace Kaede.Scripts.Model
{
    public class InventoryModel
    {
        [SerializeField] public static List<MenuData> InventoryDataList => ComboCookingController.Instance.MenuDatasList;
        
        private void UpdateMenuDataList()
        {
            
        }
    }
}