#nullable enable
using System;
using UnityEngine;

namespace NuclearBand
{
    public class BackButtonEventManager : MonoBehaviour
    {
        public event Action OnBackButtonPressed = delegate { };

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape) )
                OnBackButtonPressed.Invoke();
        }
    }
}
