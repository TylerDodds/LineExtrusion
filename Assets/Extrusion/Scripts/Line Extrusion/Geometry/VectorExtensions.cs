using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// Vector extension methods.
    /// </summary>
    public static class VectorExtensions 
    {
        /// <summary>
        /// Averages a direction vector with another direction vector.
        /// </summary>
        /// <param name="initialDirectionVector">The first direction vector.</param>
        /// <param name="secondDirectionVector">The second direction vector.</param>
        /// <param name="fraction">The fraction to weight the second direction vector in the average.</param>
        public static Vector2 AverageDirectionVector(this Vector2 initialDirectionVector, Vector2 secondDirectionVector, float fraction)
        {
            var prevAngle = Mathf.Atan2(initialDirectionVector.y, initialDirectionVector.x);
            var nextAngle = Mathf.Atan2(secondDirectionVector.y, secondDirectionVector.x);
            var diff = nextAngle - prevAngle;
            if (diff > Mathf.PI) { diff -= Mathf.PI * 2f; }
            if (diff < -Mathf.PI) { diff += Mathf.PI * 2f; }
            var angle = prevAngle + diff * fraction;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }
    }
}