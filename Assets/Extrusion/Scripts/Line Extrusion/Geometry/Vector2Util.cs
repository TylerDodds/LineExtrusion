using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// Utility methods for <see cref="Vector2"/>.
    /// </summary>
    public static class Vector2Util
    {
        /// <summary>
        /// Two-dimensional cross-product analogue.
        /// </summary>
        /// <param name="vector1">First vector</param>
        /// <param name="vector2">Second vector</param>
        public static float Cross2D(Vector2 vector1, Vector2 vector2)
        {
            return vector1.x * vector2.y - vector1.y * vector2.x;
        }
    }
}