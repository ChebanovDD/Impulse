using Impulse.Interfaces;
using Impulse.Shakes;
using UnityEngine;

namespace Impulse.ScriptableObjects
{
    [CreateAssetMenu(fileName = "KickShake", menuName = "EasyShake Presets/KickShake", order = 0)]
    public class KickShakePreset : ShakePreset
    {
        [SerializeField] private KickShake.Params _params;
        [SerializeField] private Displacement _direction;

        public override IShake CreateShake() => new KickShake(_params, _direction);
    }
}
