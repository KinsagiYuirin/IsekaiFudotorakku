using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Audios
{
    internal class SfxManagerDemo : MonoBehaviour
    {
        [Title("Audio Clips")]
        [field: SerializeField] private AudioClip _successClip;
        [field: SerializeField] private AudioClip _failureClip;
        
        [Title("Audio Settings")]
        [SerializeField] private AudioSource _audioSource;
        
        public void PlaySuccessSound()
        {
            if (_audioSource != null && _successClip != null)
            {
                _audioSource.PlayOneShot(_successClip);
            }
        }
        
        public void PlayFailureSound()
        {
            if (_audioSource != null && _failureClip != null)
            {
                _audioSource.PlayOneShot(_failureClip);
            }
        }
    }
}
