using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Item
{
    public enum MangaEffectType
    {
        None,
        SpeedLines,
        ImpactBurst,
        ScreenShake,
        ZoomIn,
        ZoomOut
    }
    
    [System.Serializable]
    public class PageSetting
    {
        public Image pageSprite;
        public int pageNumber;
        public MangaEffectType effectType;
    }
    
    [CreateAssetMenu(fileName = "MangaPage", menuName = "Scriptable Objects/MangaPage")]
    public class MangaPage : ScriptableObject
    {
        [field: SerializeField] public PageSetting[] PageSprites { get; private set; }
    }
}
