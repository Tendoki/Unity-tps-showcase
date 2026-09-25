using UnityEngine;

namespace Game.Player.ThirdPerson.Animation
{
    public class CharacterVisualSmoother : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform visualRoot;

        [Header("Smoothing")]
        [SerializeField] private float verticalSmoothing = 18f;
        [SerializeField] private float maxVerticalOffset = 0.35f;

        private Vector3 _previousRootPosition;
        private Vector3 _initialLocalPosition;
        private float _verticalOffset;

        private void Awake()
        {
            if (visualRoot == null)
            {
                enabled = false;
                return;
            }

            _previousRootPosition = transform.position;
            _initialLocalPosition = visualRoot.localPosition;
        }

        private void LateUpdate()
        {
            float rootDeltaY = transform.position.y - _previousRootPosition.y;

            _verticalOffset -= rootDeltaY;
            _verticalOffset = Mathf.Clamp(_verticalOffset, -maxVerticalOffset, maxVerticalOffset);

            float t = 1f - Mathf.Exp(-verticalSmoothing * Time.deltaTime);
            _verticalOffset = Mathf.Lerp(_verticalOffset, 0f, t);

            visualRoot.localPosition = _initialLocalPosition + Vector3.up * _verticalOffset;
            _previousRootPosition = transform.position;
        }
    }
}
