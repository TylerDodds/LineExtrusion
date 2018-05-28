using System.Collections.Generic;

namespace BabyDinoHerd.Extrusion.Line.Geometry
{
    /// <summary>
    /// Helper methods for geometry conversion.
    /// </summary>
    public static class GeometryConversion
    {
        /// <summary>
        /// Convert a list of line points into a list of <see cref="LinePointUVAndSegment"/> with segment vector information.
        /// </summary>
        /// <param name="linePoints">Line points.</param>
        internal static List<LinePointUVAndSegment> ConvertLinePointsToForwardLineSegments(IList<LinePointUV> linePoints)
        {
            var forwardSegmentPoints = new List<LinePointUVAndSegment>(linePoints.Count);
            for (int i = 0; i <= linePoints.Count - 2; i++)
            {
                var currentLinePoint = linePoints[i];
                forwardSegmentPoints.Add(new LinePointUVAndSegment(currentLinePoint, linePoints[i + 1].Point));
            }
            return forwardSegmentPoints;
        }
    }
}