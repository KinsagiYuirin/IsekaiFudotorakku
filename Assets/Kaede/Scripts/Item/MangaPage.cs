using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Item
{
    [Flags]
    public enum MangaEffectType
    {
        None,
        SpeedLines,
        ScreenShake,
        ZoomIn,
        ZoomOut
    }
    
    [System.Serializable]
    public class PageSetting
    {
        public Image pageSprite;
        public MangaEffectType[] effectType;
        public AudioClip sfx;
        public float pageDuration = 0f;
    }
    
    [CreateAssetMenu(fileName = "MangaPage", menuName = "Manga/MangaPage")]
    public class MangaPage : ScriptableObject
    {
        public bool haveMoreOnePage = false;
        [field: HideIf("haveMoreOnePage")]
        [field: SerializeField] public PageSetting pageSetting; 
        
        [field: ShowIf("haveMoreOnePage")]
        [field: SerializeField] public PageSetting[] PageSprites { get; private set; }
    }
}
