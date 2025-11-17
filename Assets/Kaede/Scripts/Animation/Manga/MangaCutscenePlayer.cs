using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
            [Serializable]
            public class CutsceneIllustration
            {
                [Tooltip("Image object that will be animated for this illustration.")]
                public Image image;

                [Tooltip("Effect that is applied when this illustration becomes visible.")]
                public CutsceneEffect[] effect;
            }

            [Tooltip("Illustrations that will be shown sequentially for this page.")]
            public List<CutsceneIllustration> illustrations = new();

            public bool playBothPagesAtOnce = false;
            
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
        private AudioSource audioSource;

        [SerializeField]
        [Tooltip("If true, SetNativeSize() is called whenever a new page is shown.")]
        private bool setNativeSizeOnShow = false;

        [SerializeField]
        [Tooltip("If true, a CanvasGroup will be created automatically when an illustration needs one.")]
        private bool createCanvasGroupIfMissing = true;
        
        [Header("Events")]
        public CutscenePageEvent onPageStarted;
        public CutscenePageEvent onPageCompleted;
        public UnityEvent onCutsceneFinished;

        private int currentPageIndex = -1;
        private CancellationTokenSource pageCancelToken;
        private bool waitingForInput;
        private readonly List<Image> activeIllustrations = new();

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
            audioSource = GetComponent<AudioSource>();
        }

        private void Start()
        {
            pageCancelToken = new CancellationTokenSource();
            if (playOnStart)
            {
                Play();
            }
        }
        
        private void OnDestroy()
        {
            StopCurrentRoutine();
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
            PlayPage(page, pageCancelToken.Token).Forget();
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

        private async UniTask PlayPage(CutscenePage page, CancellationToken  token)
        {
            token.ThrowIfCancellationRequested();
            
            HideActiveIllustrations();

            onPageStarted?.Invoke(currentPageIndex, page);

            if (page.sound != null && audioSource != null)
            {
                audioSource.clip = page.sound;
                audioSource.Play();
            }
            
            foreach (var illustration in page.illustrations)
            {
                token.ThrowIfCancellationRequested();
                
                int pageNum = currentPageIndex + 1;
                if (illustration == null || illustration.image == null)
                {
                    continue;
                }

                var image = illustration.image;
                activeIllustrations.Add(image);
                image.gameObject.SetActive(true);
                image.enabled = true;

                if (setNativeSizeOnShow)
                {
                    image.SetNativeSize();
                }

                var canvasGroup = image.GetComponent<CanvasGroup>();
                if (canvasGroup == null && createCanvasGroupIfMissing)
                {
                    canvasGroup = image.gameObject.AddComponent<CanvasGroup>();
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
                var context = new CutsceneEffectContext(
                    this,
                    image.rectTransform,
                    canvasGroup,
                    image);

                if (illustration.effect != null)
                {
                    foreach (var effect in illustration.effect)
                    {
                        if (!page.playBothPagesAtOnce)
                            await effect.Play(context, token);
                        else
                            effect.Play(context, token).Forget();
                    }
                }
                else
                {
                    var color = image.color;
                    color.a = 1f;
                    image.color = color;
                    // Ensure at least a frame passes so layout updates are visible.
                    await UniTask.Yield(token); 
                }

                Debug.Log($"Displayed illustration. {pageNum}");
            }

            onPageCompleted?.Invoke(currentPageIndex, page);

            if (page.autoAdvance)
            {
                if (page.autoAdvanceDelay > 0f)
                {
                    await UniTask.WaitForSeconds(page.autoAdvanceDelay);
                }

                Advance();
            }
            else
            {
                waitingForInput = true;
            }
        }

        private void HideActiveIllustrations()
        {
            if (activeIllustrations.Count == 0)
            {
                return;
            }

            foreach (var image in activeIllustrations)
            {
                if (image != null)
                {
                    image.gameObject.SetActive(false);
                }
            }
            activeIllustrations.Clear();
        }

        private void StopCurrentRoutine()
        {
            if (pageCancelToken != null)
            {
                pageCancelToken.Cancel();
                pageCancelToken.Dispose();
            }
            
            pageCancelToken = new CancellationTokenSource();
        }

        protected virtual void HandleCutsceneFinished()
        {
            onCutsceneFinished?.Invoke();
        }
    }
}