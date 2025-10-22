using System.Collections.Generic;
using Kaede.Scripts.Item;
using R3;

namespace Kaede.Scripts.Model
{
    public class InventoryModel
    {
        private List<MenuData> _inventoryDataList;
        private readonly ReactiveProperty<List<MenuData>> _inventoryData = new();
        
        public IReadOnlyList<MenuData> InventoryDataList => _inventoryDataList ?? new List<MenuData>();
        public ReadOnlyReactiveProperty<List<MenuData>> InventoryDataObservable => _inventoryData;

        #region Constructor
        public InventoryModel(List<MenuData> initialData = null)
        {
            SetInventoryData(initialData ?? new List<MenuData>());
        }
        #endregion

        #region Public Methods
        public void SetInventoryData(List<MenuData> menuDataList)
        {
            _inventoryDataList = menuDataList ?? new List<MenuData>();
            _inventoryData.Value = _inventoryDataList;
        }

        public void AddMenu(MenuData menuData)
        {
            if (menuData == null) return;
            
            _inventoryDataList ??= new List<MenuData>();

            if (_inventoryDataList.Contains(menuData)) return;
            _inventoryDataList.Add(menuData);
            _inventoryData.Value = _inventoryDataList;
        }

        public void RemoveMenu(MenuData menuData)
        {
            if (menuData == null || _inventoryDataList == null) return;
            
            if (_inventoryDataList.Remove(menuData))
            {
                _inventoryData.Value = _inventoryDataList;
            }
        }

        public void ClearInventory()
        {
            _inventoryDataList?.Clear();
            _inventoryData.Value = _inventoryDataList ?? new List<MenuData>();
        }

        public bool HasMenu(MenuData menuData)
        {
            return _inventoryDataList?.Contains(menuData) ?? false;
        }

        public int GetMenuCount()
        {
            return _inventoryDataList?.Count ?? 0;
        }

        public MenuData GetMenuAt(int index)
        {
            if (_inventoryDataList == null || index < 0 || index >= _inventoryDataList.Count)
                return null;
            
            return _inventoryDataList[index];
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            _inventoryData?.Dispose();
        }
        #endregion
    }
}