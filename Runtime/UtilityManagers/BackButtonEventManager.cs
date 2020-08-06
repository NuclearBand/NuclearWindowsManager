using System;
using UnityEngine;

namespace NuclearBand
{
    public class BackButtonEventManager : MonoBehaviour
    {
        public event Action? OnBackButtonPressed;
        public static BackButtonEventManager Instance { get; private set; } = null!;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape) )
                OnBackButtonPressed?.Invoke();
        }
    }
}