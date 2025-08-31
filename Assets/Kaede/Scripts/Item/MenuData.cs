using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public enum MenuLevel
{
    None = 0,
    Easy = 1 << 0,
    Normal = 1 << 1,
    Hard = 1 << 2
}

[CreateAssetMenu(fileName = "MenuData", menuName = "Scriptable Objects/MenuData")]
public class MenuData : ScriptableObject
{
    [Title("Settings")]
    [field: SerializeField] public string MenuName { get; private set; }
    [field: SerializeField] public MenuLevel MenuLevel { get; private set; }
    [field: SerializeField] public List<Key> ComboKeys { get; private set; } = new List<Key>();
    
    [Title("References")]
    [field: SerializeField] public Sprite MenuIcon { get; private set; }
    
}
