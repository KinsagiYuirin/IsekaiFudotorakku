using System;
using System.Collections.Generic;
using Kaede.Scripts.Animation;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Item
{
    public enum FoodType
    {
        Appetizer,
        MainCourse,
        Dessert
    }
    
    [System.Serializable]
    public class MenuStepRef
    {
        [HorizontalGroup("Row", 0.6f)] [HideLabel, AssetsOnly, Required]
        public ComboPreset preset;

        [HorizontalGroup("Row", 0.4f)] [HideLabel, LabelText("Override Sequence?")]
        public bool overrideSequence;
        

        [ShowIf(nameof(overrideSequence))] [TableList(ShowIndexLabels = true)]
        public List<ComboKeySetting> customSequence = new();

        public List<ComboKeySetting> ResolveSequence()
            => overrideSequence && customSequence != null && customSequence.Count > 0
                ? customSequence
                : preset != null ? preset.comboSequence : new List<ComboKeySetting>();
        
        public ComboStepAnimationDefinition ResolveAnimation()
            => preset != null ? preset.ResolveAnimationDefinition() : ComboStepAnimationDefinition.None;
    }

    [CreateAssetMenu(fileName = "MenuData", menuName = "Kaede/MenuData")]
    public class MenuData : ScriptableObject
    {
        [Title("Settings")]
        [LabelText("Menu Name")] public string menuName;
        public FoodType foodType;
        public MenuLevel menuLevel;

        [Title("Steps (Select Preset or Override Sequence)")]
        [ListDrawerSettings(Expanded = true, DraggableItems = true)]
        public List<MenuStepRef> steps = new();

        [Title("References")]
        [PreviewField(70)] public Sprite menuSprite;
        
        private float _totalTime;
        private float _totalScore;
    }
}
