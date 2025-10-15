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
            if (_graph.IsValid())
            {
                _graph.Stop();
            }
        }

        private void OnDestroy()
        {
            if (_graph.IsValid())
            {
                _graph.Destroy();
            }
        }

        public void SetAnimation(AnimationClip clip)
        {
            SetAnimation(clip, playOnAssign);
        }

        public void SetAnimation(AnimationClip clip, bool autoPlay)
        {
            if (!_graph.IsValid())
            {
                return;
            }

            _currentClip = clip;
            ReplaceCurrentPlayable(clip);

            if (autoPlay && clip != null)
            {
                Play();
            }
            else if (clip == null)
            {
                Stop();
            }
        }

        public void Play()
        {
            if (!_graph.IsValid() || !_currentPlayable.IsValid())
            {
                return;
            }

            _currentPlayable.SetTime(0f);
            _currentPlayable.SetSpeed(1f);

            if (!_graph.IsPlaying())
            {
                _graph.Play();
            }
        }

        public void Stop()
        {
            if (_currentPlayable.IsValid())
            {
                _currentPlayable.SetTime(0f);
                _currentPlayable.Pause();
            }

            if (_graph.IsValid() && _graph.IsPlaying())
            {
                _graph.Stop();
            }
        }

        public void ClearAnimation()
        {
            SetAnimation(null, false);
        }

        private void ReplaceCurrentPlayable(AnimationClip clip)
        {
            if (_currentPlayable.IsValid())
            {
                _currentPlayable.Destroy();
                _currentPlayable = default;
            }

            if (clip == null)
            {
                _animationOutput.SetSourcePlayable(Playable.Null);
                return;
            }

            _currentPlayable = AnimationClipPlayable.Create(_graph, clip);
            _currentPlayable.SetApplyFootIK(false);
            _currentPlayable.SetApplyPlayableIK(false);

            if (!loopAnimation)
            {
                _currentPlayable.SetDuration(clip.length);
            }

            _animationOutput.SetSourcePlayable(_currentPlayable);
        }
    }
}
