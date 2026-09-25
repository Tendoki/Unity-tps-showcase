using UnityEngine;

namespace Game.Player
{
    public interface IHeldItemAnimationRig
    {
        void SetLeftHandIkTarget(Transform target, float weight);
        void ClearLeftHandIk();
    }
}
