using System.Collections.Generic;
using System.Linq;
using Kaede.Scripts.Item;
using Kaede.Scripts.Managers;

namespace Kaede.Scripts.GamePlay
{
    public enum CookingState
    {
        Cooking,
        Resting,
        Finished
    }
    
    public class ComboCookingModel
    {
        public List<MenuData> MenuDatas { get; private set; }
        public int CurrentMenuIndex { get; private set; } = 0;
        public int CurrentStepIndex { get; private set; } = 0;
        public int CurrentComboIndex { get; private set; } = 0;
        
        public float StartTime { get; private set; } = 0;
        
        public ScoreManager ScoreManager { get; }
        public CookingState GameState { get; set; } = CookingState.Cooking;
        
        public ComboCookingModel(List<MenuData> menus, float maxTimePerCombo = 5f, ScoreManager scoreManager = null)
        {
            MenuDatas       = menus ?? new List<MenuData>();
            ScoreManager    = scoreManager ?? new ScoreManager();
        }
        

        #region Combo Methods
        public void ResetCombo()
        {
            CurrentComboIndex = 0;
            GameState         = CookingState.Cooking;
        }

        public void NextCombo()
        {
            CurrentComboIndex++;
        }
        #endregion

        #region Step Methods
        public void ResetStep() => CurrentStepIndex = 0;

        public bool NextStep()
        {
            if (HasNextStep())
            {
                CurrentStepIndex++;
                return true;
            }
            return false;
        }
        #endregion

        #region Menu Methods
        public void NextMenu()
        {
            ScoreManager.FinalizeCurrentMenuScore();
            
            CurrentMenuIndex++;
            CurrentComboIndex = 0;
            ResetStep();
        }

        public void CompleteMenu()
        {
            ScoreManager.FinalizeCurrentMenuScore();
        }

        #endregion

        public void Resting(float duration)
        {
            GameState = CookingState.Resting;
        }
        
        public void GameOver()
        {
            
        }

        // ----------------- Helpers (MVP: Presenter ไม่ต้องรู้อินเทอร์นัล) -----------------

        // การแสดงผลแบบเก่า
        public bool TryGetCurrentKeys(out List<ComboKey> keys)
        {
            keys = null;
            var seq = GetCurrentSequence();
            if (seq == null) return false;

            keys = seq.Count == 0
                ? new List<ComboKey>()
                : seq.ConvertAll(c => c.key);
            return true;
        }
        
        public bool TryGetCurrentComboSettings(out List<ComboKeySetting> comboSettings)
        {
            comboSettings = null;
            var seq = GetCurrentSequence();
            if (seq == null) return false;

            comboSettings = seq.Count == 0
                ? new List<ComboKeySetting>()
                : new List<ComboKeySetting>(seq);

            return true;
        }

        public bool TryGetCurrentSequenceCount(out int count)
        {
            count = 0;
            var seq = GetCurrentSequence();
            if (seq == null) return false;               // ไม่มีเมนู/สเต็ปปัจจุบัน
            count = seq.Count;
            return true;
        }

        public bool TryGetExpectedCombo(out ComboKeySetting expected)
        {
            expected = null;
            var seq = GetCurrentSequence();
            if (seq == null || CurrentComboIndex >= seq.Count) return false;
            expected = seq[CurrentComboIndex];
            return true;
        }

        public bool HasNextStep()
        {
            if (MenuDatas == null || CurrentMenuIndex >= MenuDatas.Count) return false;
            var menu = MenuDatas[CurrentMenuIndex];
            return menu?.steps != null && CurrentStepIndex + 1 < menu.steps.Count;
        }

        public bool HasNextMenu()
        {
            return MenuDatas != null && CurrentMenuIndex + 1 < MenuDatas.Count;
        }

        // ---------- ใช้ Steps + Preset อย่างเดียว ----------
        private List<ComboKeySetting> GetCurrentSequence()
        {
            if (MenuDatas == null || CurrentMenuIndex >= MenuDatas.Count) return null;

            var menu = MenuDatas[CurrentMenuIndex];
            if (menu?.steps == null || CurrentStepIndex >= menu.steps.Count) return null;

            // StepRef ต้องมี ResolveSequence() คืน List<ComboKeySetting>
            var stepRef = menu.steps[CurrentStepIndex];
            return stepRef?.ResolveSequence() ?? new List<ComboKeySetting>();
        }
    }
}

