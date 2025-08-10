using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yuirin.Script.Inventory;


namespace Yuirin.Script.Item
{
    public class ItemSlot : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemName;
        //[SerializeField] private TMP_Text itemDescription;
        [SerializeField] private TMP_Text itemCount;
        
        private ItemData _itemData;
        
        private void OnEnable()
        {
            InventoryEvent.OnItemCountChanged += UpdateCount;
        }
        
        private void OnDisable()
        {
            InventoryEvent.OnItemCountChanged -= UpdateCount;
        }
        
        public void Initialize(ItemData itemData)
        {
            _itemData = itemData;
            if (itemData != null)
            {
                UpdateView();
            }
        }
        
        private void UpdateView()
        {
            itemIcon.sprite = _itemData.ItemIcon;
            itemName.text = _itemData.ItemName;
            //itemDescription.text = _itemData.ItemDescription;
        }
        
        private void UpdateCount(ItemData itemData, int previousCount, int newCount)
        {
            if (itemData != _itemData) return;
            itemCount.text = newCount.ToString();
        }
    }
}
