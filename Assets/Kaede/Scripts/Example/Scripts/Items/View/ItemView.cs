using System;
using InventoryEx;
using SO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Items.View
{
    public class ItemViewEx : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image itemImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text countText;
    
        private ItemDataEx _itemDataEx;

        private void OnEnable()
        {
            InventoryModel.OnItemCountChanged += UpdateCount;
        }

        private void OnDisable()
        {
            InventoryModel.OnItemCountChanged -= UpdateCount;
        }

        public void Initialize(ItemDataEx itemDataEx)
        {
            _itemDataEx = itemDataEx;
            if (itemDataEx != null)
            {
                UpdateView();
            }
        }
    
        public void UpdateView()
        {
            itemImage.sprite = _itemDataEx.ItemIcon;
            nameText.text = _itemDataEx.ItemName;
            descriptionText.text = _itemDataEx.ItemDescription;
        }
        
        public void UpdateCount(ItemDataEx itemDataEx, int previousCount, int newCount)
        {
            if (itemDataEx != _itemDataEx) return;
            countText.text = newCount.ToString();
        }
    }
}
