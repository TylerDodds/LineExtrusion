using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// The information of a <see cref="LinePointUV"/> along with segment vector information.
    /// </summary>
    public struct LinePointUVAndSegment
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
        /// The other point int hte segment.
        /// </summary>
        public Vector2 OtherPoint;

        /// <summary>
        /// The segment tangent vector (normalized).
        /// </summary>        
        public Vector2 SegmentTangent;

        /// <summary>
        /// The segment normal vector (normalized).
        /// </summary>
        public Vector2 SegmentNormal;

        /// <summary>
        /// The segment length.
        /// </summary>
        public float SegmentLength;

        /// <summary>
        /// The <see cref="LinePointUV"/> of the start of the segment.
        /// </summary>
        public LinePointUV LinePoint
        {
            get
            {
                return new LinePointUV(Parameter, Point, UV);
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="LinePointUVAndSegment"/> based on the starting point <paramref name="linePoint"/> and filling in segment information based on the <paramref name="otherPoint"/>.
        /// </summary>
        /// <param name="linePoint">The line point which is the start of the line segment.</param>
        /// <param name="otherPoint">The other point in the line segment.</param>
        public LinePointUVAndSegment(LinePointUV linePoint, Vector2 otherPoint)
        {
            Parameter = linePoint.Parameter;
            Point = linePoint.Point;
            UV = linePoint.UV;
            OtherPoint = otherPoint;
            var segmentVector = otherPoint - linePoint.Point;
            SegmentTangent = segmentVector.normalized;
            SegmentNormal = NormalUtil.NormalFromTangent(SegmentTangent);
            SegmentLength = segmentVector.magnitude;
        }

        /// <summary>
        /// Creates an instance of <see cref="LinePointUVAndSegment"/> with <see cref="Parameter"/>, <see cref="Point"/>, and <see cref="UV"/> values from <paramref name="linePoint"/>, but with all other public fields explicitly specified.
        /// </summary>
        /// <param name="linePoint">The line point used for <see cref="Parameter"/>, <see cref="Point"/>, and <see cref="UV"/> values.</param>
        /// <param name="otherPoint">The other point in the segment.</param>
        /// <param name="tangent">The specified tangent vector of the segment.</param>
        /// <param name="normal">The specified normal vector of the segment.</param>
        /// <param name="length">The specified length of the segment.</param>
        public LinePointUVAndSegment(LinePointUV linePoint, Vector2 otherPoint, Vector2 tangent, Vector2 normal, float length)
        {
            Parameter = linePoint.Parameter;
            Point = linePoint.Point;
            UV = linePoint.UV;
            OtherPoint = otherPoint;
            SegmentTangent = tangent;
            SegmentNormal = normal;
            SegmentLength = length;
        }

        public override string ToString()
        {
            return string.Format("{0} & T {1} N {2} L {3}", LinePoint, SegmentTangent, SegmentNormal, SegmentLength);
        }
    }

}