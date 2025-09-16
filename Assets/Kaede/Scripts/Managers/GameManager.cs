using System.Collections.Generic;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using Sirenix.OdinInspector;
using UnityCommunity.UnitySingleton;
using UnityEngine;

namespace Kaede.Scripts.Managers
{
    
    public class GameManager : MonoSingleton<GameManager>
    {
        [Title("Level Settings")]
        [field: SerializeField] public List<MenuData> MenuInLevel { get; private set; }
        [SerializeField] private int numberOfMenus = 3;

        private Dictionary<FoodType, List<MenuData>> _menuLookup;
        private ComboCookingController _comboCookingController;

        protected override void Awake()
        {
            _comboCookingController = GetComponent<ComboCookingController>();
            _menuLookup = new Dictionary<FoodType, List<MenuData>>();
            ListMenuInLevel();

            SetRandomMenus();
            base.Awake();
        }

        private void ListMenuInLevel()
        {   
            foreach (var menuData in MenuInLevel)
            {
                if (menuData == null) continue;
                AddMenu(_menuLookup, menuData);
            }
        }

        private void AddMenu(Dictionary<FoodType, List<MenuData>> dictionary, MenuData menu)
        {
            if (!dictionary.TryGetValue(menu.foodType, out var list))
                dictionary[menu.foodType] = list = new List<MenuData>();
            list.Add(menu);
        }

        private void SetRandomMenus()
        {
            var result = new List<MenuData>();
            var allPool = new List<MenuData>(MenuInLevel); // สำรองไว้ใช้เติม

            // 1) เลือก 1 ต่อชนิดก่อน (ถ้าอยากบาลานซ์)
            foreach (var (_, menus) in _menuLookup)
            {
                if (menus == null || menus.Count == 0) continue;
                var pick = menus[Random.Range(0, menus.Count)];
                if (!result.Contains(pick))
                    result.Add(pick);
            }

            // 2) ถ้ายังไม่ครบ numberOfMenus → เติมจาก pool ทั้งหมดแบบไม่ซ้ำ
            //    เอาเมนูที่ยังไม่ได้ถูกเลือก ออกมาเป็น candidates
            var candidates = new List<MenuData>(allPool);
            candidates.RemoveAll(m => result.Contains(m));

            while (result.Count < numberOfMenus && candidates.Count > 0)
            {
                int idx = Random.Range(0, candidates.Count);
                result.Add(candidates[idx]);
                candidates.RemoveAt(idx); // กันซ้ำ
            }

            // 3) ถ้าบาลานซ์ชนิดแล้วเกิน numberOfMenus → ตัดให้เหลือเท่าที่ต้องการ
            if (result.Count > numberOfMenus)
            {
                // สุ่มทิ้งให้เหลือ numberOfMenus
                for (int i = result.Count - 1; i >= numberOfMenus; i--)
                {
                    int idx = Random.Range(0, result.Count);
                    result.RemoveAt(idx);
                }
            }
            
            var controller = _comboCookingController;
            if (controller == null) return;

            controller.OverrideMenuDatas(result);
        }
    }

}
