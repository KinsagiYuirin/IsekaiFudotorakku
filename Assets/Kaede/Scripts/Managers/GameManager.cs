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
        [SerializeField, Min(1), Tooltip("Number of menus to randomly select for each food type.")]
        private int randomMenuCountPerFoodType = 1;
        [SerializeField, ReadOnly] private List<MenuData> appetizerRandomMenu = new();
        [SerializeField, ReadOnly] private List<MenuData> mainCourseRandomMenu = new();
        [SerializeField, ReadOnly] private List<MenuData> dessertRandomMenu = new();

        private Dictionary<FoodType, IReadOnlyList<MenuData>> _randomMenusByTypeView;

        public IReadOnlyDictionary<FoodType, IReadOnlyList<MenuData>> RandomMenusByType
        {
            get
            {
                EnsureRandomMenuLists();

                _randomMenusByTypeView ??= new Dictionary<FoodType, IReadOnlyList<MenuData>>
                {
                    { FoodType.Appetizer, appetizerRandomMenu },
                    { FoodType.MainCourse, mainCourseRandomMenu },
                    { FoodType.Dessert, dessertRandomMenu }
                };

                return _randomMenusByTypeView;
            }
        }

        private void Start()
        {
            if (!HasAnyRandomMenus())
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
            if (forceRegenerate || !HasAnyRandomMenus())
            {
                GenerateRandomMenuByFoodType();
            }

            EnsureRandomMenuLists();

            return RandomMenusByType
                .SelectMany(pair => pair.Value ?? Array.Empty<MenuData>())
                .Where(menu => menu != null)
                .ToList();
        }

        private void GenerateRandomMenuByFoodType()
        {
            EnsureRandomMenuLists();

            appetizerRandomMenu.Clear();
            mainCourseRandomMenu.Clear();
            dessertRandomMenu.Clear();

            if (allMenuInLevel == null || allMenuInLevel.Count == 0)
            {
                return;
            }

            if (randomMenuCountPerFoodType <= 0)
            {
                return;
            }

            foreach (FoodType foodType in Enum.GetValues(typeof(FoodType)))
            {
                var menusOfType = allMenuInLevel
                    .Where(menu => menu != null && menu.foodType == foodType)
                    .ToList();

                if (menusOfType.Count == 0) continue;

                var targetList = GetRandomMenuList(foodType);

                for (var i = 0; i < randomMenuCountPerFoodType; i++)
                {
                    int randomIndex = UnityEngine.Random.Range(0, menusOfType.Count);
                    targetList.Add(menusOfType[randomIndex]);
                }
            }
        }
        
        private void EnsureRandomMenuLists()
        {
            var listsReinitialized = false;

            if (appetizerRandomMenu == null)
            {
                appetizerRandomMenu = new List<MenuData>();
                listsReinitialized = true;
            }

            if (mainCourseRandomMenu == null)
            {
                mainCourseRandomMenu = new List<MenuData>();
                listsReinitialized = true;
            }

            if (dessertRandomMenu == null)
            {
                dessertRandomMenu = new List<MenuData>();
                listsReinitialized = true;
            }

            if (listsReinitialized)
            {
                _randomMenusByTypeView = null;
            }
        }

        private bool HasAnyRandomMenus()
        {
            EnsureRandomMenuLists();

            return (appetizerRandomMenu != null && appetizerRandomMenu.Count > 0)
                   || (mainCourseRandomMenu != null && mainCourseRandomMenu.Count > 0)
                   || (dessertRandomMenu != null && dessertRandomMenu.Count > 0);
        }

        private List<MenuData> GetRandomMenuList(FoodType foodType)
        {
            EnsureRandomMenuLists();

            return foodType switch
            {
                FoodType.Appetizer => appetizerRandomMenu,
                FoodType.MainCourse => mainCourseRandomMenu,
                FoodType.Dessert => dessertRandomMenu,
                _ => throw new ArgumentOutOfRangeException(nameof(foodType), foodType, null)
            };
        }
    }
}
