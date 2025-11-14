using System.Threading;
using System.Threading.Tasks;
using Kaede.Scripts.Item;
using UnityEngine;

namespace Kaede.Scripts.Animation.Manga
{
    // ใน MangaSystem.cs (ตัวอย่าง logic คร่าว ๆ)

public class MangaSystem : MonoBehaviour
{
    [SerializeField] private MangaPage[] mangaPages;
    [SerializeField] private MonoBehaviour mangaViewBehaviour; // ตัวที่ implement IManga

    private IManga _view;
    private MangaActionRegistry _registry;
    private CancellationTokenSource _cts;

    private void Awake()
    {
        _view = (IManga)mangaViewBehaviour;

        _registry = new MangaActionRegistry();
        // ถ้าอยากเพิ่ม effect type ใหม่ทีหลังก็ทำ _registry.Register(new XxxFactory()); เพิ่มได้
    }

    private async void Start()
    {
        // ตัวอย่าง: เล่นหน้าแรกของ mangaPages[0]
        if (mangaPages.Length > 0)
        {
            await PlayMangaPage(mangaPages[0]);
        }
    }

    private async Task PlayMangaPage(MangaPage page)
    {
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        // กรณีมีหลายหน้า
        if (page.haveMoreOnePage && page.PageSprites != null)
        {
            foreach (var setting in page.PageSprites)
            {
                await PlaySinglePage(setting, token);
            }
        }
        else
        {
            await PlaySinglePage(page.pageSetting, token);
        }
    }

    private async Task PlaySinglePage(PageSetting setting, CancellationToken token)
    {
        if (setting == null) return;

        // เซ็ตภาพหลักก่อน (สมมุติว่า IManga มี slotKey สำหรับภาพหลักเป็น MangaEffectType.None หรือจะเพิ่ม enum ใหม่ก็ได้)
        if (setting.pageSprite != null)
        {
            await _view.SetImageAsync(MangaEffectType.None, setting.pageSprite.sprite);
        }

        if (setting.sfx != null)
        {
            await _view.PlaySfxAsync(setting.sfx);
        }

        // รัน effect ตามที่ระบุใน effectType[]
        if (setting.effectType != null)
        {
            foreach (var effect in setting.effectType)
            {
                if (_registry.TryCreate(effect, out var action))
                {
                    await action.ExecuteAsync(_view, token);
                }
            }
        }

        // รอ pageDuration ถ้ามี
        if (setting.pageDuration > 0f)
        {
            float t = 0f;
            while (t < setting.pageDuration)
            {
                token.ThrowIfCancellationRequested();
                t += Time.deltaTime;
                await Task.Yield();
            }
        }
        else
        {
            // หรือจะรอให้ผู้เล่นคลิกไปหน้าถัดไปก็ได้
        }
    }
}

}
