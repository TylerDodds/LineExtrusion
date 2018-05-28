using UnityEngine;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// An extruded point with uv parameters, line parameter, and related original line point segment indices.
    /// </summary>
    public struct ExtrudedPointUV
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
        /// The index of the original line segment start point that this point is extruded from.
        /// </summary>
        public int LinePointSegmentIndex;

        /// <summary>
        /// The index of another original line segment start point that this point is extruded from (if different from <see cref="LinePointSegmentIndex"/>).
        /// </summary>
        public int LinePointSegmentIndex2;

        /// <summary>
        /// Creates an instance of <see cref="SegmentwiseExtrudedPointListUV"/> taking the <see cref="LinePointUV.Parameter"/>, <see cref="LinePointUV.Point"/> and <see cref="LinePointUV.UV"/> values along with the given <see cref="LinePointSegmentIndex"/> and <see cref="LinePointSegmentIndex2"/> values.
        /// </summary>
        /// <param name="linePoint">The <see cref="LinePointUV>"/> line point instance, with Parameter, Point, and UV values.</param>
        /// <param name="linePointSegmentIndex">The index of the original line segment start point that this point is extruded from.</param>
        /// <param name="linePointSegmentIndex2">he index of another original line segment start point that this point is extruded from (if different from <see cref="LinePointSegmentIndex"/>).</param>
        public ExtrudedPointUV(LinePointUV linePoint, int linePointSegmentIndex, int linePointSegmentIndex2) : this(linePoint.Parameter, linePoint.Point, linePoint.UV, linePointSegmentIndex, linePointSegmentIndex2)
        {
            
        }

        /// <summary>
        /// Creates and instance of <see cref="SegmentwiseExtrudedPointListUV"/> from a <see cref="LinePointUV"/> Point and UV, as well as a normal vector, extrusion amount, line Parameter, and index used for <see cref="LinePointSegmentIndex"/> and <see cref="LinePointSegmentIndex2"/>.
        /// </summary>
        /// <param name="linePoint">The <see cref="LinePointUV>"/> line point instance, with Point, and UV values.</param>
        /// <param name="parameter">The point's parameter along the line (from a line parametrization loosely like arclength).</param>
        /// <param name="normal">The normal vector used in determining the extruded point's location.</param>
        /// <param name="extrusionAmount">The extrusion amount.</param>
        /// <param name="lineSegmentIndex">The index used for <see cref="LinePointSegmentIndex"/> and <see cref="LinePointSegmentIndex2"/></param>
        public ExtrudedPointUV(LinePointUV linePoint, float parameter, Vector2 normal, float extrusionAmount, int lineSegmentIndex) : this(parameter, linePoint.Point + normal * extrusionAmount, linePoint.UV, lineSegmentIndex, lineSegmentIndex)
        {

        }

        /// <summary>
        /// Creates a new instance of <see cref="SegmentwiseExtrudedPointListUV"/> with values for its public fields.
        /// </summary>
        /// <param name="parameter">The point's parameter along the line (from a line parametrization loosely like arclength).</param>
        /// <param name="point">The two-dimensional point.</param>
        /// <param name="uv">The UV parameters of the point.</param>
        /// <param name="linePointSegmentIndex">The index of the original line segment start point that this point is extruded from.</param>
        /// <param name="linePointSegmentIndex2">he index of another original line segment start point that this point is extruded from (if different from <see cref="LinePointSegmentIndex"/>).</param>
        public ExtrudedPointUV(float parameter, Vector2 point, Vector2 uv, int linePointSegmentIndex, int linePointSegmentIndex2)
        {
            Parameter = parameter;
            Point = point;
            UV = uv;
            LinePointSegmentIndex = linePointSegmentIndex;
            LinePointSegmentIndex2 = linePointSegmentIndex2;
        }


        public override string ToString()
        {
            return string.Format("{0} : {1} {2} : {3} {4} : {5}:{6}", Parameter, Point.x, Point.y, UV.x, UV.y, LinePointSegmentIndex, LinePointSegmentIndex2);
        }
    }
}
