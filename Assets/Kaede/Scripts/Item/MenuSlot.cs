using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Item
{
    public class MenuSlot : MonoBehaviour
    {
        [Title("References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TMP_Text itemName;
        
        private MenuData _menuData;
        
        private void OnEnable()
        {
            //InventoryEvent.OnItemCountChanged += UpdateCount;
        }
        
        private void OnDisable()
        {
            //InventoryEvent.OnItemCountChanged -= UpdateCount;
        }
        
        public void Initialize(MenuData menuData)
        {
            _menuData = menuData;
            if (menuData != null)
            {
                UpdateView();
            }
        }
        
        private void UpdateView()
        {
            itemIcon.sprite = _menuData.MenuIcon;
            itemName.text = _menuData.MenuName;
        }
        
        /*private void UpdateCount(MenuData menuData, int previousCount, int newCount)
        {
            if (menuData != _menuData) return;
            itemCount.text = newCount.ToString();
        }*/
    }
}
