using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Item
{
    public enum ComboKey
    {
        None,
        W,
        A,
        S,
        D
    }

    public enum ComboType
    {
        Single,
        Double,
        Hold,
        Stack
    }

    [Serializable]
    public enum MenuLevel
    {
        None = 0,
        Easy = 1 << 0,
        Normal = 1 << 1,
        Hard = 1 << 2
    }
    
    public enum StepPhase
    {
        Preparation,
        Cooking
    }
    
    [Serializable]
    public class ComboKeySetting
    {
        public ComboKey key;
        public ComboType type;
    
        [ShowIf("type", ComboType.Double)] 
        public float doubleTapDelay = 0.3f;
        [ShowIf("type", ComboType.Double)]
        public int pressCount = 2;
    
        [ShowIf("type", ComboType.Hold)] 
        public float holdTime = 1.0f;
    
        [ShowIf("type", ComboType.Stack)] 
        public int stackCount = 3;
    }
    
    [CreateAssetMenu(fileName = "ComboPreset", menuName = "Kaede/ComboPreset")]
    public class ComboPreset : ScriptableObject
    {
        [Title("Info")]
        [LabelText("Name")] public string DisplayName;

        [EnumToggleButtons, LabelText("Phase")]
        public StepPhase Phase; // Preparation / Cooking

        [Title("Sequence")]
        [ListDrawerSettings(
            Expanded = true, DraggableItems = true,
            NumberOfItemsPerPage = 6, ShowPaging = true)]
        public List<ComboKeySetting> ComboSequence = new();
    }
}