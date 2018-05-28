using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// A two-dimensional vector with uv parameters.
    /// </summary>
    public struct Vector2WithUV
    {
        /// <summary>
        /// The two-dimensional point vector.
        /// </summary>
        public Vector2 Vector;

        /// <summary>
        /// The UV parameters (as x and y components).
        /// </summary>
        public Vector2 UV;


        /// <summary>
        /// Creates a new instance of <see cref="Vector2WithUV"/> with <see cref="Point"/> and <see cref="UV"/> values from a <paramref name="extrudedPointUV"/>.
        /// </summary>
        /// <param name="extrudedPointUV">The <see cref="ExtrudedPointUV"/> from which to take <see cref="Point"/> and <see cref="UV"/> values.</param>
        internal Vector2WithUV(ExtrudedPointUV extrudedPointUV) : this(extrudedPointUV.Point, extrudedPointUV.UV)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="Vector2WithUV"/> with <see cref="Point"/> and <see cref="UV"/> values from a <paramref name="linePointUV"/>.
        /// </summary>
        /// <param name="linePointUV">The <see cref="LinePointUV"/> from which to take <see cref="Point"/> and <see cref="UV"/> values.</param>
        internal Vector2WithUV(LinePointUV linePointUV) : this(linePointUV.Point, linePointUV.UV)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="Vector2WithUV"/> with <see cref="Point"/> and <see cref="UV"/> values from a <paramref name="intersectionPoint"/>.
        /// </summary>
        /// <param name="intersectionPoint">The <see cref="IntersectionPoint"/> from which to take <see cref="Point"/> and <see cref="UV"/> values.</param>
        internal Vector2WithUV(IntersectionPoint intersectionPoint) : this(intersectionPoint.Point, intersectionPoint.UV)
        {
        }

        /// <summary>
        /// Creates a new instance of <see cref="Vector2WithUV"/> with values for its public fields.
        /// </summary>
        /// <param name="vector"></param>
        /// <param name="uv"></param>
        private Vector2WithUV(Vector2 vector, Vector2 uv)
        {
            Vector = vector;
            UV = uv;
        }

        /// <summary>
        /// Average with another <see cref="Vector2WithUV"/>.
        /// </summary>
        /// <param name="other">The other <see cref="Vector2WithUV"/></param>
        /// <param name="fractionOfOther">The weight of the other vector in this average.</param>
        public Vector2WithUV AverageWith(Vector2WithUV other, float fractionOfOther)
        {
            float fractionOfThis = 1f - fractionOfOther;
            return new Vector2WithUV(fractionOfThis * Vector + fractionOfOther * other.Vector, fractionOfThis * UV + fractionOfOther * other.UV);
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2} {3}", Vector.x, Vector.y, UV.x, UV.y);
        }

        public override bool Equals(object obj)
        {
            if (obj is Vector2WithUV)
            {
                return this.Equals((Vector2WithUV)obj);
            }
            return false;
        }

        public bool Equals(Vector2WithUV other)
        {
            return Vector == other.Vector && UV == other.UV;
        }


        public override int GetHashCode()
        {
            return Vector.GetHashCode() ^ UV.GetHashCode();
        }
    }
}
