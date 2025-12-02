using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace Kaede.Scripts.Utils
{
    public enum InputMode
    {
        KeyboardMouse,
        Gamepad
    }

    public class ControllerCheck : MonoBehaviour
    {
        private IDisposable _anyButtonPressListener;

        public InputMode CurrentInputMode { get; private set; } = InputMode.KeyboardMouse;

        public event Action<InputMode> InputModeChanged;
        
        private void OnEnable()
        {
            _anyButtonPressListener = InputSystem.onAnyButtonPress.Call(OnAnyButton);
        }
        
        private void OnDisable()
        {
            _anyButtonPressListener?.Dispose();
        }
        
        public async UniTask DelaySelect(GameObject toSelect = null)
        {
            if (EventSystem.current == null || toSelect == null) return;
            await UniTask.Yield();
            EventSystem.current.SetSelectedGameObject(toSelect);
        }
        
        private void OnAnyButton(InputControl control)
        {
            if (control.device is Gamepad)
            {
                Debug.Log("Controller is active");
                SetInputMode(InputMode.Gamepad);
            }
            else if (control.device is Keyboard || control.device is Mouse)
            {
                Debug.Log("Keyboard/Mouse is active");
                SetInputMode(InputMode.KeyboardMouse);
            }
        }

        private void SetInputMode(InputMode newMode)
        {
            if (CurrentInputMode == newMode) return;

            CurrentInputMode = newMode;
            InputModeChanged?.Invoke(CurrentInputMode);

            if (EventSystem.current == null) return;

            if (CurrentInputMode == InputMode.Gamepad && EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(EventSystem.current.firstSelectedGameObject);
            }
            else if (CurrentInputMode == InputMode.KeyboardMouse)
            {
                EventSystem.current.SetSelectedGameObject(null);
            }
        }
    }
}