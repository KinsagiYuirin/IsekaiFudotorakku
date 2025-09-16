using System;
using System.Collections.Generic;
using System.Linq;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace Kaede.Scripts.Managers
{
    
    public class GameManager : MonoSingleton<GameManager>
    {
        [SerializeField] private List<MenuData> allMenuInLevel;
        [SerializeField, ReadOnly] private List<MenuData> randomMenu = new List<MenuData>();
        
        public IReadOnlyList<MenuData> RandomMenu => randomMenu;

        private void Start()
        {
            if (randomMenu == null || randomMenu.Count == 0)
            {
                RefreshRandomMenu();
            }
        }

        [Button]
        public void RefreshRandomMenu()
        {
            GenerateRandomMenuByFoodType();
        }

        public List<MenuData> GetMenuSetForCombo(bool forceRegenerate = false)
        {
            if (forceRegenerate || randomMenu == null || randomMenu.Count == 0)
            {
                GenerateRandomMenuByFoodType();
            }

            return new List<MenuData>(randomMenu);
        }

        private void GenerateRandomMenuByFoodType()
        {
            if (randomMenu == null)
            {
                randomMenu = new List<MenuData>();
            }
            else
            {
                randomMenu.Clear();
            }

            if (allMenuInLevel == null || allMenuInLevel.Count == 0)
            {
                return;
            }

            foreach (FoodType foodType in Enum.GetValues(typeof(FoodType)))
            {
                var menusOfType = allMenuInLevel
                    .Where(menu => menu != null && menu.foodType == foodType)
                    .ToList();

                if (menusOfType.Count == 0) continue;

                int randomIndex = UnityEngine.Random.Range(0, menusOfType.Count);
                randomMenu.Add(menusOfType[randomIndex]);
            }
        }
    }
}
