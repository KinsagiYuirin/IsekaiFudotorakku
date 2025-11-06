using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace Kaede.Scripts.UI.TodayMenu
{
    public class TodayManuUI : MonoBehaviour
    {
        [Title("Settings")]
        [SerializeField] private Image bgImage;
        [SerializeField] private TodayMenuPrefab[] menuPrefabs;
    }
}
