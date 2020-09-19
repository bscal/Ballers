// GENERATED AUTOMATICALLY FROM 'Assets/Scripts/Controls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @Controls : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @Controls()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""Controls"",
    ""maps"": [
        {
            ""name"": ""Keyboard"",
            ""id"": ""40eb852a-53c8-44ca-8bb5-5523b55c564e"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""3b9d99b2-f70d-4200-ba54-1c13da1b2a31"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Shoot"",
                    ""type"": ""Value"",
                    ""id"": ""50e3957e-782e-4c6e-9349-d842fdb7dfca"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Dribble"",
                    ""type"": ""Button"",
                    ""id"": ""c7340167-232f-4f5b-954a-80744104e611"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pass_1"",
                    ""type"": ""Value"",
                    ""id"": ""90b075b4-71a4-466b-8aee-f9666b32e4fd"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pass_2"",
                    ""type"": ""Value"",
                    ""id"": ""369fa894-0ce4-4bff-a0e0-868281282a8e"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pass_3"",
                    ""type"": ""Value"",
                    ""id"": ""11164195-d619-4ee8-a7ca-9894b56db4bb"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pass_4"",
                    ""type"": ""Value"",
                    ""id"": ""20433785-3a7f-4a07-9a62-c12d0614bc2b"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Pass_5"",
                    ""type"": ""Value"",
                    ""id"": ""b6832ef8-83a4-4e6c-b84f-f5515b2052bd"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Call for ball"",
                    ""type"": ""Button"",
                    ""id"": ""3c03d5e4-4f7c-4e32-9287-bdc6ff17bbf8"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""b82107e0-f0c5-4afb-b07d-a88ec042cc75"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""93d9c19c-23bf-469e-9313-8e7626aed351"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""7bcb3a3b-e697-4122-b052-e0f8e212be99"",
                    ""path"": ""<Keyboard>/upArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""d8ca6993-73c1-4561-942a-27f23a3a44c4"",
                    ""path"": ""<Keyboard>/downArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""93f1586e-2faa-4092-91ae-0823c7227d0f"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""ea6059a9-b518-4e47-9cbb-630e343c00fe"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""a28d9e71-7297-4792-b543-ca56cd24f4b5"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Shoot"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""75f2bf06-b079-4394-b2e4-c7b6a1ebd17c"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Dribble"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""fd2e252a-f10a-48df-91fe-568985975449"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Dribble"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""14c2a8d9-d382-4b44-ab7d-54e0920350ed"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Dribble"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""79516236-57c6-4bc4-8774-9410380d81cb"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Dribble"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""cf8cef45-09b6-464b-b154-e3f1461995af"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Dribble"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""2eb58bbf-e082-4cc5-9111-d3e60dfeb18f"",
                    ""path"": ""<Keyboard>/1"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Pass_1"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""5016b14c-55f6-441e-935b-0d7a3e5fffca"",
                    ""path"": ""<Keyboard>/2"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Pass_2"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""14b7bbde-2778-4c77-86ba-cbbbfcc8a85d"",
                    ""path"": ""<Keyboard>/3"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Pass_3"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9fe28054-b672-45ab-99d0-160c3483eb10"",
                    ""path"": ""<Keyboard>/4"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Pass_4"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a68d058f-2735-4ba9-b47c-4b29ad232626"",
                    ""path"": ""<Keyboard>/5"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Pass_5"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""14c2f759-785e-4e9e-890c-3bbb60f23a16"",
                    ""path"": ""<Keyboard>/6"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard"",
                    ""action"": ""Call for ball"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""09661b1d-f227-4c02-b08a-0856fb8d8454"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard"",
            ""bindingGroup"": ""Keyboard"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Keyboard
        m_Keyboard = asset.FindActionMap("Keyboard", throwIfNotFound: true);
        m_Keyboard_Move = m_Keyboard.FindAction("Move", throwIfNotFound: true);
        m_Keyboard_Shoot = m_Keyboard.FindAction("Shoot", throwIfNotFound: true);
        m_Keyboard_Dribble = m_Keyboard.FindAction("Dribble", throwIfNotFound: true);
        m_Keyboard_Pass_1 = m_Keyboard.FindAction("Pass_1", throwIfNotFound: true);
        m_Keyboard_Pass_2 = m_Keyboard.FindAction("Pass_2", throwIfNotFound: true);
        m_Keyboard_Pass_3 = m_Keyboard.FindAction("Pass_3", throwIfNotFound: true);
        m_Keyboard_Pass_4 = m_Keyboard.FindAction("Pass_4", throwIfNotFound: true);
        m_Keyboard_Pass_5 = m_Keyboard.FindAction("Pass_5", throwIfNotFound: true);
        m_Keyboard_Callforball = m_Keyboard.FindAction("Call for ball", throwIfNotFound: true);
        m_Keyboard_Jump = m_Keyboard.FindAction("Jump", throwIfNotFound: true);
    }

    public void Dispose()
    {
        UnityEngine.Object.Destroy(asset);
    }

    public InputBinding? bindingMask
    {
        get => asset.bindingMask;
        set => asset.bindingMask = value;
    }

    public ReadOnlyArray<InputDevice>? devices
    {
        get => asset.devices;
        set => asset.devices = value;
    }

    public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

    public bool Contains(InputAction action)
    {
        return asset.Contains(action);
    }

    public IEnumerator<InputAction> GetEnumerator()
    {
        return asset.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Enable()
    {
        asset.Enable();
    }

    public void Disable()
    {
        asset.Disable();
    }

    // Keyboard
    private readonly InputActionMap m_Keyboard;
    private IKeyboardActions m_KeyboardActionsCallbackInterface;
    private readonly InputAction m_Keyboard_Move;
    private readonly InputAction m_Keyboard_Shoot;
    private readonly InputAction m_Keyboard_Dribble;
    private readonly InputAction m_Keyboard_Pass_1;
    private readonly InputAction m_Keyboard_Pass_2;
    private readonly InputAction m_Keyboard_Pass_3;
    private readonly InputAction m_Keyboard_Pass_4;
    private readonly InputAction m_Keyboard_Pass_5;
    private readonly InputAction m_Keyboard_Callforball;
    private readonly InputAction m_Keyboard_Jump;
    public struct KeyboardActions
    {
        private @Controls m_Wrapper;
        public KeyboardActions(@Controls wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Keyboard_Move;
        public InputAction @Shoot => m_Wrapper.m_Keyboard_Shoot;
        public InputAction @Dribble => m_Wrapper.m_Keyboard_Dribble;
        public InputAction @Pass_1 => m_Wrapper.m_Keyboard_Pass_1;
        public InputAction @Pass_2 => m_Wrapper.m_Keyboard_Pass_2;
        public InputAction @Pass_3 => m_Wrapper.m_Keyboard_Pass_3;
        public InputAction @Pass_4 => m_Wrapper.m_Keyboard_Pass_4;
        public InputAction @Pass_5 => m_Wrapper.m_Keyboard_Pass_5;
        public InputAction @Callforball => m_Wrapper.m_Keyboard_Callforball;
        public InputAction @Jump => m_Wrapper.m_Keyboard_Jump;
        public InputActionMap Get() { return m_Wrapper.m_Keyboard; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(KeyboardActions set) { return set.Get(); }
        public void SetCallbacks(IKeyboardActions instance)
        {
            if (m_Wrapper.m_KeyboardActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnMove;
                @Shoot.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnShoot;
                @Shoot.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnShoot;
                @Shoot.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnShoot;
                @Dribble.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnDribble;
                @Dribble.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnDribble;
                @Dribble.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnDribble;
                @Pass_1.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_1;
                @Pass_1.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_1;
                @Pass_1.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_1;
                @Pass_2.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_2;
                @Pass_2.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_2;
                @Pass_2.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_2;
                @Pass_3.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_3;
                @Pass_3.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_3;
                @Pass_3.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_3;
                @Pass_4.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_4;
                @Pass_4.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_4;
                @Pass_4.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_4;
                @Pass_5.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_5;
                @Pass_5.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_5;
                @Pass_5.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnPass_5;
                @Callforball.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnCallforball;
                @Callforball.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnCallforball;
                @Callforball.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnCallforball;
                @Jump.started -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_KeyboardActionsCallbackInterface.OnJump;
            }
            m_Wrapper.m_KeyboardActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Shoot.started += instance.OnShoot;
                @Shoot.performed += instance.OnShoot;
                @Shoot.canceled += instance.OnShoot;
                @Dribble.started += instance.OnDribble;
                @Dribble.performed += instance.OnDribble;
                @Dribble.canceled += instance.OnDribble;
                @Pass_1.started += instance.OnPass_1;
                @Pass_1.performed += instance.OnPass_1;
                @Pass_1.canceled += instance.OnPass_1;
                @Pass_2.started += instance.OnPass_2;
                @Pass_2.performed += instance.OnPass_2;
                @Pass_2.canceled += instance.OnPass_2;
                @Pass_3.started += instance.OnPass_3;
                @Pass_3.performed += instance.OnPass_3;
                @Pass_3.canceled += instance.OnPass_3;
                @Pass_4.started += instance.OnPass_4;
                @Pass_4.performed += instance.OnPass_4;
                @Pass_4.canceled += instance.OnPass_4;
                @Pass_5.started += instance.OnPass_5;
                @Pass_5.performed += instance.OnPass_5;
                @Pass_5.canceled += instance.OnPass_5;
                @Callforball.started += instance.OnCallforball;
                @Callforball.performed += instance.OnCallforball;
                @Callforball.canceled += instance.OnCallforball;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
            }
        }
    }
    public KeyboardActions @Keyboard => new KeyboardActions(this);
    private int m_KeyboardSchemeIndex = -1;
    public InputControlScheme KeyboardScheme
    {
        get
        {
            if (m_KeyboardSchemeIndex == -1) m_KeyboardSchemeIndex = asset.FindControlSchemeIndex("Keyboard");
            return asset.controlSchemes[m_KeyboardSchemeIndex];
        }
    }
    public interface IKeyboardActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnShoot(InputAction.CallbackContext context);
        void OnDribble(InputAction.CallbackContext context);
        void OnPass_1(InputAction.CallbackContext context);
        void OnPass_2(InputAction.CallbackContext context);
        void OnPass_3(InputAction.CallbackContext context);
        void OnPass_4(InputAction.CallbackContext context);
        void OnPass_5(InputAction.CallbackContext context);
        void OnCallforball(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
    }
}
