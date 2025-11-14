using System.Collections;
using UnityEngine;

namespace Kaede.Scripts.Animation.Manga
{
    /// <summary>
    /// Base class for cutscene effects. Create subclasses to provide different animations.
    /// </summary>
    public abstract class CutsceneEffect : ScriptableObject
    {
        /// <summary>
        /// Called when a cutscene page should be animated.
        /// </summary>
        /// <param name="context">References to the player and the active UI elements.</param>
        public abstract IEnumerator Play(CutsceneEffectContext context);
    }

    /// <summary>
    /// Shared context for cutscene effects so new effects can be implemented easily later.
    /// </summary>
    public readonly struct CutsceneEffectContext
    {
        public readonly MangaCutscenePlayer Player;
        public readonly RectTransform RectTransform;
        public readonly CanvasGroup CanvasGroup;
        public readonly UnityEngine.UI.Image Image;

        public CutsceneEffectContext(
            MangaCutscenePlayer player,
            RectTransform rectTransform,
            CanvasGroup canvasGroup,
            UnityEngine.UI.Image image)
        {
            Player = player;
            RectTransform = rectTransform;
            CanvasGroup = canvasGroup;
            Image = image;
        }
    }
}