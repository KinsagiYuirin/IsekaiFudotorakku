using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
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
        private CancellationTokenSource _sequenceCancellation;
        private bool _isSequencePlaying;
        private AnimationClip _definitionWrongFeedbackClip;
        private AnimationClipPlayable _wrongFeedbackPlayable;
        private CancellationTokenSource _wrongFeedbackCancellation;
        private double _previousPlayableTime;
        private ComboStepAnimationDefinition _pendingDefinition;
        private bool _hasPendingDefinition;

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

        public void SetPlayOnAssign(bool shouldPlay)
        {
            playOnAssign = shouldPlay;
        }

        public void SetAnimation(ComboStepAnimationDefinition definition, bool autoPlay)
        {
            if (!_graph.IsValid())
            {
                return;
            }

            _currentDefinition = definition;
            _definitionWrongFeedbackClip = definition.WrongFeedbackClip;
            ClearPendingDefinition();
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

            _wrongFeedbackCancellation = new CancellationTokenSource();
            RestoreAfterWrongAsync(_definitionWrongFeedbackClip.length, _wrongFeedbackCancellation).Forget();
            return true;
        }


        private void InitializeSingle(AnimationClip clip, bool autoPlay)
        {
            _sequenceClips = null;
            _sequenceIndex = 0;

            if (!autoPlay && clip != null)
            {
                QueuePendingDefinition();
                return;
            }

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
                ApplyPoseAtTime(0f, false);
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
            if (_hasPendingDefinition)
            {
                ApplyPendingDefinition();
            }

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
            _sequenceCancellation = new CancellationTokenSource();
            _isSequencePlaying = true;
            PauseAtEndAsync(clip.length, _sequenceCancellation).Forget();
        }

        private void StopSequenceRoutine()
        {
            if (_sequenceCancellation != null)
            {
                _sequenceCancellation.Cancel();
                _sequenceCancellation.Dispose();
                _sequenceCancellation = null;
            }

            _isSequencePlaying = false;
        }

        private void StopWrongFeedbackRoutine()
        {
            var wasPlayingWrong = _wrongFeedbackCancellation != null || _wrongFeedbackPlayable.IsValid();

            if (_wrongFeedbackCancellation != null)
            {
                _wrongFeedbackCancellation.Cancel();
                _wrongFeedbackCancellation.Dispose();
                _wrongFeedbackCancellation = null;
            }

            if (_wrongFeedbackPlayable.IsValid())
            {
                _wrongFeedbackPlayable.Destroy();
                _wrongFeedbackPlayable = default;
            }

            var outputValid = _animationOutput.IsOutputValid();

            if (wasPlayingWrong && outputValid)
            {
                if (_hasPendingDefinition)
                {
                    _animationOutput.SetSourcePlayable(Playable.Null);
                }
                else if (_currentPlayable.IsValid())
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

        private async UniTask PauseAtEndAsync(float duration, CancellationTokenSource cancellation)
        {
            try
            {
                var seconds = Mathf.Max(duration, 0f);
                if (seconds > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.DeltaTime, PlayerLoopTiming.Update, cancellation.Token);
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
            }
            catch (OperationCanceledException)
            {
                // Routine cancelled; nothing to do.
            }
            finally
            {
                if (_sequenceCancellation == cancellation)
                {
                    _sequenceCancellation.Dispose();
                    _sequenceCancellation = null;
                    _isSequencePlaying = false;
                }
                else
                {
                    cancellation.Dispose();
                }
            }
        }

        private async UniTask RestoreAfterWrongAsync(float duration, CancellationTokenSource cancellation)
        {
            try
            {
                var seconds = Mathf.Max(duration, 0f);
                if (seconds > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(seconds), DelayType.DeltaTime, PlayerLoopTiming.Update, cancellation.Token);
                }

                if (_wrongFeedbackPlayable.IsValid())
                {
                    _wrongFeedbackPlayable.Pause();
                }

                if (_currentPlayable.IsValid() && _animationOutput.IsOutputValid())
                {
                    if (_hasPendingDefinition)
                    {
                        _animationOutput.SetSourcePlayable(Playable.Null);
                    }
                    else
                    {
                        _animationOutput.SetSourcePlayable(_currentPlayable);
                        _currentPlayable.SetTime(_previousPlayableTime);
                        _currentPlayable.Pause();
                        ApplyPoseAtTime(_previousPlayableTime);
                    }
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
            catch (OperationCanceledException)
            {
                // Routine cancelled; nothing to do.
            }
            finally
            {
                if (_wrongFeedbackCancellation == cancellation)
                {
                    _wrongFeedbackCancellation.Dispose();
                    _wrongFeedbackCancellation = null;
                }
                else
                {
                    cancellation.Dispose();
                }
            }
        }

        private void ApplyPoseAtTime(double time, bool stopGraph = true)
        {
            if (!_currentPlayable.IsValid())
            {
                return;
            }

            _currentPlayable.SetTime(time);
            _currentPlayable.Pause();
            if (stopGraph && _graph.IsValid() && _graph.IsPlaying())
            {
                _graph.Stop();
            }
            if (_graph.IsValid() && (stopGraph || !_graph.IsPlaying()))
            {
                _graph.Evaluate(0f);
            }
        }

        private void ClearCurrentPlayable()
        {
            StopWrongFeedbackRoutine();
            ClearPendingDefinition();
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

        private void QueuePendingDefinition()
        {
            _hasPendingDefinition = true;
            _pendingDefinition    = _currentDefinition;
        }

        private void ApplyPendingDefinition()
        {
            if (!_hasPendingDefinition)
            {
                return;
            }

            _hasPendingDefinition = false;
            ReplaceCurrentPlayable(_pendingDefinition.SingleClip);
        }

        private void ClearPendingDefinition()
        {
            _hasPendingDefinition = false;
            _pendingDefinition    = ComboStepAnimationDefinition.None;
        }
    }
}