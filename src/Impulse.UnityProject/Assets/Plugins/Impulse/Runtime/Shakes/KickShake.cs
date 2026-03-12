using System;
using Impulse.Helpers;
using Impulse.Interfaces;
using UnityEngine;

namespace Impulse.Shakes
{
    public class KickShake : IShake
    {
        private readonly bool _attenuateStrength;

        private readonly Params _pars;
        private readonly Vector3? _sourcePosition;

        private bool _release;

        private float _t;

        private Displacement _direction;
        private Displacement _currentWaypoint;
        private Displacement _previousWaypoint;
        private Displacement _currentDisplacement;

        public KickShake(Params parameters, Vector3 sourcePosition, bool attenuateStrength)
        {
            _pars = parameters;
            _sourcePosition = sourcePosition;
            _attenuateStrength = attenuateStrength;
        }

        public KickShake(Params parameters, Displacement direction)
        {
            _pars = parameters;
            _direction = direction.Normalized;
        }

        public bool IsFinished { get; private set; }

        public IShake Initialize(Transform objectTransform)
        {
            if (_sourcePosition is not null)
            {
                objectTransform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);

                _direction = Attenuator.Direction(_sourcePosition.Value, localPosition, localRotation);

                if (_attenuateStrength)
                {
                    _direction *= Attenuator.Strength(_pars.Attenuation, _sourcePosition.Value, localPosition);
                }
            }

            _currentWaypoint = Displacement.Scale(_direction, _pars.Strength);

            return this;
        }

        public Displacement Update(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            if (_t < 1)
            {
                return _release
                    ? Move(deltaTime, _pars.ReleaseTime, _pars.ReleaseCurve)
                    : Move(deltaTime, _pars.AttackTime, _pars.AttackCurve);
            }

            _previousWaypoint = _currentWaypoint;
            _currentDisplacement = _currentWaypoint;

            if (_release)
            {
                IsFinished = true;
            }
            else
            {
                _t = 0;
                _release = true;
                _currentWaypoint = Displacement.Zero;
            }

            return _currentDisplacement;
        }

        public void ResetState()
        {
            _t = 0;
            _release = false;
            _previousWaypoint = default;

            IsFinished = false;
        }

        private Displacement Move(float deltaTime, float duration, AnimationCurve curve)
        {
            if (duration > 0)
            {
                _t += deltaTime / duration;
            }
            else
            {
                _t = 1;
            }

            return Displacement.Lerp(_previousWaypoint, _currentWaypoint, curve.Evaluate(_t));
        }

        [Serializable]
        public class Params
        {
            [Tooltip("Strength of the shake for each axis.")]
            [field: SerializeField] public Displacement Strength { get; private set; } = new(Vector3.zero, Vector3.one);

            [Tooltip("How long it takes to move forward.")]
            [field: SerializeField] public float AttackTime { get; private set; } = 0.05f;
            [field: SerializeField] public AnimationCurve AttackCurve { get; private set; } = AnimationCurve.EaseInOut(0, 0, 1, 1);

            [Tooltip("How long it takes to move back.")]
            [field: SerializeField] public float ReleaseTime { get; private set; } = 0.2f;
            [field: SerializeField] public AnimationCurve ReleaseCurve { get; private set; } = AnimationCurve.EaseInOut(0, 0, 1, 1);

            [Tooltip("How strength falls with distance from the shake source.")]
            [field: SerializeField] public Attenuator.StrengthAttenuationParams Attenuation { get; private set; }
        }
    }
}
