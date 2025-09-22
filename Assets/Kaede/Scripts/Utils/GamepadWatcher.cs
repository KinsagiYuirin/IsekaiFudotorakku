using UnityEngine;
using UnityEngine.InputSystem;

namespace Kaede.Scripts.Utils
{
    public class GamepadWatcher : MonoBehaviour
    {
        void OnEnable()  => InputSystem.onDeviceChange += OnDeviceChange;
        void OnDisable() => InputSystem.onDeviceChange -= OnDeviceChange;

        private void OnDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (!(device is Gamepad) && !(device is Joystick)) return;

            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    Debug.Log($"Connected: {device.displayName}");
                    break;

                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Removed:
                    Debug.Log($"Disconnected: {device.displayName}");
                    break;
            }
        }
    }
}
