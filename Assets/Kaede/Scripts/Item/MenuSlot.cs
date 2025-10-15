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
            itemIcon.sprite = _menuData.menuSprite;
            //itemName.text = _menuData.menuName;
        }
    }
}
