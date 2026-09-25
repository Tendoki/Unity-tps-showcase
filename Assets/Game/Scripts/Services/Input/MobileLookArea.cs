using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Game.Scripts.Services.Input
{
    public class MobileLookArea : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const int NoPointer = int.MinValue;

        [SerializeField] private float sensitivity = 0.05f;

        [InputControl(layout = "Vector2")]
        [SerializeField] private string controlPath = "<Gamepad>/rightStick";

        private static Vector2 _accumulatedLookDelta;

        private int _activePointerId = NoPointer;

        protected override string controlPathInternal
        {
            get => controlPath;
            set => controlPath = value;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_activePointerId != NoPointer)
            {
                return;
            }

            _activePointerId = eventData.pointerId;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_activePointerId != eventData.pointerId)
            {
                return;
            }

            Vector2 lookDelta = eventData.delta * sensitivity;
            _accumulatedLookDelta += lookDelta;
            SendValueToControl(lookDelta);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_activePointerId != eventData.pointerId)
            {
                return;
            }

            _activePointerId = NoPointer;
            SendValueToControl(Vector2.zero);
        }

        public static Vector2 ConsumeLookDelta()
        {
            Vector2 lookDelta = _accumulatedLookDelta;
            _accumulatedLookDelta = Vector2.zero;

            return lookDelta;
        }
    }
}
