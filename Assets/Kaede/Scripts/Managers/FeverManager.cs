using Cysharp.Threading.Tasks;
using Kaede.Art.TA_Works.Scripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Managers
{
    public class FeverManager : MonoBehaviour
    {
        [Title("Setting")] 
        [SerializeField] private float failedDelay;
        
        [SerializeField] private VFXControl vfxControl;
        
        void Start()
        {
        
        }

        private void FeverMode()
        {
            vfxControl.Fevering();
        }

        private void NormalMode()
        {
            vfxControl.Comboing();
        }

        private async UniTask FailMode()
        {
            vfxControl.Failing();
            await UniTask.WaitForSeconds(failedDelay);
            NormalMode();
        }
    }
}
