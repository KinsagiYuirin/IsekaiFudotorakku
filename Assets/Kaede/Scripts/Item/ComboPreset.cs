using System;
using System.Collections.Generic;
using System.Linq;
using Kaede.Scripts.Animation;
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
        D,
        Up,
        Down,
        Left,
        Right
    }

    public enum ComboType
    {
        Single,
        StackTimer,
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
    
    public enum AnimationType
    {
        Loop,
        StepByStep
    }
    
    [Serializable]
    public class AnimationSetting
    {
        public AnimationType type;
        [ShowIf("type", AnimationType.Loop)] 
        public float loopDuration = 1.0f;
    }
    
    [Serializable]
    public class ComboKeySetting
    {
        public ComboKey key;
        public ComboType type;
    
        [ShowIf("type", ComboType.StackTimer)] 
        public float buttonDuration = 0.3f;
        [ShowIf("type", ComboType.StackTimer)]
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
        [LabelText("Name")] public string displayName;
        
        [EnumToggleButtons, LabelText("Phase")]
        public StepPhase phase; // Preparation / Cooking
        
        [LabelText("Image")]
        public Sprite cookingSprite;
        
        [LabelText("Use Sequential Animation")]
        public bool useSequentialAnimation;
        
        [HideIf(nameof(useSequentialAnimation))]
        [LabelText("Animation")]
        public AnimationClip comboAnimation;
        
        [LabelText("Wrong Input Animation")]
        public AnimationClip wrongComboAnimation;
        
        [ShowIf(nameof(useSequentialAnimation))]
        [LabelText("Sequential Animations"), ListDrawerSettings(Expanded = true, DraggableItems = true)]
        public List<AnimationClip> comboStepAnimations = new();
        
        [Title("Sequence")]
        [ListDrawerSettings(
            Expanded = true, DraggableItems = true,
            NumberOfItemsPerPage = 6, ShowPaging = true)]
        public List<ComboKeySetting> comboSequence = new();
        
        public ComboStepAnimationDefinition ResolveAnimationDefinition()
        {
            if (useSequentialAnimation)
            {
                var clips = comboStepAnimations?.Where(clip => clip != null).ToList();
                if (clips != null && clips.Count > 0)
                {
                    return ComboStepAnimationDefinition.FromSequence(clips, wrongComboAnimation);
                }
                if (wrongComboAnimation != null)
                {
                    return ComboStepAnimationDefinition.FromSingle(null, wrongComboAnimation);
                }
            }

            if (comboAnimation != null)
            {
                return ComboStepAnimationDefinition.FromSingle(comboAnimation, wrongComboAnimation);
            }

            return wrongComboAnimation != null
                ? ComboStepAnimationDefinition.FromSingle(null, wrongComboAnimation)
                : ComboStepAnimationDefinition.None;
        }
    }
}