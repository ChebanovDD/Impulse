using System;
using Impulse.Enums;
using Impulse.Helpers;
using Impulse.Interfaces;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Impulse.Shakes
{
    public class PerlinShake : IShake
    {
        private readonly float _norm;

        private readonly Params _params;
        private readonly Envelope _envelope;
        private readonly Vector3? _sourcePosition;
        private readonly Vector2[] _seeds;

        private float _time;

        private Displacement _currentDisplacement;

        public PerlinShake(Params parameters, float maxAmplitude = 1.0f, Vector3? sourcePosition = null,
            EnvelopeControlMode strengthControlMode = EnvelopeControlMode.Auto)
        {
            _params = parameters;
            _envelope = new Envelope(_params.Envelope, maxAmplitude, strengthControlMode);
            _sourcePosition = sourcePosition;

            _norm = 0;
            _seeds = new Vector2[_params.NoiseModes.Length];

            for (var i = 0; i < _seeds.Length; i++)
            {
                _seeds[i] = Random.insideUnitCircle * 20;
                _norm += _params.NoiseModes[i].Amplitude;
            }
        }

        public bool IsFinished { get; private set; }

        public IShake Initialize(Transform objectTransform)
        {
            return this;
        }

        public Displacement Update(float deltaTime, Vector3 cameraPosition, Quaternion cameraRotation)
        {
            if (_envelope.IsFinished)
            {
                IsFinished = true;
                return _currentDisplacement;
            }

            _time += deltaTime;
            _envelope.Update(deltaTime);

            var displacement = Displacement.Zero;

            for (var i = 0; i < _params.NoiseModes.Length; i++)
            {
                displacement += _params.NoiseModes[i].Amplitude / _norm * SampleNoise(_seeds[i], _params.NoiseModes[i].Frequency);
            }

            _currentDisplacement = _envelope.Intensity * Displacement.Scale(displacement, _params.Strength);

            if (_sourcePosition is not null)
            {
                _currentDisplacement *= Attenuator.Strength(_params.Attenuation, _sourcePosition.Value, cameraPosition);
            }

            return _currentDisplacement;
        }

        public void ResetState()
        {
            _time = 0;
            _envelope.Reset();

            IsFinished = false;
        }

        private Displacement SampleNoise(Vector2 seed, float freq)
        {
            var position = new Vector3(
                Mathf.PerlinNoise(seed.x + _time * freq, seed.y),
                Mathf.PerlinNoise(seed.x, seed.y + _time * freq),
                Mathf.PerlinNoise(seed.x + _time * freq, seed.y + _time * freq));
            position -= Vector3.one * 0.5f;

            var rotation = new Vector3(
                Mathf.PerlinNoise(-seed.x - _time * freq, -seed.y),
                Mathf.PerlinNoise(-seed.x, -seed.y - _time * freq),
                Mathf.PerlinNoise(-seed.x - _time * freq, -seed.y - _time * freq));
            rotation -= Vector3.one * 0.5f;

            return new Displacement(position, rotation);
        }

        [Serializable]
        public class Params
        {
            [Tooltip("Strength of the shake for each axis.")]
            [field: SerializeField] public Displacement Strength { get; set; } = new(Vector3.zero, new Vector3(2, 2, 0.8f));

            [Tooltip("Layers of perlin noise with different frequencies.")]
            [field: SerializeField] public NoiseMode[] NoiseModes { get; set; } = { new(12, 1) };

            [Tooltip("Strength of the shake over time.")]
            [field: SerializeField] public Envelope.EnvelopeParams Envelope { get; set; }

            [Tooltip("How strength falls with distance from the shake source.")]
            [field: SerializeField] public Attenuator.StrengthAttenuationParams Attenuation { get; private set; }
        }

        [Serializable]
        public struct NoiseMode
        {
            public NoiseMode(float frequency, float amplitude)
            {
                Frequency = frequency;
                Amplitude = amplitude;
            }

            [Tooltip("Frequency multiplier for the noise.")]
            [field: SerializeField] public float Frequency { get; private set; }

            [Tooltip("Amplitude of the mode.")]
            [field: Range(0, 1)]
            [field: SerializeField] public float Amplitude { get; private set; }
        }
    }
}
