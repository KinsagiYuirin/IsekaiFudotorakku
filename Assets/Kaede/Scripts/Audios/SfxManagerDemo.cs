using System.Collections.Generic;
using Kaede.Scripts.Item;
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
        [SerializeField] private AudioSource _cookingSfxSource;
        
        [SerializeField, DisplayAsString] private List<AudioClip> _audioClips = new();
        private int _currentClipIndex;
        
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
        
        public void ConfigureSequentialSfx(AnimationAndSfx[] animationsAndSfx)
        {
            //_audioClips.Clear();
            _currentClipIndex = 0;

            if (animationsAndSfx != null) return;
            
            foreach (var entry in animationsAndSfx)
            {
                if (entry?.sfxClip != null)
                {
                    _audioClips.Add(entry.sfxClip);
                }
            }
        }
        
        public void ResetSequentialSfxProgress()
        {
            _currentClipIndex = 0;
        }
        
        public void PlaySequentialSfx()
        {
            if (_cookingSfxSource == null || _audioClips.Count == 0) return;
            
            var safeIndex = Mathf.Clamp(_currentClipIndex, 0, _audioClips.Count - 1);
            var clip = _audioClips[safeIndex];
            
            if (clip == null) return; 
            
            _cookingSfxSource.PlayOneShot(clip);

            if (_currentClipIndex + 1 < _audioClips.Count)
            {
                _currentClipIndex++;
            }
        }
    }
}
