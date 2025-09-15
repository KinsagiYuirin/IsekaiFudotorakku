using System.Collections.Generic;
using Kaede.Scripts.Item;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Title("Level Settings")]
        [field: SerializeField] public List<MenuData> MenuInLevel { get; private set; }
        
        private Dictionary<FoodType ,List<MenuData>> _menuLookup;
        
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _menuLookup = new Dictionary<FoodType, List<MenuData>>();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        private void ListMenuInLevel()
        {
            foreach (var menuData in MenuInLevel)
            {
                AddMenu(_menuLookup, menuData);
            }
        }

        private void AddMenu(Dictionary<FoodType, List<MenuData>> dictionary, MenuData menu)
        {
            if (!dictionary.TryGetValue(menu.foodType, out var list))
                dictionary[menu.foodType] = list = new List<MenuData>();
            
            list.Add(menu);
        }
    }
}
