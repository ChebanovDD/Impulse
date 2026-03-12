using Impulse.Interfaces;
using UnityEngine;

// ReSharper disable NotAccessedField.Local

namespace Impulse.ScriptableObjects
{
    public abstract class ShakePreset : ScriptableObject, IShakePreset
    {
        [TextArea]
        [SerializeField] private string _description;

        public abstract IShake CreateShake();
    }
}
