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
        [SerializeField] private List<MenuData> allMenuInLevel;
        [SerializeField, ReadOnly] private List<MenuData> randomMenu = new List<MenuData>();
    }
}
