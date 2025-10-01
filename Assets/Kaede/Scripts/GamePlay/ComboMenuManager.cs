using System;
using System.Collections.Generic;
using System.Linq;
using Kaede.Scripts.Item;
using Kaede.Scripts.Managers;

namespace Kaede.Scripts.GamePlay
{
    internal sealed class ComboMenuManager
    {
        private readonly Func<RandomSystem> _randomSystemProvider;

        public ComboMenuManager(Func<RandomSystem> randomSystemProvider)
        {
            _randomSystemProvider = randomSystemProvider;
        }

        public IReadOnlyList<MenuData> CurrentMenus { get; private set; } = new List<MenuData>();

        public bool Initialize(bool forceRegenerate)
        {
            return TryApplyMenus(forceRegenerate, false);
        }

        public bool MoveToNextMenuType()
        {
            return TryApplyMenus(false, true);
        }

        private bool TryApplyMenus(bool forceRegenerate, bool advanceToNextType)
        {
            var randomSystem = _randomSystemProvider?.Invoke();
            if (randomSystem == null)
            {
                return false;
            }

            List<MenuData> menus;
            if (advanceToNextType)
            {
                menus = randomSystem.MoveToNextMenuSetForCombo();
            }
            else
            {
                menus = randomSystem.GetMenuSetForCombo(forceRegenerate);
            }

            if (menus == null || menus.Count == 0)
            {
                return false;
            }

            CurrentMenus = menus.Where(menu => menu != null).ToList();
            return true;
        }
    }
}