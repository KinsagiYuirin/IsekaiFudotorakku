using System.Threading;
using System.Threading.Tasks;
using Kaede.Scripts.Item;

namespace Kaede.Scripts.Animation.Manga
{
    public class ScreenShakeAction : IMangaAction
    {
        public async Task ExecuteAsync(IManga view, CancellationToken token)
        {
            // เขย่าตามค่า default (ยูรินจังปรับค่าในภายหลังได้)
            await view.ShakeAsync(MangaEffectType.ScreenShake, 10f, 0.3f);
        }
    }

    public class ScreenShakeActionFactory : IMangaActionFactory
    {
        public MangaEffectType Key => MangaEffectType.ScreenShake;
        public IMangaAction Create() => new ScreenShakeAction();
    }
}