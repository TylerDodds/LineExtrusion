using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// A point coming from the intersection of two extruded line points.
    /// </summary>
    public struct IntersectionPoint
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
        /// The index of the original line segment start point that the first point of a segment is extruded from.
        /// </summary>
        public int FirstPointSegmentIndex;

        /// <summary>
        /// The index of another original line segment start point that the first point of a segment is extruded from.
        /// </summary>
        public int FirstPointSegmentIndex2;

        /// <summary>
        /// The index of the original line segment start point that the second point of a segment is extruded from.
        /// </summary>
        public int SecondPointSegmentIndex;

        /// <summary>
        /// The index of another original line segment start point that the second point of a segment is extruded from.
        /// </summary>
        public int SecondPointSegmentIndex2;

        /// <summary>
        /// Creates an instance of <see cref="IntersectionPoint"/> with values for its public fields based on <paramref name="point"/>: <see cref="ExtrudedPointUV.Parameter"/>, <see cref="ExtrudedPointUV.Point"/> and <see cref="ExtrudedPointUV.UV"/> values.
        /// <see cref="FirstPointSegmentIndex"/> and <see cref="FirstPointSegmentIndex2"/> are based on <paramref name="pointIndices"/>.
        /// <see cref="SecondPointSegmentIndex"/> and <see cref="SecondPointSegmentIndex2"/> are based on <paramref name="otherPointIndices"/>.
        /// </summary>
        /// <param name="point">The <see cref="ExtrudedPointUV"/> supplying <see cref="Parameter"/>, <see cref="Point"/> and <see cref="UV"/> values.</param>
        /// <param name="pointIndices">The <see cref="ExtrudedPointUV"/> supplying <see cref="FirstPointSegmentIndex"/> and <see cref="FirstPointSegmentIndex2"/> values.</param>
        /// <param name="otherPointIndices">The <see cref="ExtrudedPointUV"/> supplying <see cref="SecondPointSegmentIndex"/> and <see cref="SecondPointSegmentIndex2"/> values.</param>
        public IntersectionPoint(ExtrudedPointUV point, ExtrudedPointUV pointIndices, ExtrudedPointUV otherPointIndices)
        {
            Parameter = point.Parameter;
            Point = point.Point;
            UV = point.UV;
            FirstPointSegmentIndex = pointIndices.LinePointSegmentIndex;
            FirstPointSegmentIndex2 = pointIndices.LinePointSegmentIndex2;
            SecondPointSegmentIndex = otherPointIndices.LinePointSegmentIndex;
            SecondPointSegmentIndex2 = otherPointIndices.LinePointSegmentIndex2;
        }

        public override string ToString()
        {
            return string.Format("{0} : {1} {2} : {3} {4} : {5} {6} : {7} {8}", Parameter, Point.x, Point.y, UV.x, UV.y, FirstPointSegmentIndex, FirstPointSegmentIndex2, SecondPointSegmentIndex, SecondPointSegmentIndex2);
        }
    }
}