using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kaede.Scripts.Animation {
    /// <summary>
    /// เล่นแอนิเมชันแบบ Step-by-step: กดครั้งละชิ้น → เล่นคลิปสั้น → ค้างท่าปลายทาง
    /// - ใช้ Playables + AnimationMixerPlayable ทำ crossfade เนียน ๆ
    /// - รองรับ Animation Events: AE_InsertStart(), AE_StepLock(int slotIndex), AE_InsertEnd()
    /// - เลือกค้างเฟรมสุดท้ายหรือรีเซ็ตหลังจบลิสต์ได้
    /// - มี IsBusy กันกดรัว และโหมด Scaled/Unscaled time
    /// </summary>
    [DisallowMultipleComponent]
    public class ComboStepAnimationCooking : MonoBehaviour {
        [Header("Animator / Output")]
        [SerializeField] private Animator animator;

        [Header("Sequence Clips (Step 0..N-1)")]
        [Tooltip("คลิปที่เล่นทีละสเต็ป เช่น Insert_0..Insert_N-1")]
        [SerializeField] private List<AnimationClip> sequenceClips = new();

        [Header("Play Options")]
        [SerializeField] private bool playOnAssign = false;
        [SerializeField] private bool loopAnimation = false; // ใช้ตอน mode=Single เท่านั้น
        [SerializeField] private bool resetIndexAfterEnd = false; // false = ค้างที่ท่าสุดท้าย
        [SerializeField] private float crossfadeDuration = 0.12f;
        [SerializeField] private bool pauseAtNextClipStart = true;
        
        public enum UpdateClock { Scaled, Unscaled }
        [SerializeField] private UpdateClock updateClock = UpdateClock.Scaled;

        [Header("Wrong Feedback (Optional)")]
        [SerializeField] private AnimationClip wrongFeedbackClip;
        [SerializeField] private float wrongFeedbackFade = 0.08f;

        // === Events for gameplay hook ===
        public event Action<int> OnStepStart; // k
        public event Action<int> OnStepLocked; // slot index
        public event Action<int> OnStepEnd;   // k

        // === State ===
        public int StepIndex => _stepIndex;
        public bool IsBusy => _isSequencePlaying || _wrongFeedbackCancellation != null;

        // === Playables ===
        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationMixerPlayable _mixer;        // 2 ช่อง: [0]=current, [1]=next
        private Playable _current;                    // เก็บ current clip playable
        private CancellationTokenSource _sequenceCts; // สำหรับ step ต่อเนื่อง
        private CancellationTokenSource _wrongFeedbackCancellation;

        private int _stepIndex = 0;
        private bool _graphReady = false;
        private bool _isSequencePlaying = false;
        private bool _autoResetAfterSingleClip = false;

        #region Unity lifecycle
        private void Awake()
        {
            if (!animator) animator = GetComponent<Animator>();
            EnsureGraph();
        }

        private void OnEnable()
        {
            if (playOnAssign && sequenceClips.Count > 0)
            {
                // วางท่าเริ่ม (ไม่เล่น) เพื่อให้ค้างท่าตาม index ปัจจุบัน
                PoseStepAt(_stepIndex);
            }
        }

        private void OnDisable()
        {
            StopAllRoutines();
            StopGraph();
        }

        private void OnDestroy()
        {
            StopAllRoutines();
            DestroyGraph();
        }
        #endregion

        #region Public API (Buttons / Game input)
        /// <summary>กดเพิ่มหนึ่งสเต็ป ถ้าเล่นได้</summary>
        public async UniTask PlayNext(bool force = false, CancellationToken cancellationToken = default)
        {
            if (!force && !CanPlayNext()) return;

            if (force)
            {
                if (sequenceClips.Count == 0)
                {
                    return;
                }

                if (_isSequencePlaying)
                {
                    StopAllRoutines();

                    if (_stepIndex < sequenceClips.Count - 1)
                    {
                        _stepIndex++;
                    }
                }
                else if (_stepIndex >= sequenceClips.Count)
                {
                    return;
                }
            }

            _sequenceCts?.Cancel();
            _sequenceCts?.Dispose();

            _sequenceCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var ct = _sequenceCts.Token;

            try
            {
                await PlayStep(_stepIndex, crossfadeDuration, ct);
            }
            catch (OperationCanceledException)
            {
                ResetMixerToCurrent();
                _isSequencePlaying = false;
                return;
            }

            _stepIndex++;
            if (_stepIndex >= sequenceClips.Count)
            {
                // จบท้ายลิสต์
                if (resetIndexAfterEnd || _autoResetAfterSingleClip)
                {
                    _stepIndex = 0;
                }
            }
        }

        public bool CanPlayNext() => !_isSequencePlaying && _stepIndex < sequenceClips.Count;

        /// <summary>วางค้างท่าสเต็ป k โดยไม่เล่น (ใช้ดีบัก/ตั้งต้น)</summary>
        public void PoseStepAt(int index)
        {
            if (!IsComponentAlive())
            {
                return;
            }

            index = Mathf.Clamp(index, 0, sequenceClips.Count - 1);
            var clip = sequenceClips[index];
            EnsureGraph();
            EnsureMixer();

            if (!IsMixerReady())
            {
                return;
            }

            if (_current.IsValid())
            {
                _mixer.DisconnectInput(0);
                _mixer.DisconnectInput(1);
                _current.Destroy();
                _current = Playable.Null;
            }
            
            // ทำ playable ใหม่แล้ว set เวลาไปท้ายคลิป → Pause = ค้างท่าสุดท้าย
            var p = AnimationClipPlayable.Create(_graph, clip);
            p.SetApplyFootIK(false);
            p.SetApplyPlayableIK(false);
            p.SetTime(clip.length);
            p.SetSpeed(0); // ค้าง
            p.Pause();

            // ใส่เข้า mixer เป็น current
            _mixer.DisconnectInput(0);
            _mixer.DisconnectInput(1);
            _mixer.ConnectInput(0, p, 0, 1f);
            _mixer.SetInputWeight(0, 1f);
            _mixer.SetInputWeight(1, 0f);

            _current = p;
            if (!_graph.IsPlaying()) _graph.Play();
        }

        /// <summary>รีเซ็ตลิสต์และค้างท่าเริ่มต้น (หรือไม่ค้างก็ได้)</summary>
        public void RewindToStart(bool poseFirst = true) 
        {
            StopAllRoutines();
            _stepIndex = 0;
            if (poseFirst && sequenceClips.Count > 0)
            {
                PoseStepAt(0);
            }
        }

        /// <summary>เล่น wrong feedback clip (เช่น สั่น/ปัดออก) แล้วค้างท่าเดิม</summary>
        public async void PlayWrongFeedback()
        {
            if (wrongFeedbackClip == null || !_graphReady) return;
            if (_wrongFeedbackCancellation != null) return; // กันซ้อน

            _wrongFeedbackCancellation = new CancellationTokenSource();
            try
            {
                await CrossfadeToTempClip(wrongFeedbackClip, wrongFeedbackFade, _wrongFeedbackCancellation.Token);
            } 
            catch (OperationCanceledException)
            {
                // ignored
            } 
            finally
            {
                _wrongFeedbackCancellation?.Dispose();
                _wrongFeedbackCancellation = null;
            }
        }
        
        public bool TryPlayWrongFeedback()
        {
            if (wrongFeedbackClip == null || !_graphReady)
            {
                return false;
            }

            if (_wrongFeedbackCancellation != null)
            {
                return true;
            }

            PlayWrongFeedback();
            return true;
        }

        public void SetAnimation(ComboStepAnimationDefinition definition, bool autoPlaySequence)
        {
            Stop();

            wrongFeedbackClip = definition.WrongFeedbackClip;
            sequenceClips.Clear();
            var shouldAutoReset = definition.Mode == ComboStepAnimationMode.SingleClip;
            
            switch (definition.Mode)
            {
                case ComboStepAnimationMode.SingleClip:
                    if (definition.SingleClip != null)
                    {
                        sequenceClips.Add(definition.SingleClip);
                    }
                    break;
                case ComboStepAnimationMode.SequentialClips:
                    if (definition.SequentialClips != null)
                    {
                        foreach (var clip in definition.SequentialClips)
                        {
                            if (clip != null)
                            {
                                sequenceClips.Add(clip);
                            }
                        }
                    }
                    break;
                default:
                    break;
            }

            _stepIndex = 0;
            _autoResetAfterSingleClip = shouldAutoReset && sequenceClips.Count == 1;

            if (sequenceClips.Count == 0)
            {
                return;
            }

            if (autoPlaySequence)
            {
                PlayNext();
            }
            else
            {
                RewindToStart(true);
            }
        }

        public void ClearAnimation()
        {
            Stop();
            sequenceClips.Clear();
            wrongFeedbackClip = null;
            _stepIndex = 0;
            _autoResetAfterSingleClip = false;
        }

        public void Stop()
        {
            StopAllRoutines();
            StopGraph();
        }
        #endregion

        #region Animation Events (call from clips)
        // ใส่ event ในคลิปที่เฟรมแรกของสเต็ป
        public void AE_InsertStart()
        {
            OnStepStart?.Invoke(Mathf.Clamp(_stepIndex, 0, sequenceClips.Count - 1));
        }

        // ใส่ event ตอนชิ้นเข้าล็อค: ส่ง slotIndex มากับ Event (หรือไม่ส่งก็จะใช้ _stepIndex-1)
        public void AE_StepLock(int slotIndex)
        {
            int idx = slotIndex >= 0 ? slotIndex : Mathf.Max(0, _stepIndex - 1);
            OnStepLocked?.Invoke(idx);
        }

        public void AE_InsertEnd()
        {
            OnStepEnd?.Invoke(Mathf.Clamp(_stepIndex, 0, sequenceClips.Count - 1));
        }
        #endregion

        #region Core playing
        private async UniTask PlayStep(int index, float fade, CancellationToken ct)
        {
            if (!IsComponentAlive()) return;
            if (index < 0 || index >= sequenceClips.Count) return;

            var clip = sequenceClips[index];
            EnsureGraph();
            EnsureMixer();

            if (!IsMixerReady()) return;

            _isSequencePlaying = true;

            var nextPlayable = AnimationClipPlayable.Create(_graph, clip);
            bool promotedToCurrent = false;

            try
            {
                nextPlayable.SetApplyFootIK(false);
                nextPlayable.SetApplyPlayableIK(false);
                nextPlayable.SetTime(0);
                nextPlayable.SetSpeed(1);

                // ต่อเข้าช่อง 1 เป็น next, ช่อง 0 เก็บ current (ถ้ามี)
                if (!IsMixerReady())
                {
                    ResetMixerToCurrent();
                    _isSequencePlaying = false;
                    return;
                }

                _mixer.DisconnectInput(1);
                _mixer.ConnectInput(1, nextPlayable, 0, 0f); // เริ่มน้ำหนัก 0

                if (!_graph.IsPlaying()) _graph.Play();

                // Crossfade: current(0)->down, next(1)->up
                float t = 0f;
                while (t < fade)
                {
                    ct.ThrowIfCancellationRequested();
                    if (!IsMixerReady())
                    {
                        ResetMixerToCurrent();
                        _isSequencePlaying = false;
                        return;
                    }

                    t += DeltaTime();
                    float w = Mathf.Clamp01(t / fade);
                    _mixer.SetInputWeight(0, 1f - w);
                    _mixer.SetInputWeight(1, w);
                    await UniTask.Yield(ct);
                }
                if (!IsMixerReady())
                {
                    ResetMixerToCurrent();
                    _isSequencePlaying = false;
                    return;
                }

                _mixer.SetInputWeight(0, 0f);
                _mixer.SetInputWeight(1, 1f);

                // ทำ next เป็น current
                if (_current.IsValid() && IsMixerReady()) _mixer.DisconnectInput(0);
                _current = nextPlayable;
                promotedToCurrent = true;

                // รอจนคลิปจบ แล้วค้างเฟรมสุดท้าย
                double length = clip.length;
                while (_current.IsValid() && _current.GetTime() < length)
                {
                    ct.ThrowIfCancellationRequested();
                    await UniTask.Yield(ct);
                }
            }
            catch (OperationCanceledException)
            {
                if (!promotedToCurrent && nextPlayable.IsValid())
                {
                    if (IsMixerReady())
                    {
                        _mixer.DisconnectInput(1);
                        _mixer.SetInputWeight(1, 0f);
                        _mixer.SetInputWeight(0, 1f);
                    }

                    nextPlayable.Destroy();
                }

                ResetMixerToCurrent();
                _isSequencePlaying = false;
                throw;
            }
            finally
            {
                if (!promotedToCurrent && nextPlayable.IsValid())
                {
                    nextPlayable.Destroy();
                }
            }
            
            // ค้างเฟรมสุดท้าย หริือค้างเฟรมแรกของคลิปถัดไป / รีเซ็ต
            var nextIndex = index + 1;
            var hasNext = nextIndex < sequenceClips.Count;

            if (pauseAtNextClipStart && hasNext) 
            {
                // มีคลิปถัดไป → ค้างเฟรมแรกของคลิปถัดไป
                PoseClipStart(sequenceClips[nextIndex]);
            }
            else if (pauseAtNextClipStart && !hasNext && resetIndexAfterEnd && sequenceClips.Count > 0) 
            {
                // จบลิสต์ + เซ็ตให้รีเซ็ต → ค้างเฟรมแรกของคลิปแรก
                PoseClipStart(sequenceClips[0]);
            }
            else 
            {
                // กรณีอื่น ๆ → ค้างเฟรมสุดท้ายของคลิปปัจจุบันเหมือนเดิม
                if (_current.IsPlayableOfType<AnimationClipPlayable>()) 
                {
                    var cp = (AnimationClipPlayable)_current;
                    PauseAtEnd(cp, clip);
                } 
                else if (IsMixerReady()) 
                {
                    _current.SetSpeed(0);
                    _current.Pause();
                    _mixer.DisconnectInput(0);
                    _mixer.DisconnectInput(1);
                    _mixer.ConnectInput(0, _current, 0, 1f);
                }
            }

            _isSequencePlaying = false;
        }

        private async UniTask CrossfadeToTempClip(AnimationClip tempClip, float fade, CancellationToken ct) 
        {
            if (!IsComponentAlive())
            {
                return;
            }

            EnsureGraph();
            EnsureMixer();

            if (!IsMixerReady())
            {
                return;
            }

            // next temp
            var tempPlayable = AnimationClipPlayable.Create(_graph, tempClip);
            tempPlayable.SetApplyFootIK(false);
            tempPlayable.SetApplyPlayableIK(false);
            tempPlayable.SetTime(0);
            tempPlayable.SetSpeed(1);

            if (!IsMixerReady())
            {
                tempPlayable.Destroy();
                return;
            }

            _mixer.DisconnectInput(1);
            _mixer.ConnectInput(1, tempPlayable, 0, 0f);

            if (!_graph.IsPlaying()) _graph.Play();

            // Fade in temp
            float t = 0f;
            while (t < fade) 
            {
                ct.ThrowIfCancellationRequested();
                t += DeltaTime();
                float w = Mathf.Clamp01(t / fade);
                if (!IsMixerReady())
                {
                    tempPlayable.Destroy();
                    return;
                }

                _mixer.SetInputWeight(1, w);
                _mixer.SetInputWeight(0, 1f - w);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            if (!IsMixerReady())
            {
                tempPlayable.Destroy();
                return;
            }

            _mixer.SetInputWeight(1, 1f);
            _mixer.SetInputWeight(0, 0f);

            // รอ temp เล่นจบ
            var length = tempClip.length;
            while (tempPlayable.IsValid() && tempPlayable.GetTime() < length) 
            {
                ct.ThrowIfCancellationRequested();
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // กลับค้างที่ current เดิม (ถ้ามี)
            if (_current.IsValid())
            {
                // ค้างท่าสุดท้ายของ current (หรือจะกลับกลางคลิปก็เปลี่ยนได้)
                if (_current.IsPlayableOfType<AnimationClipPlayable>())
                {
                    var cp = (AnimationClipPlayable)_current;
                    PauseAtEnd(cp, cp.GetAnimationClip());
                }
                else if (IsMixerReady())
                {
                    // ค้างที่สถานะปัจจุบันของ _current
                    _current.SetSpeed(0);
                    _current.Pause();
                    _mixer.SetInputWeight(0, 1f);
                }

                // Fade out temp
                float t2 = 0f;
                while (t2 < fade)
                {
                    ct.ThrowIfCancellationRequested();
                    if (!IsMixerReady())
                    {
                        tempPlayable.Destroy();
                        return;
                    }

                    t2 += DeltaTime();
                    float w = 1f - Mathf.Clamp01(t2 / fade);
                    _mixer.SetInputWeight(1, w);
                    _mixer.SetInputWeight(0, 1f - w);
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }

            // cleanup
            if (IsMixerReady())
            {
                _mixer.DisconnectInput(1);
            }
            tempPlayable.Destroy();
        }

        private void PauseAtEnd(AnimationClipPlayable playable, AnimationClip clip)
        {
            playable.SetTime(clip.length);
            playable.SetSpeed(0);
            playable.Pause();
            // ให้ช่อง current = playable ที่ค้างอยู่ ช่อง 1 ว่าง
            if (!IsMixerReady())
            {
                return;
            }

            _mixer.DisconnectInput(0);
            _mixer.DisconnectInput(1);
            _mixer.ConnectInput(0, playable, 0, 1f);
        }

        private void PoseClipStart(AnimationClip clip) {
            if (!IsComponentAlive())
            {
                return;
            }

            EnsureGraph();
            EnsureMixer();

            if (!IsMixerReady())
            {
                return;
            }

            // เคลียร์ current เดิมถ้ามี
            if (_current.IsValid()) {
                _mixer.DisconnectInput(0);
                _mixer.DisconnectInput(1);
                _current.Destroy();
                _current = Playable.Null;
            }

            var p = AnimationClipPlayable.Create(_graph, clip);
            p.SetApplyFootIK(false);
            p.SetApplyPlayableIK(false);
            p.SetTime(0.0); // เฟรมแรก
            p.SetSpeed(0);
            p.Pause();

            _mixer.DisconnectInput(0);
            _mixer.DisconnectInput(1);
            _mixer.ConnectInput(0, p, 0, 1f);
            _mixer.SetInputWeight(0, 1f);
            _mixer.SetInputWeight(1, 0f);

            _current = p;
            if (!_graph.IsPlaying()) _graph.Play();
        }

        #endregion

        #region Graph helpers
        private bool IsComponentAlive()
        {
            return this != null;
        }

        private void EnsureGraph()
        {
            if (!IsComponentAlive())
            {
                return;
            }

            if (_graphReady) return;
            if (!animator)
            {
                animator = GetComponent<Animator>();
                if (!animator) {
                    Debug.LogWarning("ComboStepAnimationCooking needs an Animator.", this);
                    return;
                }
            }
            var graphName = IsComponentAlive() ? name : "ComboStep";
            _graph = PlayableGraph.Create($"{graphName}_ComboStepGraph");
            _graph.SetTimeUpdateMode(updateClock == UpdateClock.Scaled
                ? DirectorUpdateMode.GameTime
                : DirectorUpdateMode.UnscaledGameTime);

            _output = AnimationPlayableOutput.Create(_graph, "ComboStepOutput", animator);
            EnsureMixer();

            _graphReady = true;
        }

        private void EnsureMixer()
        {
            if (!IsComponentAlive())
            {
                return;
            }

            if (_mixer.IsValid()) return;
            _mixer = AnimationMixerPlayable.Create(_graph, 2, true);
            _output.SetSourcePlayable(_mixer);
            _mixer.SetInputWeight(0, 1f);
            _mixer.SetInputWeight(1, 0f);
        }

        private void StopGraph()
        {
            if (!_graphReady) return;
            if (_graph.IsValid()) {
                _graph.Stop();
            }
        }

        private void DestroyGraph()
        {
            if (!_graphReady) return;
            if (_current.IsValid()) _current.Destroy();
            if (_mixer.IsValid()) _mixer.Destroy();
            if (_graph.IsValid()) _graph.Destroy();
            _graphReady = false;
        }

        private bool IsMixerReady()
        {
            return IsComponentAlive() && _graphReady && _graph.IsValid() && _mixer.IsValid();
        }

        private void StopAllRoutines()
        {
            _sequenceCts?.Cancel();
            _sequenceCts?.Dispose();
            _sequenceCts = null;

            _wrongFeedbackCancellation?.Cancel();
            _wrongFeedbackCancellation?.Dispose();
            _wrongFeedbackCancellation = null;

            ResetMixerToCurrent();
            _isSequencePlaying = false;
        }

        private float DeltaTime()
        {
            return updateClock == UpdateClock.Scaled ? Time.deltaTime : Time.unscaledDeltaTime;
        }

        private void ResetMixerToCurrent()
        {
            if (!IsMixerReady())
            {
                return;
            }

            if (_current.IsValid())
            {
                _mixer.DisconnectInput(0);
                _mixer.DisconnectInput(1);
                _mixer.ConnectInput(0, _current, 0, 1f);
                _mixer.SetInputWeight(0, 1f);
            }
            else
            {
                _mixer.DisconnectInput(1);
                _mixer.SetInputWeight(0, 1f);
            }

            _mixer.SetInputWeight(1, 0f);
        }
        #endregion

        #region ContextMenu (Editor quick test)
        [ContextMenu("Play Next Step")]
        private void CM_PlayNext() => PlayNext();

        [ContextMenu("Rewind To Start (Pose)")]
        private void CM_Rewind() => RewindToStart(true);

        [ContextMenu("Play Wrong Feedback")]
        private void CM_Wrong() => PlayWrongFeedback();
        #endregion
    }
}
