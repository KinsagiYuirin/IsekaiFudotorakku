using System;
using Cysharp.Threading.Tasks;
using Kaede.Scripts.Item;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.Serialization;

namespace MadDuck.Scripts.Inputs
{
    /// <summary>
    /// Handle player input. Required in the same object as CharacterHub with Player character type.
    /// </summary>
    public class PlayerInputHandler : MonoBehaviour, PlayerInputAction.IPlayerActions
    {
        #region Data Structures
        [Serializable]
        public record InputButton(InputAction InputAction)
        {
            private InputAction InputAction { get; set; } = InputAction;
            [ShowInInspector, DisplayAsString] 
            public string ButtonName =>
                InputAction != null 
                    ? InputAction.GetBindingDisplayString(InputBinding.DisplayStringOptions.DontIncludeInteractions) 
                    : string.Empty;
            public bool isDown;
            public bool isUp;
            public bool isHeld;
            public bool isUpAfterHeld;
            /// <summary>
            /// Warning: Use isUpAfterHeld if you want to check if the button is released after being held. This property is only for input handler.
            /// </summary>
            public bool heldLastTime;
            public InputBinding? inputBinding;
        }
        #endregion

        #region Inspector

        #region Values
        [field: SerializeField, ReadOnly] public bool AnyButtonPressed { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2 MovementInput { get; private set; }
        [field: SerializeField, ReadOnly] public Vector2 MouseDelta { get; private set; }
        [field: SerializeField, ReadOnly] public float BoatInput { get; private set; }
        

        #endregion
        
        #region Buttons
        [field: SerializeField, ReadOnly] 
        public SerializableReactiveProperty<InputButton> InteractButton { get; private set; }
        [field: SerializeField, ReadOnly] 
        public SerializableReactiveProperty<InputButton> JerkBaitButton { get; private set; }
        [field: SerializeField, ReadOnly] 
        public InputBinding[] JerkBindings { get; private set; }
        [field: SerializeField, ReadOnly] 
        public SerializableReactiveProperty<InputButton> LeftMouseClick { get; private set; }
        [field: SerializeField, ReadOnly] 
        public SerializableReactiveProperty<InputButton> RightMouseClick { get; private set; }
        [field: SerializeField, ReadOnly] 
        public SerializableReactiveProperty<InputButton> ReelingButton { get; private set; }
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> PauseGameButton { get; private set; }
        
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> ComboUpButton { get; private set; }
        
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> ComboDownButton { get; private set; }
        
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> ComboLeftButton { get; private set; }
        
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> ComboRightButton { get; private set; }
        
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> ComboArrowUp { get; private set; }
        
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> ComboArrowDown { get; private set; }
        
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> ComboArrowLeft { get; private set; }
        
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> ComboArrowRight { get; private set; }
        
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> ConfirmButton { get; private set; }
        [field: SerializeField, ReadOnly]
        public SerializableReactiveProperty<InputButton> CancelButton { get; private set; }
        
        #endregion
        
        #endregion

        #region Fields
        private PlayerInputAction _playerInputAction;
        private IDisposable _anyButtonPressListener;
        private bool _comboInputEnabled = true;
        #endregion

        #region Life Cycle
        private void OnEnable()
        {
            Subscribe();
            RegisterInputAction();
        }
        
        private void OnDisable()
        {
            Unsubscribe();
        }

        private void RegisterInputAction()
        {
            InteractButton.Value = new InputButton(_playerInputAction.Player.Interact);
            LeftMouseClick.Value = new InputButton(_playerInputAction.Player.MeleeAttack);
            RightMouseClick.Value = new InputButton(_playerInputAction.Player.RangeAttack);
            PauseGameButton.Value = new InputButton(_playerInputAction.Player.Pause);
            ComboUpButton.Value = new InputButton(_playerInputAction.Player.ComboUp);
            ComboDownButton.Value = new InputButton(_playerInputAction.Player.ComboDown);
            ComboLeftButton.Value = new InputButton(_playerInputAction.Player.ComboLeft);
            ComboRightButton.Value = new InputButton(_playerInputAction.Player.ComboRight);
            ComboArrowUp.Value = new InputButton(_playerInputAction.Player.ComboArrowUp);
            ComboArrowDown.Value = new InputButton(_playerInputAction.Player.ComboArrowDown);
            ComboArrowLeft.Value = new InputButton(_playerInputAction.Player.ComboArrowLeft);
            ComboArrowRight.Value = new InputButton(_playerInputAction.Player.ComboArrowRight);
            CancelButton.Value = new InputButton(_playerInputAction.Player.CancelButton);
            ConfirmButton.Value = new InputButton(_playerInputAction.Player.ConfirmButton);
            
            /*JerkBaitButton.Value = new InputButton(_playerInputAction.Player.JerkBait);
            ReelingButton.Value = new InputButton(_playerInputAction.Player.Reeling);
            JerkBindings = _playerInputAction.Player.JerkBait.bindings.ToArray();*/
        }
        #endregion

        #region Subscriptions

        private void Subscribe()
        {
            if (_playerInputAction == null)
            {
                _playerInputAction = new PlayerInputAction();
                _playerInputAction.Player.SetCallbacks(this);
            }
            _playerInputAction.Player.Enable();
            _anyButtonPressListener = InputSystem.onAnyButtonPress.Call(OnAnyButton);
        }
    
        private void Unsubscribe()
        {
            _playerInputAction.Player.Disable();
            _anyButtonPressListener?.Dispose();
        }
        #endregion

        #region Event Handlers

        private async void OnAnyButton(InputControl inputControl)
        {
            AnyButtonPressed = true;
            await UniTask.WaitForEndOfFrame();
            AnyButtonPressed = false;
        }
        
        public void OnMove(InputAction.CallbackContext context)
        {
            MovementInput = context.ReadValue<Vector2>();
        }

        public void OnMeleeAttack(InputAction.CallbackContext context)
        {
            BindPressButton(LeftMouseClick, context);
        }

        public void OnRangeAttack(InputAction.CallbackContext context)
        {
            BindPressButton(RightMouseClick, context);
        }

        /*public void OnControlBoat(InputAction.CallbackContext context)
        {
            if (context.performed) 
            {
                float input = context.ReadValue<float>();
                BoatInput = input;
            }
            else if (context.canceled)
            {
                BoatInput = 0f;
            }
            
        }*/

        public void OnPause(InputAction.CallbackContext context)
        {
            BindPressButton(PauseGameButton, context);
        }
        
        public void OnInteract(InputAction.CallbackContext context)
        {
            BindPressButton(InteractButton, context);
        }

        public void OnComboUp(InputAction.CallbackContext context)
        {
            if (!CanProcessComboInput())
            {
                return;
            }
            BindPressButton(ComboUpButton, context);
        }
        
        public void OnComboDown(InputAction.CallbackContext context)
        {
            if (!CanProcessComboInput())
            {
                return;
            }
            BindPressButton(ComboDownButton, context);
        }

        public void OnComboLeft(InputAction.CallbackContext context)
        {
            if (!CanProcessComboInput())
            {
                return;
            }
            BindPressButton(ComboLeftButton, context);
        }
        
        public void OnComboRight(InputAction.CallbackContext context)
        {
            if (!CanProcessComboInput())
            {
                return;
            }
            BindPressButton(ComboRightButton, context);
        }
        
        public void OnComboArrowUp(InputAction.CallbackContext context)
        {
            if (!CanProcessComboInput())
            {
                return;
            }
            BindPressButton(ComboArrowUp, context);
        }
        
        public void OnComboArrowDown(InputAction.CallbackContext context)
        {
            if (!CanProcessComboInput())
            {
                return;
            }
            BindPressButton(ComboArrowDown, context);
        }
        
        public void OnComboArrowLeft(InputAction.CallbackContext context)
        {
            if (!CanProcessComboInput())
            {
                return;
            }
            BindPressButton(ComboArrowLeft, context);
        }
        
        public void OnComboArrowRight(InputAction.CallbackContext context)
        {
            if (!CanProcessComboInput())
            {
                return;
            }
            BindPressButton(ComboArrowRight, context);
        }

        public void OnConfirmButton(InputAction.CallbackContext context)
        {
            BindPressButton(ConfirmButton, context);
        }

        public void OnCancelButton(InputAction.CallbackContext context)
        {
            BindPressButton(CancelButton, context);
        }

        #endregion

        #region Button
        private void BindPressButton(ReactiveProperty<InputButton> button, InputAction.CallbackContext context)
        {
            button.Value.isDown = context.performed;
            button.Value.isUp = context.canceled;
            button.Value.isHeld = context.performed;
            button.Value.isUpAfterHeld = context.canceled;
            button.Value.heldLastTime = context.performed;
            button.Value.inputBinding = context.action.GetBindingForControl(context.control);
            button.OnNext(button.Value);
            ButtonPressTask(button).Forget();
        }

        private async UniTaskVoid ButtonPressTask(ReactiveProperty<InputButton> button)
        {
            await UniTask.WaitForEndOfFrame();
            button.Value.isDown = false;
            if (!button.Value.isHeld)
            {
                button.Value.isUp = false;
                button.Value.isUpAfterHeld = false;
            }
            button.OnNext(button.Value);
        }
        
        private void BindHoldButton(ReactiveProperty<InputButton> button, InputAction.CallbackContext context)
        {
            button.Value.inputBinding = context.action.GetBindingForControl(context.control);
            switch (context)
            {
                case { started: true, performed: false }:
                    button.Value.isDown = true;
                    button.Value.isHeld = false;
                    button.Value.isUp = false;
                    button.Value.isUpAfterHeld = false;
                    button.Value.heldLastTime = false;
                    button.OnNext(button.Value);
                    ButtonPressTask(button).Forget();
                    break;
                case { performed: true }:
                    button.Value.isDown = false;
                    button.Value.isHeld = true;
                    button.Value.isUp = false;
                    button.Value.isUpAfterHeld = false;
                    button.Value.heldLastTime = true;
                    button.OnNext(button.Value);
                    break;
                case { canceled: true }:
                    button.Value.isDown = false;
                    button.Value.isHeld = false;
                    button.Value.isUp = true;
                    button.Value.isUpAfterHeld = button.Value.heldLastTime;
                    button.Value.heldLastTime = false;
                    button.OnNext(button.Value);
                    ButtonPressTask(button).Forget();
                    break;
            }
        }
        #endregion
        
        #region Utils
        public void SetBoatInput(float input)
        {
            BoatInput = input;
        }
        #endregion

        #region Utils(by Yuirin)

        public void SetComboInputEnabled(bool enabled)
        {
            if (_comboInputEnabled == enabled)
            {
                return;
            }

            _comboInputEnabled = enabled;

            if (!enabled)
            {
                ResetComboInputState();
            }
        }

        private bool CanProcessComboInput()
        {
            return _comboInputEnabled;
        }

        private void ResetComboInputState()
        {
            ResetComboButtonState(ComboUpButton);
            ResetComboButtonState(ComboDownButton);
            ResetComboButtonState(ComboLeftButton);
            ResetComboButtonState(ComboRightButton);
            ResetComboButtonState(ComboArrowUp);
            ResetComboButtonState(ComboArrowDown);
            ResetComboButtonState(ComboArrowLeft);
            ResetComboButtonState(ComboArrowRight);
        }

        private static void ResetComboButtonState(ReactiveProperty<InputButton> button)
        {
            if (button?.Value == null)
            {
                return;
            }

            button.Value.isDown = false;
            button.Value.isUp = false;
            button.Value.isHeld = false;
            button.Value.isUpAfterHeld = false;
            button.Value.heldLastTime = false;
            button.OnNext(button.Value);
        }

        #endregion
        
        #region Helper(by Yuirin)
        
        public bool IsKeyDown(ComboKey key) => key switch
        {
            ComboKey.W => ComboUpButton?.Value.isDown == true,
            ComboKey.S => ComboDownButton?.Value.isDown == true,
            ComboKey.A => ComboLeftButton?.Value.isDown == true,
            ComboKey.D => ComboRightButton?.Value.isDown == true,
            ComboKey.Up => ComboArrowUp?.Value.isDown == true,
            ComboKey.Down => ComboArrowDown?.Value.isDown == true,
            ComboKey.Left => ComboArrowLeft?.Value.isDown == true,
            ComboKey.Right => ComboArrowRight?.Value.isDown == true,
            _ => false
        };

        public bool IsKeyHeld(ComboKey key) => key switch
        {
            ComboKey.W => ComboUpButton?.Value.isHeld == true,
            ComboKey.S => ComboDownButton?.Value.isHeld == true,
            ComboKey.A => ComboLeftButton?.Value.isHeld == true,
            ComboKey.D => ComboRightButton?.Value.isHeld == true,
            ComboKey.Up => ComboArrowUp?.Value.isHeld == true,
            ComboKey.Down => ComboArrowDown?.Value.isHeld == true,
            ComboKey.Left => ComboArrowLeft?.Value.isHeld == true,
            ComboKey.Right => ComboArrowRight?.Value.isHeld == true,
            _ => false
        };

        public bool IsKeyUp(ComboKey key) => key switch
        {
            ComboKey.W => ComboUpButton?.Value.isUp == true,
            ComboKey.S => ComboDownButton?.Value.isUp == true,
            ComboKey.A => ComboLeftButton?.Value.isUp == true,
            ComboKey.D => ComboRightButton?.Value.isUp == true,
            ComboKey.Up => ComboArrowUp?.Value.isUp == true,
            ComboKey.Down => ComboArrowDown?.Value.isUp == true,
            ComboKey.Left => ComboArrowLeft?.Value.isUp == true,
            ComboKey.Right => ComboArrowRight?.Value.isUp == true,
            _ => false
        };

        public bool AnyOtherKeyDown(ComboKey expectedKey)
        {
            foreach (ComboKey key in Enum.GetValues(typeof(ComboKey)))
            {
                if (key == ComboKey.None || key == expectedKey)
                    continue;

                if (IsKeyDown(key))
                    return true;
            }

            return false;
        }
        
        #endregion
    }
}
