using System.Threading;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.GamePlay;
using Kaede.Scripts.Item;
using MadDuck.Scripts.Inputs;

namespace Kaede.Scripts.Inputs.ComboHandlers
{
    public interface IComboHandler
    {
        /// <summary>
        /// ตรวจสอบว่า input ปัจจุบันผ่านเงื่อนไขของ combo type หรือไม่
        /// </summary>
        ComboInputResult CheckInput(PlayerInputHandler input, ComboKey expectedKey, CancellationToken ct);
    }
}