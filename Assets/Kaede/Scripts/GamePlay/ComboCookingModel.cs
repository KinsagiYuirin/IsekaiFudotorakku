using System.Collections.Generic;
using MadDuck.Scripts.Inputs;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede.Scripts.GamePlay
{
    
    public class ComboCookingModel
    {
        public List<MenuData> MenuDatas { get; private set; }
        public int CurrentMenuIndex { get; private set; } = 0;
        public int CurrentComboIndex { get; private set; } = 0;

        public float MaxTimePerCombo { get; private set; }
        public float CurrentTimer { get; private set; }

        public ComboCookingModel(List<MenuData> menus, float maxTimePerCombo = 5f)
        {
            MenuDatas = menus ?? new List<MenuData>();
            MaxTimePerCombo = maxTimePerCombo;
            CurrentTimer = maxTimePerCombo;
        }

        public void Tick(float deltaTime)
        {
            CurrentTimer -= deltaTime;
        }

        public void ResetCombo()
        {
            CurrentComboIndex = 0;
            CurrentTimer = MaxTimePerCombo;
        }

        public void NextCombo()
        {
            CurrentComboIndex++;
            CurrentTimer = MaxTimePerCombo;
        }

        public void NextMenu()
        {
            CurrentMenuIndex++;
            CurrentComboIndex = 0;
            CurrentTimer = MaxTimePerCombo;
        }
        
        public void CompleteMenu()
        {
            
        }
    }
}
