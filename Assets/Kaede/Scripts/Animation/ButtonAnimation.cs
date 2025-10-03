using Kaede.Scripts.Item;
using Kaede.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

public class ButtonAnimation : MonoBehaviour
{
    public YuirinSlideBar slideBar;
    
    public void SetBlockType(ComboType type)
    {
        if (type == ComboType.Hold)
        {
            slideBar = GetComponent<YuirinSlideBar>(); 
            Debug.Log("Getting Yuirin Component");
        }
    }
} 
