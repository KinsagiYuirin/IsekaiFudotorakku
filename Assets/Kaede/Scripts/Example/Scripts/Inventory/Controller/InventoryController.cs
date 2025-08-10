using System;
using System.Collections.Generic;
using Items.View;
using SO;
using UnityEngine;

namespace InventoryEx
{
    
    public class InventoryController : MonoBehaviour
    {
        [SerializeField] private ItemViewEx itemViewExPrefab;
        [SerializeField] private Transform itemViewContainer;
        [SerializeReference] private InventoryModel inventoryModel = new();
        
        private readonly List<ItemViewEx> _itemViews = new();

        private void Start()
        {
            LoadInventory();
            inventoryModel.InitializeInventory();
        }
        
        private void LoadInventory()
        {
            // Clear existing item views
            foreach (var itemView in _itemViews)
            {
                Destroy(itemView.gameObject);
            }
            _itemViews.Clear();

            // Create new item views based on inventory data
            foreach (var inventoryData in inventoryModel.InventoryDataList)
            {
                var itemView = Instantiate(itemViewExPrefab, itemViewContainer);
                itemView.Initialize(inventoryData.itemDataEx);
                _itemViews.Add(itemView);
            }
        }
    }
}