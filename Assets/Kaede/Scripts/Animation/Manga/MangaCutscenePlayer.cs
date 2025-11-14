using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Kaede.Scripts.Animation.Manga
{
    /// <summary>
    /// Plays a sequence of manga style cutscene pages.
    /// </summary>
    public class MangaCutscenePlayer : MonoBehaviour
    {
        [Serializable]
        public class CutscenePage
        {
            [Tooltip("Image that should be shown on this page.")]
            public Sprite illustration;

            [Tooltip("Effect that is applied when the page becomes visible.")]
            public CutsceneEffect effect;

            [Tooltip("If true, the cutscene will advance after the delay without user input.")]
            public bool autoAdvance = true;

            [Tooltip("Time in seconds to wait before advancing automatically.")]
            public float autoAdvanceDelay = 2f;

            [Tooltip("Sound effect or voice line that plays when this page is shown.")]
            public AudioClip sound;
        }

        [Serializable]
        public class CutscenePageEvent : UnityEvent<int, CutscenePage> { }

        [Header("Playback")]
        [SerializeField]
        private bool playOnStart = true;

        [SerializeField]
        private List<CutscenePage> pages = new();

        [SerializeField]
        private Image targetImage;

        [SerializeField]
        private CanvasGroup canvasGroup;

        [SerializeField]
        private AudioSource audioSource;

        [SerializeField]
        [Tooltip("If true, SetNativeSize() is called whenever a new page is shown.")]
        private bool setNativeSizeOnShow = false;

        [Header("Events")]
        public CutscenePageEvent onPageStarted;
        public CutscenePageEvent onPageCompleted;
        public UnityEvent onCutsceneFinished;

        private int currentPageIndex = -1;
        private Coroutine pageRoutine;
        private bool waitingForInput;

        /// <summary>
        /// Exposes the configured pages in a read-only manner.
        /// </summary>
        public IReadOnlyList<CutscenePage> Pages => pages;

        /// <summary>
        /// Index of the currently visible page. Returns -1 when the cutscene has not started.
        /// </summary>
        public int CurrentPageIndex => currentPageIndex;

        /// <summary>
        /// Indicates whether the player is waiting for manual input to advance.
        /// </summary>
        public bool IsWaitingForInput => waitingForInput;

        private void Reset()
        {
            targetImage = GetComponentInChildren<Image>();
            canvasGroup = GetComponentInChildren<CanvasGroup>();
            audioSource = GetComponent<AudioSource>();
        }

        private void Awake()
        {
            if (canvasGroup == null && targetImage != null)
            {
                canvasGroup = targetImage.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = targetImage.gameObject.AddComponent<CanvasGroup>();
                }
            }
        }

        private void Start()
        {
            if (playOnStart)
            {
                Play();
            }
        }

        /// <summary>
        /// Begin playing the cutscene from the first page.
        /// </summary>
        public void Play()
        {
            StopCurrentRoutine();
            currentPageIndex = -1;
            Advance();
        }

        /// <summary>
        /// Jump directly to a particular page.
        /// </summary>
        public void JumpTo(int index)
        {
            if (index < 0 || index >= pages.Count)
            {
                Debug.LogWarning($"Cutscene index {index} is out of range.");
                return;
            }

            StopCurrentRoutine();
            currentPageIndex = index - 1;
            Advance();
        }

        /// <summary>
        /// Trigger playback of the next page. This can be bound to a UI button.
        /// </summary>
        public void Advance()
        {
            if (waitingForInput)
            {
                waitingForInput = false;
            }

            currentPageIndex++;
            if (currentPageIndex >= pages.Count)
            {
                HandleCutsceneFinished();
                return;
            }

            var page = pages[currentPageIndex];
            pageRoutine = StartCoroutine(PlayPage(page));
        }

        /// <summary>
        /// Stops playback entirely.
        /// </summary>
        public void Stop()
        {
            StopCurrentRoutine();
            currentPageIndex = pages.Count;
            HandleCutsceneFinished();
        }

        /// <summary>
        /// Allows external code to cancel automatic progression when it is waiting for input.
        /// </summary>
        public void CancelWaitingForInput()
        {
            waitingForInput = false;
        }

        /// <summary>
        /// Adds a new page at runtime. This supports extending the cutscene via scripts.
        /// </summary>
        public void AddPage(CutscenePage page)
        {
            pages.Add(page);
        }

        private IEnumerator PlayPage(CutscenePage page)
        {
            ApplyPageVisuals(page);

            onPageStarted?.Invoke(currentPageIndex, page);

            if (page.sound != null && audioSource != null)
            {
                audioSource.clip = page.sound;
                audioSource.Play();
            }

            if (page.effect != null)
            {
                var context = new CutsceneEffectContext(
                    this,
                    targetImage != null ? targetImage.rectTransform : null,
                    canvasGroup,
                    targetImage);

                yield return page.effect.Play(context);
            }

            onPageCompleted?.Invoke(currentPageIndex, page);

            if (page.autoAdvance)
            {
                if (page.autoAdvanceDelay > 0f)
                {
                    yield return new WaitForSeconds(page.autoAdvanceDelay);
                }

                Advance();
            }
            else
            {
                waitingForInput = true;
            }
        }

        private void ApplyPageVisuals(CutscenePage page)
        {
            if (targetImage != null)
            {
                targetImage.sprite = page.illustration;
                if (setNativeSizeOnShow)
                {
                    targetImage.SetNativeSize();
                }
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
        }

        private void StopCurrentRoutine()
        {
            if (pageRoutine != null)
            {
                StopCoroutine(pageRoutine);
                pageRoutine = null;
            }
        }

        protected virtual void HandleCutsceneFinished()
        {
            onCutsceneFinished?.Invoke();
        }
    }
}