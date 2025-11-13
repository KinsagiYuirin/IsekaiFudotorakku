using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.Item
{
    [System.Serializable]
    public class PageSetting
    {
        public Image pageSprite;
        public int pageNumber;
    }
    
    [CreateAssetMenu(fileName = "MangaPage", menuName = "Scriptable Objects/MangaPage")]
    public class MangaPage : ScriptableObject
    {
        [field: SerializeField] public PageSetting[] PageSprites { get; private set; }
    }
}
