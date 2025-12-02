using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class ControllerCheck : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    private void OnAnyButton(InputControl control)
    {
        if (control.device is Gamepad)
        {
            Debug.Log("Controller is active");
            SetInputMode(StandaloneInputModule.InputMode.Gamepad);
        }
        else if (control.device is Keyboard || control.device is Mouse)
        {
            Debug.Log("Keyboard/Mouse is active");
            SetInputMode(StandaloneInputModule.InputMode.KeyboardMouse);
        }
    }
}
