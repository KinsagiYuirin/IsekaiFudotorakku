using System.Reflection;
using Kaede.Scripts.Item;
using UnityEngine;

namespace Kaede.Scripts.Utils
{
    public static class FoodTypeExtensions
    {
        public static string ToNiceString(this FoodType ft)
        {
            var member = typeof(FoodType).GetMember(ft.ToString());
            if (member.Length > 0)
            {
                var attr = member[0].GetCustomAttribute<InspectorNameAttribute>();
                if (attr != null)
                    return attr.displayName; // คืนค่าชื่อสวย ๆ เช่น "Main Course"
            }
            return ft.ToString(); // fallback
        }
    }
}