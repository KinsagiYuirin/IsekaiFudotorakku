using System.Threading;
using System.Threading.Tasks;
using Kaede.Scripts.Item;
using UnityEngine;

namespace Kaede.Scripts.Animation.Manga
{
    public interface IManga
    {
        Transform GetSlot(MangaEffectType slotKey);
        Task SetImageAsync(MangaEffectType slotKey, Sprite sprite);
        Task FadeAsync(MangaEffectType slotKey, float from, float to, float duration);
        Task ShakeAsync(MangaEffectType slotKey, float intensity, float time);
        Task PlaySfxAsync(AudioClip clip, float volume = 1f);
    }
    
    public interface IMangaAction 
    {
        Task ExecuteAsync(IManga view, CancellationToken token);
    }

    public interface IMangaActionFactory 
    {
        MangaEffectType Key { get; } // ex. "fadeInImage"
        IMangaAction Create();
    }

    public interface IMangaActionRegistry
    {
        void Register(IMangaActionFactory factory);
        bool TryCreate(MangaEffectType key, out IMangaAction action);
    }
}
