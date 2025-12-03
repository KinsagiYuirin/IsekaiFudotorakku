using System;
using UnityEngine;

namespace Kaede.Scripts.Audios
{
    public class BgmManager : MonoBehaviour
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioClip firstBgm;
        [SerializeField] private AudioClip secondBgm;

        private void Start()
        {
            PlayFirstBgm();
        }

        public void PlayFirstBgm()
        {
            if (bgmSource != null && firstBgm != null)
            {
                bgmSource.clip = firstBgm;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }

        public void PlaySecondBgm()
        {
            if (bgmSource != null && secondBgm != null)
            {
                bgmSource.clip = secondBgm;
                bgmSource.loop = true;
                bgmSource.Play();
            }
        }
    }
}
