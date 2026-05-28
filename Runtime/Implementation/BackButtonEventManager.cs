#nullable enable
using System;
using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace NuclearBand
{
    public class BackButtonEventManager : MonoBehaviour
    {
        public event Action OnBackButtonPressed = delegate { };

        private void Update()
        {
            if(IsBackButtonPressed())
                OnBackButtonPressed.Invoke();
        }

        private static bool IsBackButtonPressed()
        {
#if ENABLE_INPUT_SYSTEM
            return Keyboard.current?.escapeKey.wasPressedThisFrame == true;
#else
            return Input.GetKeyDown(KeyCode.Escape);
#endif
        }
    }
}
