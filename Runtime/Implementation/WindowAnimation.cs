using System;
using UnityEngine;

namespace Nuclear.WindowsManager
{
    public abstract class WindowAnimation : MonoBehaviour
    {
        public event Action OnEnd = delegate { };

        public abstract void Play();

        public void End() => OnEnd.Invoke();
    }
}