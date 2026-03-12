using Impulse.Interfaces;
using Impulse.Shakes;
using UnityEngine;

namespace Impulse.ScriptableObjects
{
    [CreateAssetMenu(fileName = "PerlinShake", menuName = "EasyShake Presets/PerlinShake", order = 0)]
    public class PerlinShakePreset : ShakePreset
    {
        [SerializeField] private PerlinShake.Params _params;

        public override IShake CreateShake() => new PerlinShake(_params);
    }
}
