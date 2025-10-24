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
        public AnimationClip WrongFeedbackClip { get; }

        public bool HasAnimation => Mode != ComboStepAnimationMode.None;

        private ComboStepAnimationDefinition(ComboStepAnimationMode mode, AnimationClip singleClip,
            IReadOnlyList<AnimationClip> sequentialClips, AnimationClip wrongFeedbackClip)
        {
            Mode                = mode;
            SingleClip          = singleClip;
            SequentialClips     = sequentialClips;
            WrongFeedbackClip   = wrongFeedbackClip;
        }

        public static ComboStepAnimationDefinition FromSingle(AnimationClip clip, AnimationClip wrongFeedbackClip = null)
        {
            return clip != null || wrongFeedbackClip != null
                ? new ComboStepAnimationDefinition(ComboStepAnimationMode.SingleClip, clip, null, wrongFeedbackClip)
                : None;
        }

        public static ComboStepAnimationDefinition FromSequence(IReadOnlyList<AnimationClip> clips, AnimationClip wrongFeedbackClip = null)
        {
            if (clips != null && clips.Count > 0)
            {
                return new ComboStepAnimationDefinition(ComboStepAnimationMode.SequentialClips, null, clips, wrongFeedbackClip);
            }

            return wrongFeedbackClip != null
                ? FromSingle(null, wrongFeedbackClip)
                : None;
        }
    }
}