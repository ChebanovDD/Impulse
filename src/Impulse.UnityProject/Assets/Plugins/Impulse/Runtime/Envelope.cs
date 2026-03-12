using System;
using System.Runtime.CompilerServices;
using Impulse.Enums;
using Impulse.Helpers;
using UnityEngine;

namespace Impulse
{
    /// <summary>
    /// Controls strength of the shake over time.
    /// </summary>
    public class Envelope
    {
        private readonly float _initialTargetAmplitude;

        private readonly EnvelopeParams _params;
        private readonly EnvelopeControlMode _controlMode;

        private bool _finishImmediately;
        private bool _finishWhenAmplitudeZero;

        private float _amplitude;
        private float _sustainEndTime;
        private float _targetAmplitude;

        private EnvelopeState _state;

        public Envelope(EnvelopeParams parameters, float initialTargetAmplitude, EnvelopeControlMode controlMode)
        {
            _params = parameters;
            _controlMode = controlMode;
            _initialTargetAmplitude = initialTargetAmplitude;

            SetTarget(initialTargetAmplitude);
        }

        public float Intensity { get; private set; }

        public bool IsFinished
        {
            get
            {
                if (_finishImmediately)
                {
                    return true;
                }

                return (_finishWhenAmplitudeZero || _controlMode == EnvelopeControlMode.Auto)
                       && _amplitude <= 0 && _targetAmplitude <= 0;
            }
        }

        public void Update(float deltaTime)
        {
            switch (_state)
            {
                case EnvelopeState.Increase:
                {
                    IncreaseShaking(deltaTime);
                    break;
                }
                case EnvelopeState.Decrease:
                {
                    DecreaseShaking(deltaTime);
                    break;
                }
                case EnvelopeState.Sustain:
                    SustainShaking();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            _amplitude = Mathf.Clamp01(_amplitude);
            Intensity = Power.Evaluate(_amplitude, _params.Degree);
        }

        public void SetTargetAmplitude(float value)
        {
            if (_controlMode == EnvelopeControlMode.Manual && _finishWhenAmplitudeZero == false)
            {
                SetTarget(value);
            }
        }

        public void Finish()
        {
            _finishWhenAmplitudeZero = true;
            SetTarget(0);
        }

        public void FinishImmediately()
        {
            _finishImmediately = true;
        }

        public void Reset()
        {
            _amplitude = 0;
            _sustainEndTime = 0;

            _finishImmediately = false;
            _finishWhenAmplitudeZero = false;

            SetTarget(_initialTargetAmplitude);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncreaseShaking(float deltaTime)
        {
            if (_params.Attack > 0)
            {
                _amplitude += deltaTime * _params.Attack;
            }

            if (_amplitude > _targetAmplitude || _params.Attack <= 0)
            {
                _amplitude = _targetAmplitude;
                _state = EnvelopeState.Sustain;

                if (_controlMode == EnvelopeControlMode.Auto)
                {
                    _sustainEndTime = Time.time + _params.Sustain;
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DecreaseShaking(float deltaTime)
        {
            if (_params.Decay > 0)
            {
                _amplitude -= deltaTime * _params.Decay;
            }

            if (_amplitude < _targetAmplitude || _params.Decay <= 0)
            {
                _amplitude = _targetAmplitude;
                _state = EnvelopeState.Sustain;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SustainShaking()
        {
            if (_controlMode == EnvelopeControlMode.Auto && Time.time > _sustainEndTime)
            {
                SetTarget(0);
            }
        }

        private void SetTarget(float value)
        {
            _targetAmplitude = Mathf.Clamp01(value);
            _state = _targetAmplitude > _amplitude ? EnvelopeState.Increase : EnvelopeState.Decrease;
        }

        [Serializable]
        public class EnvelopeParams
        {
            [Tooltip("How fast the amplitude increases.")]
            [field: SerializeField] public float Attack { get; private set; } = 10.0f;

            [Tooltip("How long in seconds the amplitude holds maximum value.")]
            [field: SerializeField] public float Sustain { get; private set; }

            [Tooltip("How fast the amplitude decreases.")]
            [field: SerializeField] public float Decay { get; set; } = 1.0f;

            [Tooltip("Power in which the amplitude is raised to get intensity.")]
            [field: SerializeField] public Degree Degree { get; private set; } = Degree.Cubic;
        }
    }
}
