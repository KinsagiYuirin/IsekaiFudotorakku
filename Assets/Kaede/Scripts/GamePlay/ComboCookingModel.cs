using System.Collections.Generic;
using Kaede.Scripts.Item;

namespace Kaede.Scripts.GamePlay
{
    public class ComboCookingModel
    {
        public List<MenuData> MenuDatas { get; private set; }
        public int CurrentMenuIndex { get; private set; } = 0;
        public int CurrentStepIndex { get; private set; } = 0;
        public int CurrentComboIndex { get; private set; } = 0;

        public float MaxTimePerCombo { get; private set; }
        public float CurrentTimer { get; private set; }

        public ComboCookingModel(List<MenuData> menus, float maxTimePerCombo = 5f)
        {
            MenuDatas = menus ?? new List<MenuData>();
            MaxTimePerCombo = maxTimePerCombo;
            CurrentTimer    = maxTimePerCombo;
        }

        public void Tick(float deltaTime) => CurrentTimer -= deltaTime;

        #region Combo Methods
        public void ResetCombo()
        {
            CurrentComboIndex = 0;
            CurrentTimer      = MaxTimePerCombo;
        }

        public void NextCombo()
        {
            CurrentComboIndex++;
            CurrentTimer = MaxTimePerCombo;
        }
        #endregion

        #region Step Methods
        public void ResetStep() => CurrentStepIndex = 0;
        public void NextStep() => CurrentStepIndex++;
        #endregion

        #region Menu Methods
        public void NextMenu()
        {
            CurrentMenuIndex++;
            CurrentComboIndex = 0;
            CurrentTimer      = MaxTimePerCombo;
            ResetStep();
        }

        public void CompleteMenu()
        {
            
        }
        #endregion

        public void GameOver()
        {
            
        }

        // ----------------- Helpers (MVP: Presenter ไม่ต้องรู้อินเทอร์นัล) -----------------

        public bool TryGetCurrentKeys(out List<ComboKey> keys)
        {
            keys = null;
            var seq = GetCurrentSequence();
            if (seq == null) return false;               // ไม่มีเมนู/สเต็ปปัจจุบัน

            keys = seq.Count == 0
                ? new List<ComboKey>()                   // มีสเต็ปแต่ไม่มีคีย์
                : seq.ConvertAll(c => c.key);
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

