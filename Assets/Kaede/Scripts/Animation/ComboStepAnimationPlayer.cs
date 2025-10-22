using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kaede.Scripts.Animation
{
    [DisallowMultipleComponent]
    public class ComboStepAnimationPlayer : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private bool playOnAssign;
        [SerializeField] private bool loopAnimation = false;

        private PlayableGraph _graph;
        private AnimationPlayableOutput _animationOutput;
        private AnimationClipPlayable _currentPlayable;
        private AnimationClip _currentClip;

        private ComboStepAnimationDefinition _currentDefinition;
        private List<AnimationClip> _sequenceClips;
        private int _sequenceIndex;
        private Coroutine _sequenceRoutine;
        private bool _isSequencePlaying;

        private bool IsSequentialMode => _currentDefinition.Mode == ComboStepAnimationMode.SequentialClips;

        private void Awake()
        {
            animator ??= GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("ComboStepAnimationPlayer requires an Animator component to play clips.", this);
                return;
            }
            _graph = PlayableGraph.Create($"{name}_ComboStepAnimationGraph");
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _animationOutput = AnimationPlayableOutput.Create(_graph, "ComboStepAnimation", animator);
        }

        private void OnDisable()
        {
            StopSequenceRoutine();
            if (_graph.IsValid())
            {
                _graph.Stop();
            }
        }

        private void OnDestroy()
        {
            StopSequenceRoutine();
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

        public void SetAnimation(AnimationClip clip)
        {
            SetAnimation(ComboStepAnimationDefinition.FromSingle(clip), playOnAssign);
        }

        public void SetAnimation(AnimationClip clip, bool autoPlay)
        {
            SetAnimation(ComboStepAnimationDefinition.FromSingle(clip), autoPlay);
        }

        public void SetAnimation(ComboStepAnimationDefinition definition)
        {
            SetAnimation(definition, playOnAssign);
        }

        public void SetAnimation(ComboStepAnimationDefinition definition, bool autoPlay)
        {
            if (!_graph.IsValid())
            {
                return;
            }

            _currentDefinition = definition;
            StopSequenceRoutine();

            switch (definition.Mode)
            {
                case ComboStepAnimationMode.SequentialClips:
                    InitializeSequence(definition.SequentialClips);
                    if (autoPlay)
                    {
                        PlayNextSequenceClip();
                    }
                    break;
                case ComboStepAnimationMode.SingleClip:
                    InitializeSingle(definition.SingleClip, autoPlay);
                    break;
                default:
                    ClearCurrentPlayable();
                    break;
            }
        }

        public void Play()
        {
            if (!_graph.IsValid())
            {
                return;
            }

            if (IsSequentialMode)
            {
                PlayNextSequenceClip();
            }
            else
            {
                PlaySingleClip();
            }
        }

        public void Stop()
        {
            StopSequenceRoutine();

            if (_graph.IsValid() && _graph.IsPlaying())
            {
                _graph.Stop();
            }

            if (IsSequentialMode)
            {
                ResetSequenceToStart();
            }
            else if (_currentPlayable.IsValid())
            {
                _currentPlayable.SetTime(0f);
                _currentPlayable.Pause();
                ApplyPoseAtTime(0f);
            }
        }

        public void ClearAnimation()
        {
            SetAnimation(ComboStepAnimationDefinition.None, false);
        }

        private void InitializeSingle(AnimationClip clip, bool autoPlay)
        {
            _sequenceClips = null;
            _sequenceIndex = 0;

            ReplaceCurrentPlayable(clip);

            if (clip == null)
            {
                if (_graph.IsValid())
                {
                    _graph.Stop();
                }
                return;
            }

            if (autoPlay)
            {
                PlaySingleClip();
            }
            else
            {
                ApplyPoseAtTime(0f);
            }
        }

        private void InitializeSequence(IReadOnlyList<AnimationClip> clips)
        {
            _sequenceClips = new List<AnimationClip>();
            if (clips != null)
            {
                foreach (var clip in clips)
                {
                    if (clip != null)
                    {
                        _sequenceClips.Add(clip);
                    }
                }
            }

            _sequenceIndex = 0;

            if (_sequenceClips.Count > 0)
            {
                ReplaceCurrentPlayable(_sequenceClips[0]);
                ApplyPoseAtTime(0f);
            }
            else
            {
                ClearCurrentPlayable();
            }
        }

        private void ResetSequenceToStart()
        {
            if (_sequenceClips == null || _sequenceClips.Count == 0)
            {
                ClearCurrentPlayable();
                return;
            }

            _sequenceIndex = 0;
            ReplaceCurrentPlayable(_sequenceClips[0]);
            ApplyPoseAtTime(0f);
        }

        private void PlaySingleClip()
        {
            if (!_currentPlayable.IsValid())
            {
                return;
            }

            _currentPlayable.SetTime(0f);
            _currentPlayable.SetSpeed(1f);
            _currentPlayable.Play();

            if (!_graph.IsPlaying())
            {
                _graph.Play();
            }
        }

        private void PlayNextSequenceClip()
        {
            if (_sequenceClips == null || _sequenceClips.Count == 0)
            {
                return;
            }

            if (_sequenceIndex >= _sequenceClips.Count)
            {
                return;
            }

            if (_isSequencePlaying)
            {
                return;
            }

            var clip = _sequenceClips[_sequenceIndex];
            _sequenceIndex++;

            if (clip == null)
            {
                PlayNextSequenceClip();
                return;
            }

            ReplaceCurrentPlayable(clip);
            _currentPlayable.SetTime(0f);
            _currentPlayable.SetSpeed(1f);
            _currentPlayable.Play();

            if (!_graph.IsPlaying())
            {
                _graph.Play();
            }

            StopSequenceRoutine();
            _sequenceRoutine = StartCoroutine(PauseAtEnd(clip.length));
            _isSequencePlaying = true;
        }

        private IEnumerator PauseAtEnd(float duration)
        {
            var remaining = Mathf.Max(duration, 0f);
            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                yield return null;
            }

            if (_currentPlayable.IsValid())
            {
                var finalTime = _currentClip != null ? _currentClip.length : 0f;
                _currentPlayable.SetTime(finalTime);
                _currentPlayable.Pause();
            }

            if (_graph.IsValid() && _graph.IsPlaying())
            {
                _graph.Stop();
            }

            _sequenceRoutine = null;
            _isSequencePlaying = false;
        }

        private void StopSequenceRoutine()
        {
            if (_sequenceRoutine != null)
            {
                StopCoroutine(_sequenceRoutine);
                _sequenceRoutine = null;
            }

            _isSequencePlaying = false;
        }

        private void ApplyPoseAtTime(double time)
        {
            if (!_currentPlayable.IsValid())
            {
                return;
            }

            _currentPlayable.SetTime(time);
            _currentPlayable.Pause();
            if (_graph.IsValid() && _graph.IsPlaying())
            {
                _graph.Stop();
            }
            if (_graph.IsValid())
            {
                _graph.Evaluate(0f);
            }
        }

        private void ClearCurrentPlayable()
        {
            if (_currentPlayable.IsValid())
            {
                _currentPlayable.Destroy();
                _currentPlayable = default;
            }

            _currentClip = null;
            _sequenceClips = null;
            _sequenceIndex = 0;
            _animationOutput.SetSourcePlayable(Playable.Null);
        }

        private void ReplaceCurrentPlayable(AnimationClip clip)
        {
            if (_currentPlayable.IsValid())
            {
                _currentPlayable.Destroy();
                _currentPlayable = default;
            }

            _currentClip = clip;

            if (clip == null)
            {
                _animationOutput.SetSourcePlayable(Playable.Null);
                return;
            }

            _currentPlayable = AnimationClipPlayable.Create(_graph, clip);
            _currentPlayable.SetApplyFootIK(false);
            _currentPlayable.SetApplyPlayableIK(false);

            var shouldLoop = !IsSequentialMode && loopAnimation;
            if (!shouldLoop)
            {
                _currentPlayable.SetDuration(clip.length);
            }

            _animationOutput.SetSourcePlayable(_currentPlayable);
        }
    }
}