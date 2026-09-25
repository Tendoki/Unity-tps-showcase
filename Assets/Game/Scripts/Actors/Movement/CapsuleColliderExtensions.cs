using UnityEngine;

namespace Game.Actors.Movement
{
    public static class CapsuleColliderExtensions
    {
        public static void GetWorldPoints(
            this Transform transform,
            float height,
            Vector3 center,
            float localRadius,
            out Vector3 lowerSphereCenter,
            out Vector3 upperSphereCenter,
            out float radius)
        {
            Vector3 worldCenter = transform.TransformPoint(center);
            Vector3 scale = transform.lossyScale;
            float horizontalScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.z));
            radius = localRadius * horizontalScale;
            float worldHeight = Mathf.Max(height * Mathf.Abs(scale.y), radius * 2f);
            float segmentHalfHeight = worldHeight * 0.5f - radius;
            Vector3 up = transform.up;
            upperSphereCenter = worldCenter + up * segmentHalfHeight;
            lowerSphereCenter = worldCenter - up * segmentHalfHeight;
        }
    }
}
