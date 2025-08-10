using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Yuirin.Script.Item
{
    /// <summary>
    /// Base class for item data.
    /// </summary>
    /// <remarks>
    /// This class is used to create ScriptableObjects that represent items in the game.
    /// </remarks>

    
    [Serializable]
    public enum ItemRarity
    {
        None = 0,
        Common = 1 << 0,
        Rare = 1 << 1,
        SuperRare = 1 << 2,
    }
    
    [Serializable]
    public enum ItemType
    {
        None = 0,
        Ingredient = 1 << 0,
        Equipment = 1 << 1,
    }

    #region Ingredient Enums
    [Serializable]
    public enum Ingredient
    {
        None = 0,
        Meat = 1 << 0,
        Vegetable = 1 << 1,
        AnimalProduct = 1 << 2,
    }

    [Serializable]
    public enum MeatType
    {
        None,
        Pork,
        Beef,
        Chicken,
        Fish,
        Shrimp,
    }

    [Serializable]
    public enum VegetableType
    {
        None,
        Cabbage,
    }

    [Serializable]
    public enum AnimalProductType
    {
        None,
        Milk,
        Egg,
    }
    #endregion
    
    [CreateAssetMenu(fileName = "ItemSO", menuName = "Scriptable Objects/ItemData")]
    public class ItemData : ScriptableObject
    {
        [field: SerializeField] public string ItemName { get; private set; }
        [field: SerializeField] public ItemRarity Rarity { get; private set; } = ItemRarity.None;
        
        [field: TabGroup("Setting")]
        [field: SerializeField] public ItemType Type { get; private set; } = ItemType.None;
        
        [field: TabGroup("Setting"), ShowIf("@Type == ItemType.Ingredient")]
        [field: SerializeField] public Ingredient IngredientType { get; private set; } = Ingredient.None;

        [field: TabGroup("Setting"), ShowIf("@IngredientType == Ingredient.Meat")]
        [field: SerializeField] public MeatType MeatType { get; private set; } = MeatType.None;
        
        [field: TabGroup("Setting"), ShowIf("@IngredientType == Ingredient.Vegetable")]
        [field: SerializeField] public VegetableType VegetableType { get; private set; } = VegetableType.None;
        
        [field: TabGroup("Setting"), ShowIf("@IngredientType == Ingredient.AnimalProduct")]
        [field: SerializeField] public AnimalProductType AnimalProductType { get; private set; } = AnimalProductType.None;
  
        [field: SerializeField] public string ItemDescription { get; private set; }
        [field: SerializeField] public Sprite ItemIcon { get; private set; }
        [field: SerializeField] public GameObject ItemPrefab { get; private set; }
        private Guid _itemId;
    }
}
