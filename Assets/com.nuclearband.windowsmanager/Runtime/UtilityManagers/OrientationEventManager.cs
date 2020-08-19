#nullable enable
using System;
using UnityEngine;

namespace NuclearBand
{
    public class OrientationEventManager : MonoBehaviour
    {
        public static OrientationEventManager Instance { get; private set; } = null!;

        private enum DeviceScreenOrientation
        {
            Landscape,
            Portrait
        }

        public event Action? OnOrientationChanged;
        private DeviceScreenOrientation orientation;
#if UNITY_EDITOR
        private int prevWidth;
        private int prevHeight;
#endif

        private void Awake()
        {
            Instance = this;
            orientation = CalculateOrientation();
#if UNITY_EDITOR
            prevWidth = Screen.width;
            prevHeight = Screen.height;
#endif
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (Screen.width != prevWidth || Screen.height != prevHeight)
            {
                prevWidth = Screen.width;
                prevHeight = Screen.height;
                OnOrientationChanged?.Invoke();
            }

            return;
#endif
#pragma warning disable CS0162
            var curOrientation = CalculateOrientation();
            if (curOrientation != orientation)
            {
                orientation = curOrientation;
                OnOrientationChanged?.Invoke();
            }
#pragma warning restore CS0162
        }

        private DeviceScreenOrientation CalculateOrientation()
        {
            if (Screen.width >= Screen.height)
                return DeviceScreenOrientation.Landscape;
            else
                return DeviceScreenOrientation.Portrait;
        }
    }
}
