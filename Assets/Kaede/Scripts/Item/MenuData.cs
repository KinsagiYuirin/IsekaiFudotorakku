using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Item
{
    [System.Serializable]
    public class MenuStepRef
    {
        [HorizontalGroup("Row", 0.6f)]
        [HideLabel, AssetsOnly, Required]
        public ComboPreset Preset;

        [HorizontalGroup("Row", 0.4f)]
        [HideLabel, LabelText("Override Sequence?")]
        public bool OverrideSequence;

        [ShowIf(nameof(OverrideSequence))]
        [TableList(ShowIndexLabels = true)]
        public List<ComboKeySetting> CustomSequence = new();

        public List<ComboKeySetting> ResolveSequence()
            => OverrideSequence && CustomSequence != null && CustomSequence.Count > 0
                ? CustomSequence
                : Preset != null ? Preset.ComboSequence : new List<ComboKeySetting>();
    }

    [CreateAssetMenu(fileName = "MenuData", menuName = "Kaede/MenuData")]
    public class MenuData : ScriptableObject
    {
        [Title("Settings")]
        [LabelText("Menu Name")] public string MenuName;
        public MenuLevel MenuLevel;

        [Title("Steps (เลือกพรีเซ็ต)")]
        [ListDrawerSettings(Expanded = true, DraggableItems = true)]
        public List<MenuStepRef> Steps = new();

        [Title("References")]
        [PreviewField(70)] public Sprite MenuIcon;
    }
}
