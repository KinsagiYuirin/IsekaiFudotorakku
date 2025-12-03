using System;
using UnityEngine;
using System.Collections;

namespace Kaede.Scripts.Audios
{
    public class BgmManager : MonoBehaviour
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioClip firstBgm;
        [SerializeField] private AudioClip secondBgm;
        
        [Header("Transition Settings")]
        [SerializeField] private float fadeDuration = 1.5f;

        private Coroutine transitionRoutine;

        private void Start()
        {
            PlayFirstBgm();
        }

        public void PlayFirstBgm()
        {
            PlayWithTransition(firstBgm);
        }

        public void PlaySecondBgm()
        {
            PlayWithTransition(secondBgm);
        }

        public void PlayWithTransition(AudioClip newClip)
        {
            if (transitionRoutine != null)
                StopCoroutine(transitionRoutine);

            transitionRoutine = StartCoroutine(TransitionBgm(newClip));
        }

        private IEnumerator TransitionBgm(AudioClip newClip)
        {
            if (bgmSource == null || newClip == null)
                yield break;

            float startVolume = bgmSource.volume;

            // Fade Out
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
                yield return null;
            }

            // Switch Clip
            bgmSource.clip = newClip;
            bgmSource.Play();

            // Fade In
            t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                bgmSource.volume = Mathf.Lerp(0f, startVolume, t / fadeDuration);
                yield return null;
            }

            bgmSource.volume = startVolume;
        }
    }
}