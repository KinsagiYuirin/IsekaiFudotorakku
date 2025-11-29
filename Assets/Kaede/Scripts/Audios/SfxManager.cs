using System.Collections.Generic;
using Kaede.Scripts.Item;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Kaede.Scripts.Audios
{
    internal class SfxManager : MonoBehaviour
    {
        [Title("Audio Clips")]
        [field: SerializeField] private AudioClip _successClip;
        [field: SerializeField] private AudioClip _failureClip;
        
        [Title("Audio Settings")]
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioSource _cookingSfxSource;
        
        [SerializeField, DisplayAsString] private List<AudioClip> _sfxClips = new();
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
        
        public void ConfigureSequentialSfx(AnimationAndSfx[] animationAndSfx)
        {
            _sfxClips.Clear();
            _currentClipIndex = 0;

            if (animationAndSfx == null) return;

            foreach (var entry in animationAndSfx)
            {
                if (entry?.sfxClip != null)
                {
                    _sfxClips.Add(entry.sfxClip);
                }
            }
        }
        
        public void ResetSequentialSfxProgress()
        {
            _currentClipIndex = 0;
        }
        
        public void PlaySequentialSfx()
        {
            if (_cookingSfxSource == null || _sfxClips.Count == 0) return;
            
            var safeIndex = Mathf.Clamp(_currentClipIndex, 0, _sfxClips.Count - 1);
            var clip = _sfxClips[safeIndex];
            
            if (clip == null) return; 
            
            _cookingSfxSource.PlayOneShot(clip);

            if (_currentClipIndex + 1 < _sfxClips.Count)
            {
                _currentClipIndex++;
            }
        }
    }
}
