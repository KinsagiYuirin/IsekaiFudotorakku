using UnityEngine;

public enum ItemType
{
    None = 0,
    Ingredient = 1,
    Armor = 2
}

[System.Serializable]
public class Item
{
    [field: SerializeField] public string name;
    [field: SerializeField] public ItemType itemType;
    [field: SerializeField] public Sprite icon;
    
}
