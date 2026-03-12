using UnityEngine;

namespace Impulse
{
    /// <summary>
    /// Representation of translation and rotation. 
    /// </summary>
    [System.Serializable]
    public struct Displacement
    {
        [field: SerializeField] public Vector3 Position { get; private set; }
        [field: SerializeField] public Vector3 EulerAngles { get; private set; }

        public Displacement(Vector3 position) 
            : this(position, Vector3.zero)
        {
        }

        public Displacement(Vector3 position, Vector3 eulerAngles)
        {
            Position = position;
            EulerAngles = eulerAngles;
        }

        public static Displacement Zero => new(Vector3.zero, Vector3.zero);
        public Displacement Normalized => new(Position.normalized, EulerAngles.normalized);

        public Displacement ScaledBy(float posScale, float rotScale)
        {
            return new Displacement(Position * posScale, EulerAngles * rotScale);
        }

        public static Displacement Scale(Displacement a, Displacement b)
        {
            return new Displacement(Vector3.Scale(a.Position, b.Position), Vector3.Scale(b.EulerAngles, a.EulerAngles));
        }

        public static Displacement Lerp(Displacement a, Displacement b, float t)
        {
            return new Displacement(Vector3.Lerp(a.Position, b.Position, t), Vector3.Lerp(a.EulerAngles, b.EulerAngles, t));
        }

        public static Displacement InsideUnitSpheres()
        {
            return new Displacement(Random.insideUnitSphere, Random.insideUnitSphere);
        }

        public static Displacement operator +(Displacement a, Displacement b)
        {
            return new Displacement(a.Position + b.Position,
                b.EulerAngles + a.EulerAngles);
        }

        public static Displacement operator -(Displacement a, Displacement b)
        {
            return new Displacement(a.Position - b.Position,
                b.EulerAngles - a.EulerAngles);
        }

        public static Displacement operator -(Displacement displacement)
        {
            return new Displacement(-displacement.Position, -displacement.EulerAngles);
        }

        public static Displacement operator *(Displacement coords, float number)
        {
            return new Displacement(coords.Position * number, coords.EulerAngles * number);
        }

        public static Displacement operator *(float number, Displacement coords)
        {
            return coords * number;
        }

        public static Displacement operator /(Displacement coords, float number)
        {
            return new Displacement(coords.Position / number, coords.EulerAngles / number);
        }
    }
}