using UnityEngine;

namespace Impulse.Interfaces
{
    public interface IShake
    {
        bool IsFinished { get; }

        IShake Initialize(Transform objectTransform);
        Displacement Update(float deltaTime, Vector3 objectPosition, Quaternion objectRotation);
        void ResetState();
    }
}
