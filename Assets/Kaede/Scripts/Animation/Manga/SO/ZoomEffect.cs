using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Kaede.Scripts.Animation.Manga.SO
{
    [CreateAssetMenu(fileName = "ZoomEffect", menuName = "Cutscenes/Effects/Zoom")]
    public class ZoomEffect : CutsceneEffect
    {
        public override UniTask Play(CutsceneEffectContext contex, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
