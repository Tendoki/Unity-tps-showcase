using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;
using UnityEngine.UI;

namespace Game.Scripts.Services.Input
{
    [RequireComponent(typeof(Button))]
    public class MobileOnScreenButton : OnScreenControl, IPointerDownHandler, IPointerUpHandler
    {
        [InputControl(layout = "Button")]
        [SerializeField] private string controlPath = "<Gamepad>/buttonSouth";

        private readonly HashSet<int> _pressedPointers = new();
        private Button _button;

        protected override string controlPathInternal
        {
            get => controlPath;
            set => controlPath = value;
        }

        private void Awake()
        {
            _button = GetComponent<Button>();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Release();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_button.interactable)
            {
                return;
            }

            _pressedPointers.Add(eventData.pointerId);
            SendValueToControl(1f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressedPointers.Remove(eventData.pointerId);

            if (_pressedPointers.Count > 0)
            {
                return;
            }

            Release();
        }

        private void Release()
        {
            _pressedPointers.Clear();
            SendValueToControl(0f);
        }
    }
}
