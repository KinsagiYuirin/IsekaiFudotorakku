using System.Collections.Generic;
using UnityEngine;

namespace Kaede.Scripts.Animation
{
    public enum ComboStepAnimationMode
    {
        None,
        SingleClip,
        SequentialClips
    }

    public readonly struct ComboStepAnimationDefinition
    {
        public static ComboStepAnimationDefinition None => default;

        public ComboStepAnimationMode Mode { get; }
        public AnimationClip SingleClip { get; }
        public IReadOnlyList<AnimationClip> SequentialClips { get; }

        public bool HasAnimation => Mode != ComboStepAnimationMode.None;

        private ComboStepAnimationDefinition(ComboStepAnimationMode mode, AnimationClip singleClip,
            IReadOnlyList<AnimationClip> sequentialClips)
        {
            Mode             = mode;
            SingleClip       = singleClip;
            SequentialClips  = sequentialClips;
        }

        public static ComboStepAnimationDefinition FromSingle(AnimationClip clip)
        {
            return clip != null
                ? new ComboStepAnimationDefinition(ComboStepAnimationMode.SingleClip, clip, null)
                : None;
        }

        public static ComboStepAnimationDefinition FromSequence(IReadOnlyList<AnimationClip> clips)
        {
            return clips != null && clips.Count > 0
                ? new ComboStepAnimationDefinition(ComboStepAnimationMode.SequentialClips, null, clips)
                : None;
        }
    }
}