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
        private AnimationClip _definitionWrongFeedbackClip;
        private AnimationClipPlayable _wrongFeedbackPlayable;
        private Coroutine _wrongFeedbackRoutine;
        private double _previousPlayableTime;

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
            StopWrongFeedbackRoutine();
            if (_graph.IsValid())
            {
                _graph.Stop();
            }
        }

        private void OnDestroy()
        {
            StopSequenceRoutine();
            StopWrongFeedbackRoutine();
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
            _definitionWrongFeedbackClip = definition.WrongFeedbackClip;
            StopSequenceRoutine();
            StopWrongFeedbackRoutine();

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
            StopWrongFeedbackRoutine();

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
        
        public bool PlayWrongFeedback()
        {
            if (!_graph.IsValid())
            {
                return false;
            }

            if (_definitionWrongFeedbackClip == null)
            {
                return false;
            }

            StopSequenceRoutine();
            StopWrongFeedbackRoutine();

            _previousPlayableTime = _currentPlayable.IsValid() ? _currentPlayable.GetTime() : 0d;

            if (!_animationOutput.IsOutputValid())
            {
                return false;
            }

            _wrongFeedbackPlayable = AnimationClipPlayable.Create(_graph, _definitionWrongFeedbackClip);
            _wrongFeedbackPlayable.SetApplyFootIK(false);
            _wrongFeedbackPlayable.SetApplyPlayableIK(false);
            _wrongFeedbackPlayable.SetTime(0f);
            _wrongFeedbackPlayable.SetSpeed(1f);

            _animationOutput.SetSourcePlayable(_wrongFeedbackPlayable);
            _wrongFeedbackPlayable.Play();

            if (!_graph.IsPlaying())
            {
                _graph.Play();
            }

            _wrongFeedbackRoutine = StartCoroutine(RestoreAfterWrong(_definitionWrongFeedbackClip.length));
            return true;
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

        private void StopWrongFeedbackRoutine()
        {
            var wasPlayingWrong = _wrongFeedbackRoutine != null || _wrongFeedbackPlayable.IsValid();

            if (_wrongFeedbackRoutine != null)
            {
                StopCoroutine(_wrongFeedbackRoutine);
                _wrongFeedbackRoutine = null;
            }

            if (_wrongFeedbackPlayable.IsValid())
            {
                _wrongFeedbackPlayable.Destroy();
                _wrongFeedbackPlayable = default;
            }

            var outputValid = _animationOutput.IsOutputValid();

            if (wasPlayingWrong && outputValid)
            {
                if (_currentPlayable.IsValid())
                {
                    _animationOutput.SetSourcePlayable(_currentPlayable);
                    ApplyPoseAtTime(_previousPlayableTime);
                }
                else
                {
                    _animationOutput.SetSourcePlayable(Playable.Null);
                }
            }

            _previousPlayableTime = 0d;
        }

        private IEnumerator RestoreAfterWrong(float duration)
        {
            var remaining = Mathf.Max(duration, 0f);
            while (remaining > 0f)
            {
                remaining -= Time.deltaTime;
                yield return null;
            }

            if (_wrongFeedbackPlayable.IsValid())
            {
                _wrongFeedbackPlayable.Pause();
            }

            _wrongFeedbackRoutine = null;

            if (_currentPlayable.IsValid() && _animationOutput.IsOutputValid())
            {
                _animationOutput.SetSourcePlayable(_currentPlayable);
                _currentPlayable.SetTime(_previousPlayableTime);
                _currentPlayable.Pause();
                ApplyPoseAtTime(_previousPlayableTime);
            }
            else if (_animationOutput.IsOutputValid())
            {
                _animationOutput.SetSourcePlayable(Playable.Null);
            }

            if (_wrongFeedbackPlayable.IsValid())
            {
                _wrongFeedbackPlayable.Destroy();
                _wrongFeedbackPlayable = default;
            }

            if (_graph.IsValid() && _graph.IsPlaying())
            {
                _graph.Stop();
            }

            _previousPlayableTime = 0d;
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
            StopWrongFeedbackRoutine();
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