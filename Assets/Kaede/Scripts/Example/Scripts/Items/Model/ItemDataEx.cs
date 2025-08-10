using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SO
{
    [CreateAssetMenu(fileName = "NewItem", menuName = "Scriptable Objects/ItemDataEx", order = 1)]
    public class ItemDataEx : ScriptableObject
    {
        [field: SerializeField] public string ItemName {get ; private set;}
        [field: SerializeField] public string ItemDescription {get; private set;}
        [field: SerializeField] public Sprite ItemIcon {get ; private set;}
        private Guid _itemId;
    }
}
