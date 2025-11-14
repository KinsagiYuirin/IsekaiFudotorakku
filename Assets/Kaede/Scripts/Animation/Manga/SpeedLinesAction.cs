using System.Threading;
using System.Threading.Tasks;
using Kaede.Scripts.Item;

namespace Kaede.Scripts.Animation.Manga
{
    public class SpeedLinesAction : IMangaAction
    {
        public async Task ExecuteAsync(IManga view, CancellationToken token)
        {
            // ตัวอย่าง: ให้ SpeedLines โผล่ขึ้นมาด้วย fade 0 → 1 ภายใน 0.25 วิ
            await view.FadeAsync(MangaEffectType.SpeedLines, 0f, 1f, 0.25f);
        }
    }

    public class SpeedLinesActionFactory : IMangaActionFactory
    {
        public MangaEffectType Key => MangaEffectType.SpeedLines;
        public IMangaAction Create() => new SpeedLinesAction();
    }
}