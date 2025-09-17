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
    public class RandomSystem : MonoSingleton<RandomSystem>
    {
        [SerializeField] private List<MenuData> allMenuInLevel;
        [SerializeField, Min(0)] private int appetizerRandomMenuCount = 1;
        [SerializeField, Min(0)] private int mainCourseRandomMenuCount = 1;
        [SerializeField, Min(0)] private int dessertRandomMenuCount = 1;
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

            foreach (FoodType foodType in Enum.GetValues(typeof(FoodType)))
            {
                var menusOfType = allMenuInLevel
                    .Where(menu => menu != null && menu.foodType == foodType)
                    .ToList();

                if (menusOfType.Count == 0) continue;

                var targetList = GetRandomMenuList(foodType);
                var menusToGenerate = GetRandomMenuCount(foodType);

                if (menusToGenerate <= 0)
                {
                    continue;
                }

                for (var i = 0; i < menusToGenerate; i++)
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

        private int GetRandomMenuCount(FoodType foodType)
        {
            return foodType switch
            {
                FoodType.Appetizer => Mathf.Max(0, appetizerRandomMenuCount),
                FoodType.MainCourse => Mathf.Max(0, mainCourseRandomMenuCount),
                FoodType.Dessert => Mathf.Max(0, dessertRandomMenuCount),
                _ => throw new ArgumentOutOfRangeException(nameof(foodType), foodType, null)
            };
        }
    }
}
