using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// A line point consisting of a two-dimensional vector and a parameter corresponding to the line parametrization.
    /// </summary>
    public partial struct LinePointStandard
    {
        /// <summary>
        /// The point's parameter along the line (from a line parametrization loosely like arclength).
        /// </summary>
        public float Parameter;

        /// <summary>
        /// The two-dimensional point.
        /// </summary>
        public Vector2 Point;

        /// <summary>
        /// Creates an instance of <see cref="LinePointStandard"/> with values for its public fields.
        /// </summary>
        /// <param name="parameter">The point's parameter along the line (from a line parametrization loosely like arclength).</param>
        /// <param name="point">The two-dimensional point.</param>
        public LinePointStandard(float parameter, Vector2 point)
        {
            Parameter = parameter;
            Point = point;
        }

        public override string ToString()
        {
            return string.Format("{0}, {1} {2}", Parameter, Point.x, Point.y);
        }
    }

    /// <summary>
    ///  A line point consisting of a two-dimensional vector and a parameter corresponding to the line parametrization, and uv parameters.
    /// </summary>
    public struct LinePointUV
    {
        /// <summary>
        /// The point's parameter along the line (from a line parametrization loosely like arclength).
        /// </summary>
        public float Parameter;

        /// <summary>
        /// The two-dimensional point.
        /// </summary>
        public Vector2 Point;

        /// <summary>
        /// The UV parameters of the point.
        /// </summary>
        public Vector2 UV;

        /// <summary>
        /// Creates an instance of <see cref="LinePointUV"/> with values for its public fields.
        /// </summary>
        /// <param name="parameter">The point's parameter along the line (from a line parametrization loosely like arclength).</param>
        /// <param name="point">The two-dimensional point.</param>
        /// <param name="uv">The UV parameters of the point.</param>
        public LinePointUV(float parameter, Vector2 point, Vector2 uv)
        {
            Parameter = parameter;
            Point = point;
            UV = uv;
        }

        /// <summary>
        /// Creates an instance of <see cref="LinePointUV"/> with values for its public field taken from an extruded point <see cref="SegmentwiseExtrudedPointListUV"/>.
        /// </summary>
        /// <param name="extrudedPoint">The extruded point fromm which to take <see cref="Parameter"/>, <see cref="Point"/> and <see cref="UV"/> values.</param>
        public LinePointUV(ExtrudedPointUV extrudedPoint) : this(extrudedPoint.Parameter, extrudedPoint.Point, extrudedPoint.UV)
        {
        }

        /// <summary>
        /// Average with another <see cref="LinePointUV"/>.
        /// </summary>
        /// <param name="other">The other point.</param>
        /// <param name="fractionOfOther">The fractional weight of the other point in the average.</param>
        public LinePointUV AverageWith(LinePointUV other, float fractionOfOther)
        {
            float fractionOfThis = 1f - fractionOfOther;
            return new LinePointUV(fractionOfThis * Parameter + fractionOfOther * other.Parameter, fractionOfThis * Point + fractionOfOther * other.Point, fractionOfThis * UV + fractionOfOther * other.UV);
        }

        public override string ToString()
        {
            return string.Format("{0} : {1} {2} : {3} {4}", Parameter, Point.x, Point.y, UV.x, UV.y);
        }
    }
}