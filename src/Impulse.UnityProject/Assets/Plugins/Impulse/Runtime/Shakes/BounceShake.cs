using System;
using Impulse.Helpers;
using Impulse.Interfaces;
using UnityEngine;

namespace Impulse.Shakes
{
    public class BounceShake : IShake
    {
        private readonly Params _params;
        private readonly Vector3? _sourcePosition;
        private readonly Displacement _initialDirection;
        private readonly AnimationCurve _moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        private int _bounceIndex;
        
        private float _t;
        private float _attenuation = 1;
        
        private Displacement _direction;
        private Displacement _currentWaypoint;
        private Displacement _previousWaypoint;
        private Displacement _currentDisplacement;

        public BounceShake(Params parameters, Vector3? sourcePosition = null) 
            : this(parameters, Displacement.InsideUnitSpheres(), sourcePosition)
        {
        }

        public BounceShake(Params parameters, Displacement initialDirection, Vector3? sourcePosition = null)
        {
            _params = parameters;
            _direction = Displacement.Scale(initialDirection, _params.AxesMultiplier).Normalized;
            _sourcePosition = sourcePosition;
            _initialDirection = _direction;
        }

        public bool IsFinished { get; private set; }

        public IShake Initialize(Transform objectTransform)
        {
            _attenuation = _sourcePosition is null
                ? 1
                : Attenuator.Strength(_params.Attenuation, _sourcePosition.Value, objectTransform.position);
            _currentWaypoint = _attenuation * _direction.ScaledBy(_params.PositionStrength, _params.RotationStrength);

            return this;
        }

        public Displacement Update(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            if (_t < 1)
            {
                _t += deltaTime * _params.Frequency;

                if (_params.Frequency == 0)
                {
                    _t = 1;
                }

                _currentDisplacement = Displacement.Lerp(_previousWaypoint, _currentWaypoint, _moveCurve.Evaluate(_t));
                return _currentDisplacement;
            }

            _t = 0;
            _bounceIndex++;
            _previousWaypoint = _currentWaypoint;

            _currentDisplacement = _currentWaypoint;

            if (_bounceIndex > _params.NumBounces)
            {
                IsFinished = true;
                return _currentDisplacement;
            }

            _direction = -_direction + _params.Randomness *
                Displacement.Scale(Displacement.InsideUnitSpheres(), _params.AxesMultiplier).Normalized;
            _direction = _direction.Normalized;

            var decayValue = 1 - (float)_bounceIndex / _params.NumBounces;
            _currentWaypoint = decayValue * decayValue * _attenuation *
                               _direction.ScaledBy(_params.PositionStrength, _params.RotationStrength);

            return _currentDisplacement;
        }

        public void ResetState()
        {
            _t = 0;
            _bounceIndex = 0;
            _previousWaypoint = default;

            _direction = _initialDirection;

            IsFinished = false;
        }

        [Serializable]
        public class Params
        {
            [Tooltip("Strength of the shake for positional axes.")]
            [field: SerializeField] public float PositionStrength { get; set; } = 0.05f;

            [Tooltip("Strength of the shake for rotational axes.")]
            [field: SerializeField] public float RotationStrength { get; set; } = 0.1f;

            [Tooltip("Preferred direction of shaking.")]
            [field: SerializeField] public Displacement AxesMultiplier { get; private set; } = new(Vector2.one, Vector3.forward);

            [Tooltip("Frequency of shaking.")]
            [field: SerializeField] public float Frequency { get; set; } = 25.0f;

            [Tooltip("Number of vibrations before stop.")]
            [field: SerializeField] public int NumBounces { get; set; } = 5;

            [Tooltip("Randomness of motion.")]
            [field: Range(0, 1)]
            [field: SerializeField] public float Randomness { get; private set; } = 0.5f;

            [Tooltip("How strength falls with distance from the shake source.")]
            [field: SerializeField] public Attenuator.StrengthAttenuationParams Attenuation { get; private set; }
        }
    }
}
