using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Kaede.Scripts.Animation 
{
    /// <summary>
    /// เล่นแอนิเมชันแบบ Step-by-step สำหรับการเสียบไม้:
    /// - กดปุ่ม = เล่นคลิปของ step ปัจจุบันทันที (ไม่มีหน่วง)
    /// - กดรัว = ยกเลิกของเก่าแล้วเล่นอันใหม่ทันที (override)
    /// - เลือกได้ว่าจะค้างเฟรมสุดท้ายของคลิป หรือเฟรมแรกของคลิปถัดไป
    /// </summary>
    [DisallowMultipleComponent]
    public class ComboStepAnimationCooking : MonoBehaviour 
    {
        [Header("Animator / Output")]
        [SerializeField] private Animator animator;

        [Header("Sequence Clips (Step 0..N-1)")]
        [Tooltip("คลิปที่เล่นทีละสเต็ป เช่น Insert_0..Insert_N-1")]
        [SerializeField] private List<AnimationClip> sequenceClips = new();

        [Header("Play Options")]
        [SerializeField] private bool playOnAssign = false;
        [SerializeField] private bool resetIndexAfterEnd = false; // false = ค้างที่ step สุดท้าย
        [SerializeField] private bool pauseAtNextClipStart = false; // true = จบแล้วค้างที่เฟรมแรกของคลิปถัดไป

        public enum UpdateClock { Scaled, Unscaled }
        [SerializeField] private UpdateClock updateClock = UpdateClock.Scaled;

        [Header("Wrong Feedback (Optional)")]
        [SerializeField] private AnimationClip wrongFeedbackClip;
        [SerializeField] private float wrongFeedbackFade = 0.08f;

        // === Events for gameplay hook ===
        public event Action<int> OnStepStart;  // index ของ step ที่เริ่มเล่น
        public event Action<int> OnStepLocked; // slot index / step index
        public event Action<int> OnStepEnd;    // index ของ step ที่จบ

        // === State ===
        public int StepIndex => _stepIndex;
        public bool IsBusy => _wrongFeedbackCancellation != null; // main sequence ไม่ล็อกแล้ว กดทับได้

        // === Playables ===
        private PlayableGraph _graph;
        private AnimationPlayableOutput _output;
        private AnimationMixerPlayable _mixer; // ใช้ input 0 สำหรับ main, input 1 สำหรับ wrongFeedback
        private AnimationClipPlayable _currentPlayable;
        private bool _graphReady = false;

        private int _stepIndex = 0;
        private bool _autoResetAfterSingleClip = false;

        // ใช้ soft-cancel รอบเก่า (override) เวลา PlayNext ถูกกดซ้ำ
        private int _playVersion = 0;

        // wrong feedback
        private CancellationTokenSource _wrongFeedbackCancellation;

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
        /// <summary>
        /// กดเพิ่มหนึ่งสเต็ป: กดเมื่อไหร่ = เล่นคลิปของ step ปัจจุบันทันที
        /// กดซ้ำระหว่างกำลังเล่น = override รอบเก่า (ยกเลิกแล้วเล่นอันใหม่)
        /// </summary>
        public async void PlayNext()
        {
            if (!CanPlayNext())
                return;

            // ถ้า index หลุด ให้จัดการให้กลับมาอยู่ในช่วง
            if (_stepIndex < 0 || _stepIndex >= sequenceClips.Count)
            {
                if (resetIndexAfterEnd || _autoResetAfterSingleClip) 
                {
                    _stepIndex = 0;
                } else {
                    _stepIndex = Mathf.Clamp(_stepIndex, 0, sequenceClips.Count - 1);
                }
            }

            int indexToPlay = _stepIndex;
            
            int nextIndex = _stepIndex + 1;
            if (nextIndex >= sequenceClips.Count) {
                if (resetIndexAfterEnd || _autoResetAfterSingleClip) {
                    nextIndex = 0;                      // วนกลับไปตัวแรก
                } else {
                    nextIndex = sequenceClips.Count - 1; // ค้างที่ตัวสุดท้าย
                }
            }
            _stepIndex = nextIndex;   // <<< ขยับ index ตั้งแต่ตอนกดแล้ว

            // รอบใหม่ → เพิ่ม version เพื่อให้รอบเก่ารู้ตัวว่าควรเลิกทำงาน
            _playVersion++;
            int localVersion = _playVersion;

            // ตัด playable เดิมออก / ล้างท่าเดิม
            StopCurrentSequencePlayable();

            try 
            {
                // --- 4) เล่นคลิปของ indexToPlay ทันที ---
                await PlayStepImmediate(indexToPlay, localVersion);

                // ถ้าโดน override ระหว่างรอ ก็ไม่ต้องทำอะไรต่อ
                if (localVersion != _playVersion)
                    return;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ComboStepAnimationCooking] PlayNext error: {ex}", this);
            }
        }

        /// <summary>
        /// ตอนนี้: ถ้ามีคลิปในลิสต์ = เล่นได้ (ให้ logic ข้างในจัดการ index เอง)
        /// </summary>
        public bool CanPlayNext() => sequenceClips != null && sequenceClips.Count > 0;

        /// <summary>วางค้างท่าสเต็ป k โดยไม่เล่น (ใช้ดีบัก/ตั้งต้น)</summary>
        public void PoseStepAt(int index) 
        {
            if (!IsComponentAlive()) return;
            if (sequenceClips == null || sequenceClips.Count == 0) return;

            index = Mathf.Clamp(index, 0, sequenceClips.Count - 1);
            var clip = sequenceClips[index];

            EnsureGraph();
            EnsureMixer();
            if (!IsMixerReady()) return;

            // ทำ playable ใหม่ แล้วค้างที่เฟรมสุดท้าย
            var playable = AnimationClipPlayable.Create(_graph, clip);
            playable.SetApplyFootIK(false);
            playable.SetApplyPlayableIK(false);

            // ค้างเฟรมท้ายเหมือน PauseAtEnd
            PauseAtEnd(playable, clip);

            _currentPlayable = playable;
            if (!_graph.IsPlaying()) _graph.Play();
        }

        /// <summary>รีเซ็ตลิสต์และค้างท่าเริ่มต้น (หรือไม่ค้างก็ได้)</summary>
        public void RewindToStart(bool poseFirst = true)
        {
            StopAllRoutines();
            _stepIndex = 0;
            _playVersion++;

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
                // ปกติ ไม่ต้อง log
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
            _playVersion++;
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

        // ใส่ event ที่ท้ายคลิป
        public void AE_InsertEnd()
        {
            OnStepEnd?.Invoke(Mathf.Clamp(_stepIndex, 0, sequenceClips.Count - 1));
        }
        #endregion

        #region Core playing

        /// <summary>
        /// ตัด playable เดิมออกจาก mixer และทำลายมัน (ไม่ทำลายทั้งกราฟ)
        /// ใช้ตอนจะเล่นรอบใหม่ทับของเดิม
        /// </summary>
        private void StopCurrentSequencePlayable()
        {
            if (!IsMixerReady())
                return;

            _mixer.DisconnectInput(0);

            if (_currentPlayable.IsValid()) 
            {
                _currentPlayable.Destroy();
                _currentPlayable = default;
            }
        }

        /// <summary>
        /// เล่นคลิปของ step index แบบ “เล่นทันที” ไม่มี crossfade ระหว่าง step
        /// ใช้ playVersion เพื่อรู้ว่าถูก override หรือยัง
        /// </summary>
        private async UniTask PlayStepImmediate(int index, int playVersion)
        {
            if (!IsComponentAlive()) return;
            if (index < 0 || index >= sequenceClips.Count) return;
            if (playVersion != _playVersion) return; // ถูก override ก่อนเริ่ม

            var clip = sequenceClips[index];
            EnsureGraph();
            EnsureMixer();
            if (!IsMixerReady()) return;
            if (playVersion != _playVersion) return;

            // สร้าง playable ใหม่
            var playable = AnimationClipPlayable.Create(_graph, clip);
            playable.SetApplyFootIK(false);
            playable.SetApplyPlayableIK(false);
            playable.SetTime(0);
            playable.SetSpeed(1);

            // ต่อเข้าช่อง 0 ของ mixer และ set weight เต็ม
            _mixer.DisconnectInput(0);
            _mixer.ConnectInput(0, playable, 0, 1f);
            _mixer.SetInputWeight(0, 1f);

            _currentPlayable = playable;

            if (!_graph.IsPlaying())
                _graph.Play();

            // รอจนคลิปจบ หรือถูก override
            double length = clip.length;
            while (playable.IsValid() && playable.GetTime() < length)
            {
                if (playVersion != _playVersion) 
                {
                    // ถูก override → ทำลายตัวเองแล้วจบ
                    playable.Destroy();
                    return;
                }

                await UniTask.Yield();
            }

            if (!playable.IsValid())
            {
                return;
            }

            if (playVersion != _playVersion)
            {
                playable.Destroy();
                return;
            }

            // เลือกค้างเฟรมตามโหมด pauseAtNextClipStart
            if (!pauseAtNextClipStart) 
            {
                // 1) ค้างเฟรมสุดท้ายของคลิปนี้
                PauseAtEnd(playable, clip);
                _currentPlayable = playable;
            } 
            else 
            {
                // 2) ค้างเฟรมแรกของคลิปถัดไป (หรือคลิปแรกถ้า reset), ถ้าไม่มี → ค้างท้ายคลิปนี้
                int nextIndex = index + 1;
                bool hasNext = nextIndex < sequenceClips.Count;

                if (hasNext) 
                {
                    PoseClipStart(sequenceClips[nextIndex]);
                    playable.Destroy(); // clip ปัจจุบันไม่ใช้แล้ว
                }
                else if (resetIndexAfterEnd && sequenceClips.Count > 0) 
                {
                    PoseClipStart(sequenceClips[0]);
                    playable.Destroy();
                }
                else 
                {
                    PauseAtEnd(playable, clip);
                    _currentPlayable = playable;
                }
            }
        }

        /// <summary>
        /// เล่น wrongFeedback บน input 1 แล้ว crossfade เข้า/ออก จาก main (input 0)
        /// </summary>
        private async UniTask CrossfadeToTempClip(AnimationClip tempClip, float fade, CancellationToken ct)
        {
            if (!IsComponentAlive()) return;
            EnsureGraph();
            EnsureMixer();
            if (!IsMixerReady()) return;

            var tempPlayable = AnimationClipPlayable.Create(_graph, tempClip);
            tempPlayable.SetApplyFootIK(false);
            tempPlayable.SetApplyPlayableIK(false);
            tempPlayable.SetTime(0);
            tempPlayable.SetSpeed(1);

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
            double length = tempClip.length;
            while (tempPlayable.IsValid() && tempPlayable.GetTime() < length) 
            {
                ct.ThrowIfCancellationRequested();
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            // กลับไปหาท่าปัจจุบัน (ค้าง)
            if (_currentPlayable.IsValid()) 
            {
                PauseAtEnd(_currentPlayable, _currentPlayable.GetAnimationClip());
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

            if (IsMixerReady()) 
            {
                _mixer.DisconnectInput(1);
            }
            tempPlayable.Destroy();
        }

        /// <summary>
        /// ค้างที่ "เฟรมสุดท้าย" ของ clip (รองรับกรณี Loop Time)
        /// </summary>
        private void PauseAtEnd(AnimationClipPlayable playable, AnimationClip clip) 
        {
            if (!playable.IsValid() || clip == null) return;

            double endTime = clip.length;

            // ถ้า clip loop อยู่ ให้ถอยเวลามาหนึ่งเฟรม เพื่อไม่ให้เด้งกลับเฟรมแรก
            if (clip.isLooping && clip.frameRate > 0f)
            {
                endTime = Math.Max(0.0, clip.length - (1.0 / clip.frameRate));
            }

            playable.SetTime(endTime);
            playable.SetSpeed(0);
            playable.Pause();

            if (!IsMixerReady()) return;

            _mixer.DisconnectInput(0);
            _mixer.ConnectInput(0, playable, 0, 1f);
            _mixer.SetInputWeight(0, 1f);
        }

        /// <summary>
        /// ค้างที่ "เฟรมแรก" ของ clip ที่ส่งมา (ใช้ตอน pauseAtNextClipStart = true)
        /// </summary>
        private void PoseClipStart(AnimationClip clip) 
        {
            if (!IsComponentAlive() || clip == null) return;
            EnsureGraph();
            EnsureMixer();
            if (!IsMixerReady()) return;

            // เคลียร์ current เดิม
            if (_currentPlayable.IsValid())
            {
                _mixer.DisconnectInput(0);
                _currentPlayable.Destroy();
                _currentPlayable = default;
            }

            var p = AnimationClipPlayable.Create(_graph, clip);
            p.SetApplyFootIK(false);
            p.SetApplyPlayableIK(false);
            p.SetTime(0.0);
            p.SetSpeed(0);
            p.Pause();

            _mixer.DisconnectInput(0);
            _mixer.ConnectInput(0, p, 0, 1f);
            _mixer.SetInputWeight(0, 1f);

            _currentPlayable = p;
            if (!_graph.IsPlaying()) _graph.Play();
        }

        #endregion

        #region Graph helpers
        private bool IsComponentAlive() => this != null;

        private void EnsureGraph()
        {
            if (!IsComponentAlive()) return;
            if (_graphReady) return;

            if (!animator)
            {
                animator = GetComponent<Animator>();
                if (!animator) {
                    Debug.LogWarning("ComboStepAnimationCooking needs an Animator.", this);
                    return;
                }
            }

            string graphName = IsComponentAlive() ? name : "ComboStep";
            _graph = PlayableGraph.Create($"{graphName}_ComboStepGraph");
            _graph.SetTimeUpdateMode(updateClock == UpdateClock.Scaled
                ? DirectorUpdateMode.GameTime
                : DirectorUpdateMode.UnscaledGameTime);

            _output = AnimationPlayableOutput.Create(_graph, "ComboStepOutput", animator);
            EnsureMixerInternal();

            _graphReady = true;
        }

        private void EnsureMixerInternal()
        {
            if (_mixer.IsValid()) return;
            _mixer = AnimationMixerPlayable.Create(_graph, 2, true);
            _output.SetSourcePlayable(_mixer);
            _mixer.SetInputWeight(0, 1f);
            _mixer.SetInputWeight(1, 0f);
        }

        private void EnsureMixer() 
        {
            if (!IsComponentAlive()) return;
            if (!_graphReady || !_graph.IsValid()) return;
            EnsureMixerInternal();
        }

        private bool IsMixerReady()
        {
            return IsComponentAlive() && _graphReady && _graph.IsValid() && _mixer.IsValid();
        }

        private void StopGraph()
        {
            if (!_graphReady) return;
            if (_graph.IsValid()) 
            {
                _graph.Stop();
            }
        }

        private void DestroyGraph()
        {
            if (!_graphReady) return;

            if (_currentPlayable.IsValid())
            {
                _currentPlayable.Destroy();
                _currentPlayable = default;
            }

            if (_mixer.IsValid()) _mixer.Destroy();
            if (_graph.IsValid()) _graph.Destroy();

            _graphReady = false;
        }

        private void StopAllRoutines()
        {
            _wrongFeedbackCancellation?.Cancel();
            _wrongFeedbackCancellation?.Dispose();
            _wrongFeedbackCancellation = null;
        }

        private float DeltaTime()
        {
            return updateClock == UpdateClock.Scaled ? Time.deltaTime : Time.unscaledDeltaTime;
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
