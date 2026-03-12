using System.Collections.Generic;
using Impulse.Interfaces;
using UnityEngine;

namespace Impulse
{
    public class ObjectShaker
    {
        private readonly float _strengthMultiplier;

        private readonly Transform _objectTransform;
        private readonly List<IShake> _activeShakes;

        public ObjectShaker(Transform objectTransform, float strengthMultiplier = 1.0f)
        {
            _activeShakes = new List<IShake>();
            _objectTransform = objectTransform;
            _strengthMultiplier = strengthMultiplier;
        }

        public void Shake(IShake shake)
        {
            _activeShakes.Add(shake.Initialize(_objectTransform));
        }

        public void Update()
        {
            var deltaTime = Time.deltaTime;
            var cameraDisplacement = Displacement.Zero;

            for (var i = _activeShakes.Count - 1; i >= 0; i--)
            {
                var shake = _activeShakes[i];

                if (shake.IsFinished)
                {
                    shake.ResetState();
                    _activeShakes.RemoveAt(i);
                }
                else
                {
                    _objectTransform.GetLocalPositionAndRotation(out var localPosition, out var localRotation);
                    cameraDisplacement += shake.Update(deltaTime, localPosition, localRotation);
                }
            }

            var newLocalPosition = _strengthMultiplier * cameraDisplacement.Position;
            var newLocalRotation = Quaternion.Euler(_strengthMultiplier * cameraDisplacement.EulerAngles);

            _objectTransform.SetLocalPositionAndRotation(newLocalPosition, newLocalRotation);
        }
    }
}
