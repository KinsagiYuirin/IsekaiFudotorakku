using System.Collections.Generic;
using Kaede.Scripts.Item;
using UnityEngine;

namespace Kaede.Scripts.Animation.Manga
{
    public class MangaActionRegistry : IMangaActionRegistry
    {
        private readonly Dictionary<MangaEffectType, IMangaActionFactory> _map =
            new Dictionary<MangaEffectType, IMangaActionFactory>();
        
        public MangaActionRegistry()
        {
            // ลงทะเบียน default effect ทั้งหมดตรงนี้ได้เลย
            Register(new SpeedLinesActionFactory());
            Register(new ScreenShakeActionFactory());
            Register(new ZoomInActionFactory());
            Register(new ZoomOutActionFactory());
        }
        
        public void Register(IMangaActionFactory factory)
        {
            if (factory == null)
            {
                Debug.LogWarning("MangaActionRegistry: factory is null");
                return;
            }

            _map[factory.Key] = factory;
        }
        
        public bool TryCreate(MangaEffectType key, out IMangaAction action)
        {
            if (_map.TryGetValue(key, out var factory))
            {
                action = factory.Create();
                return true;
            }

            Debug.LogWarning($"MangaActionRegistry: no action registered for key: {key}");
            action = null;
            return false;
        }
    }
}
