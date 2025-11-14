using System.Threading;
using System.Threading.Tasks;
using Kaede.Scripts.Item;

namespace Kaede.Scripts.Animation.Manga
{
    public class ZoomInAction : IMangaAction
    {
        public async Task ExecuteAsync(IManga view, CancellationToken token)
        {
            // ตัวอย่าง: ใช้ Fade เบา ๆ แทนการซูม ถ้ายังไม่มีระบบซูม
            await view.FadeAsync(MangaEffectType.ZoomIn, 0f, 1f, 0.2f);
        }
    }

    public class ZoomInActionFactory : IMangaActionFactory
    {
        public MangaEffectType Key => MangaEffectType.ZoomIn;
        public IMangaAction Create() => new ZoomInAction();
    }

    public class ZoomOutAction : IMangaAction
    {
        public async Task ExecuteAsync(IManga view, CancellationToken token)
        {
            await view.FadeAsync(MangaEffectType.ZoomOut, 1f, 0f, 0.2f);
        }
    }

    public class ZoomOutActionFactory : IMangaActionFactory
    {
        public MangaEffectType Key => MangaEffectType.ZoomOut;
        public IMangaAction Create() => new ZoomOutAction();
    }
}