using System.Collections;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kaede.Scripts.Animation
{
    [DisallowMultipleComponent]
    public class ComboCharacterEmotionPlayer : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip idleAnimation;
        [SerializeField] private AnimationClip successAnimation;
        [SerializeField] private AnimationClip failureAnimation;
        [SerializeField] private AnimationClip holdLoopAnimation;
        [SerializeField] private bool playIdleOnEnable = true;

        private PlayableGraph _graph;
        private AnimationPlayableOutput _animationOutput;
        private AnimationClipPlayable _currentPlayable;
        private AnimationClip _currentClip;
        private Coroutine _returnToIdleRoutine;
        private bool _initialized;

        private void Awake()
        {
            animator ??= GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("ComboCharacterEmotionPlayer requires an Animator component to play clips.", this);
                return;
            }

            _graph = PlayableGraph.Create($"{name}_CharacterEmotionGraph");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _animationOutput = AnimationPlayableOutput.Create(_graph, "CharacterEmotion", animator);
            _initialized = true;
        }

        private void OnEnable()
        {
            if (!_initialized)
            {
                return;
            }

            if (playIdleOnEnable)
            {
                PlayIdle();
            }
        }

        private void OnDisable()
        {
            if (_graph.IsValid())
            {
                _graph.Stop();
            }

            if (_returnToIdleRoutine != null)
            {
                StopCoroutine(_returnToIdleRoutine);
                _returnToIdleRoutine = null;
            }
        }

        private void OnDestroy()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

        public void PlayIdle()
        {
            PlayClip(idleAnimation, true, false);
        }

        public void PlaySuccess()
        {
            if (!PlayClip(successAnimation != null ? successAnimation : idleAnimation, false, true) && successAnimation == null)
            {
                // Fallback to idle when there is no success animation clip.
                PlayIdle();
            }
        }

        public void PlayFailure()
        {
            if (!PlayClip(failureAnimation != null ? failureAnimation : idleAnimation, false, true) && failureAnimation == null)
            {
                // Fallback to idle when there is no failure animation clip.
                PlayIdle();
            }
        }

        public void PlayHoldLoop()
        {
            if (!PlayClip(holdLoopAnimation != null ? holdLoopAnimation : idleAnimation, true, false) && holdLoopAnimation == null)
            {
                PlayIdle();
            }
        }

        public void ResetToIdle()
        {
            if (_returnToIdleRoutine != null)
            {
                StopCoroutine(_returnToIdleRoutine);
                _returnToIdleRoutine = null;
            }

            PlayIdle();
        }

        private bool PlayClip(AnimationClip clip, bool loop, bool forceRestart)
        {
            if (!_graph.IsValid() || !_initialized)
            {
                return false;
            }

            if (clip == null)
            {
                StopCurrentPlayable();
                if (_graph.IsPlaying())
                {
                    _graph.Stop();
                }
                return false;
            }

            if (!forceRestart && _currentPlayable.IsValid() && _currentClip == clip)
            {
                if (!_graph.IsPlaying())
                {
                    _graph.Play();
                }

                return true;
            }

            ReplaceCurrentPlayable(clip, loop);
            _currentClip = clip;

            _currentPlayable.SetTime(0f);
            _currentPlayable.SetSpeed(1f);

            if (!_graph.IsPlaying())
            {
                _graph.Play();
            }

            if (!loop)
            {
                ScheduleReturnToIdle(clip.length);
            }
            else if (_returnToIdleRoutine != null)
            {
                StopCoroutine(_returnToIdleRoutine);
                _returnToIdleRoutine = null;
            }

            return true;
        }

        private void ReplaceCurrentPlayable(AnimationClip clip, bool loop)
        {
            StopCurrentPlayable();

            _currentPlayable = AnimationClipPlayable.Create(_graph, clip);
            _currentPlayable.SetApplyFootIK(false);
            _currentPlayable.SetApplyPlayableIK(false);

            if (!loop)
            {
                _currentPlayable.SetDuration(clip.length);
            }

            _animationOutput.SetSourcePlayable(_currentPlayable);
        }

        private void StopCurrentPlayable()
        {
            if (_currentPlayable.IsValid())
            {
                _currentPlayable.Destroy();
                _currentPlayable = default;
            }

            _currentClip = null;
        }

        private void ScheduleReturnToIdle(float delay)
        {
            if (delay <= 0f || idleAnimation == null)
            {
                return;
            }

            if (_returnToIdleRoutine != null)
            {
                StopCoroutine(_returnToIdleRoutine);
            }

            _returnToIdleRoutine = StartCoroutine(ReturnToIdleAfter(delay));
        }

        private IEnumerator ReturnToIdleAfter(float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayIdle();
            _returnToIdleRoutine = null;
        }
    }
}
