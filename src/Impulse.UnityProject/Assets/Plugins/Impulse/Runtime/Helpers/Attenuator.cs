using Impulse.Enums;
using UnityEngine;

namespace Impulse.Helpers
{
    /// <summary>
    /// Contains methods for changing strength and direction of shakes depending on their position.
    /// </summary>
    public static class Attenuator
    {
        public static float Strength(StrengthAttenuationParams pars, Vector3 sourcePosition, Vector3 objectPosition)
        {
            var distance = Vector3.Scale(pars.AxesMultiplier, objectPosition - sourcePosition).sqrMagnitude;
            var strength = Mathf.Clamp01(1 - (distance - pars.ClippingDistance * pars.ClippingDistance) / pars.FalloffScale);

            return Power.Evaluate(strength, pars.FalloffDegree);
        }

        public static Displacement Direction(Vector3 sourcePosition, Vector3 objectPosition, Quaternion objectRotation)
        {
            var directionPosition = Quaternion.Inverse(objectRotation) * (objectPosition - sourcePosition).normalized;
            Vector3 directionEulerAngles = new(directionPosition.z, directionPosition.x, directionPosition.x);

            return new Displacement(directionPosition, directionEulerAngles);
        }

        [System.Serializable]
        public class StrengthAttenuationParams
        {
            [Tooltip("Radius in which shake doesn't lose strength.")]
            [field: SerializeField] public float ClippingDistance { get; private set; } = 10.0f;

            [Tooltip("How fast strength falls with distance.")]
            [field: SerializeField] public float FalloffScale { get; private set; } = 50.0f;

            [Tooltip("Power of the falloff function.")]
            [field: SerializeField] public Degree FalloffDegree { get; private set; } = Degree.Quadratic;

            [Tooltip("Contribution of each axis to distance. E. g. (1, 1, 0) for a 2D game in XY plane.")]
            [field: SerializeField] public Vector3 AxesMultiplier { get; private set; } = Vector3.one;
        }
    }
}
