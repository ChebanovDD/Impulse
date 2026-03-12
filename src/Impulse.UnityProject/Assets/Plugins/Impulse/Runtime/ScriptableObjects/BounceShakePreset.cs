using Impulse.Interfaces;
using Impulse.Shakes;
using UnityEngine;

namespace Impulse.ScriptableObjects
{
    [CreateAssetMenu(fileName = "BounceShake", menuName = "EasyShake Presets/BounceShake", order = 0)]
    public class BounceShakePreset : ShakePreset
    {
        [SerializeField] private BounceShake.Params _params;

        public override IShake CreateShake() => new BounceShake(_params);
    }
}
