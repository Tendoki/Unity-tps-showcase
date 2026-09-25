using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

namespace Game.Scripts.Services.Input
{
    public class MobileOnScreenStick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private const int NoPointer = int.MinValue;

        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform thumb;
        [SerializeField] private float movementRange = 80f;
        [InputControl(layout = "Vector2")]
        [SerializeField] private string controlPath = "<Gamepad>/leftStick";

        private RectTransform _area;
        private int _activePointerId = NoPointer;
        private Vector2 _originLocalPosition;

        protected override string controlPathInternal
        {
            get => controlPath;
            set => controlPath = value;
        }

        private void Awake()
        {
            _area = (RectTransform)transform;
            Hide();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_activePointerId != NoPointer)
            {
                return;
            }

            if (!TryGetLocalPoint(eventData, out _originLocalPosition))
            {
                return;
            }

            _activePointerId = eventData.pointerId;

            background.gameObject.SetActive(true);
            background.anchoredPosition = _originLocalPosition;
            thumb.anchoredPosition = Vector2.zero;

            UpdateStick(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_activePointerId != eventData.pointerId)
            {
                return;
            }

            UpdateStick(eventData);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_activePointerId != eventData.pointerId)
            {
                return;
            }

            _activePointerId = NoPointer;
            SendValueToControl(Vector2.zero);
            thumb.anchoredPosition = Vector2.zero;
            Hide();
        }

        private void UpdateStick(PointerEventData eventData)
        {
            if (!TryGetLocalPoint(eventData, out Vector2 pointerLocalPosition))
            {
                return;
            }

            Vector2 delta = pointerLocalPosition - _originLocalPosition;
            Vector2 clampedDelta = Vector2.ClampMagnitude(delta, movementRange);
            Vector2 value = clampedDelta / movementRange;

            thumb.anchoredPosition = clampedDelta;
            SendValueToControl(value);
        }

        private bool TryGetLocalPoint(PointerEventData eventData, out Vector2 localPoint)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _area,
                eventData.position,
                eventData.pressEventCamera,
                out localPoint);
        }

        private void Hide()
        {
            if (background != null)
            {
                background.gameObject.SetActive(false);
            }
        }
    }
}
