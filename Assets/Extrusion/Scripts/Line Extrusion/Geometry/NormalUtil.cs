using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// Utility methods for normal vectors.
    /// </summary>
    public static class NormalUtil
    {
        /// <summary>
        /// Gets a 2D normal vector from a 2D tangent vector.
        /// This defines the directionality between normal and tangent vectors.
        /// </summary>
        /// <param name="tangent">The tangent vector.</param>
        public static Vector2 NormalFromTangent(Vector2 tangent)
        {
            return new Vector2(-tangent.y, tangent.x);
        }
    }
}
